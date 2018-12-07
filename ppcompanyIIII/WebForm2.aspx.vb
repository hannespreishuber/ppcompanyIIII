Imports Microsoft.Owin.Security
Imports Microsoft.Owin.Security.OpenIdConnect

Public Class WebForm2
    Inherits System.Web.UI.Page

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load

    End Sub
    Protected Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        If (Request.IsAuthenticated = False) Then

            Request.GetOwinContext().Authentication.Challenge(
                    New AuthenticationProperties With {.RedirectUri = "/webform2"},
OpenIdConnectAuthenticationDefaults.AuthenticationType)

        Else

        End If
    End Sub
End Class