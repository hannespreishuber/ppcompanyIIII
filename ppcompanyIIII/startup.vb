
Imports Microsoft.Owin
Imports Owin
<Assembly: OwinStartupAttribute(GetType(Startup))>
Partial Public Class startup
    Public Sub Configuration(app As IAppBuilder)
        ConfigureAuth(app)
    End Sub
End Class
