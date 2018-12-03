Imports Microsoft.Graph
Imports Microsoft.Identity.Client
Imports System.Net.Http.Headers
Imports System.Security.Claims
Imports System.Threading.Tasks

Module GraphHelper
    Async Function GetUserDetailsAsync(ByVal accessToken As String) As Task(Of User)
        Dim graphClient = New GraphServiceClient(New DelegateAuthenticationProvider(Async Function(requestMessage)
                                                                                        requestMessage.Headers.Authorization = New AuthenticationHeaderValue("Bearer", accessToken)
                                                                                    End Function))
        Return Await graphClient.[Me].Request().GetAsync()
    End Function
    Public Function GetAuthenticatedClient() As GraphServiceClient
        Dim gsc = New DelegateAuthenticationProvider(Async Function(requestMessage)
                                                         Dim signedInUserId As String = ClaimsPrincipal.Current.FindFirst(ClaimTypes.NameIdentifier).Value
                                                         Dim tokenStore As SessionTokenStore = New SessionTokenStore(signedInUserId, New HttpContextWrapper(HttpContext.Current))
                                                         Dim idClient = New ConfidentialClientApplication(startup.appId, startup.redirectUri,
                                                                                                                              New ClientCredential(startup.appSecret),
                                                                                                                              tokenStore.GetMsalCacheInstance(), Nothing)
                                                         Dim accounts = Await idClient.GetAccountsAsync()
                                                         Dim result = Await idClient.AcquireTokenSilentAsync(startup.graphScopes.Split(" "c), accounts.FirstOrDefault())
                                                         requestMessage.Headers.Authorization = New AuthenticationHeaderValue("Bearer", result.AccessToken)
                                                     End Function)
        Return New GraphServiceClient(gsc)
    End Function

    Public Async Function GetEventsAsync() As Task(Of IEnumerable(Of [Event]))
        Dim graphClient = GetAuthenticatedClient()
        Dim events = Await graphClient.[Me].Events.Request().[Select]("subject,organizer,start,end").OrderBy("createdDateTime DESC").GetAsync()
        Return events.CurrentPage
    End Function
End Module
