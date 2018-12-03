Imports Microsoft.Identity.Client
Imports Microsoft.IdentityModel.Protocols.OpenIdConnect
Imports Microsoft.IdentityModel.Tokens
Imports Microsoft.Owin.Security
Imports Microsoft.Owin.Security.Cookies
Imports Microsoft.Owin.Security.Notifications
Imports Microsoft.Owin.Security.OpenIdConnect
Imports Owin
Imports System.Configuration
Imports System.IdentityModel.Claims
Imports System.Threading.Tasks
Imports System.Web


Partial Public Class Startup
    Public Shared appId As String = ConfigurationManager.AppSettings("ida:AppId")
    Public Shared appSecret As String = ConfigurationManager.AppSettings("ida:AppSecret")
    Public Shared redirectUri As String = ConfigurationManager.AppSettings("ida:RedirectUri")
    Public Shared graphScopes As String = ConfigurationManager.AppSettings("ida:AppScopes")

    Public Sub ConfigureAuth(ByVal app As IAppBuilder)
        app.SetDefaultSignInAsAuthenticationType(CookieAuthenticationDefaults.AuthenticationType)
        app.UseCookieAuthentication(New CookieAuthenticationOptions())
        app.UseOpenIdConnectAuthentication(New OpenIdConnectAuthenticationOptions With {
            .ClientId = appId,
            .Authority = "https://login.microsoftonline.com/common/v2.0",
            .Scope = $"openid email profile offline_access {graphScopes}",
            .RedirectUri = redirectUri,
            .PostLogoutRedirectUri = redirectUri,
            .TokenValidationParameters = New TokenValidationParameters With {
                .ValidateIssuer = False
            },
            .Notifications = New OpenIdConnectAuthenticationNotifications With {
                .AuthenticationFailed = AddressOf OnAuthenticationFailedAsync,
                .AuthorizationCodeReceived = AddressOf OnAuthorizationCodeReceivedAsync
            }
        })
    End Sub

    Private Shared Function OnAuthenticationFailedAsync(ByVal notification As AuthenticationFailedNotification(Of OpenIdConnectMessage, OpenIdConnectAuthenticationOptions)) As Task
        notification.HandleResponse()
        Dim redirect As String = $"/Home/Error?message={notification.Exception.Message}"

        If notification.ProtocolMessage IsNot Nothing AndAlso Not String.IsNullOrEmpty(notification.ProtocolMessage.ErrorDescription) Then
            redirect += $"&debug={notification.ProtocolMessage.ErrorDescription}"
        End If

        notification.Response.Redirect(redirect)
        Return Task.FromResult(0)
    End Function

    Private Async Function OnAuthorizationCodeReceivedAsync(ByVal notification As AuthorizationCodeReceivedNotification) As Task
        Dim signedInUserId As String = notification.AuthenticationTicket.Identity.FindFirst(ClaimTypes.NameIdentifier).Value
        Dim ctx = TryCast(notification.OwinContext.Environment("System.Web.HttpContextBase"), HttpContextBase)
        Dim s = ctx.Session

        Dim tokenStore As SessionTokenStore = New SessionTokenStore(signedInUserId, ctx)
        Dim idClient = New ConfidentialClientApplication(appId, redirectUri, New ClientCredential(appSecret), tokenStore.GetMsalCacheInstance(), Nothing)

        Try
            Dim scopes As String() = graphScopes.Split(" "c)
            Dim result = Await idClient.AcquireTokenByAuthorizationCodeAsync(notification.Code, scopes)
            Dim userDetails = Await GraphHelper.GetUserDetailsAsync(result.AccessToken)
            Dim cachedUser = New CachedUser() With {
                .DisplayName = userDetails.DisplayName,
                .Email = If(String.IsNullOrEmpty(userDetails.Mail), userDetails.UserPrincipalName, userDetails.Mail),
                .Avatar = String.Empty
            }
            tokenStore.SaveUserDetails(cachedUser)
        Catch ex As MsalException
            Dim message As String = "AcquireTokenByAuthorizationCodeAsync threw an exception"
            notification.HandleResponse()
            notification.Response.Redirect($"/Home/Error?message={message}&debug={ex.Message}")
        Catch ex As Microsoft.Graph.ServiceException
            Dim message As String = "GetUserDetailsAsync threw an exception"
            notification.HandleResponse()
            notification.Response.Redirect($"/Home/Error?message={message}&debug={ex.Message}")
        End Try
    End Function
End Class
