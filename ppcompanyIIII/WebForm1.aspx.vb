Imports System.Net.Http.Headers
Imports System.Security.Claims
Imports System.Threading.Tasks
Imports Microsoft.Graph
Imports Microsoft.Owin.Security
Imports Microsoft.Owin.Security.OpenIdConnect

Public Class WebForm1
    Inherits System.Web.UI.Page

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        FormsAuthentication.SetAuthCookie("username", True)
        Response.Write(User.Identity.Name)
        If Request.IsAuthenticated Then
            Label1.Text = ClaimsPrincipal.Current.FindFirst(ClaimTypes.Email).Value
            Dim x = User.Identity.Name
        End If
    End Sub

    Protected Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        If (Request.IsAuthenticated = False) Then

            Request.GetOwinContext().Authentication.Challenge(
                    New AuthenticationProperties With {.RedirectUri = "/"},
OpenIdConnectAuthenticationDefaults.AuthenticationType)

        Else

        End If
    End Sub

    Protected Function Button3_ClickAsync(sender As Object, e As EventArgs) Handles Button3.Click
        'https://tasks.office.com/ppedv.de/de-DE/Home/Planner/#/plantaskboard?groupId=145db263-90e3-4cae-b401-0381b06ff2b5&planId=Frczk8wfmEGL8xL0X1dfOZYADb1w
        'https://graph.microsoft.com/v1.0/planner/plans/Frczk8wfmEGL8xL0X1dfOZYADb1w/buckets
        '/v1.0/planner/plans/Frczk8wfmEGL8xL0X1dfOZYADb1w/tasks 
        '"BucketId""pu2t9gaDEk2DKo9N64nnDZYALL-B""
        '"BucketId":"53hpjCFHU0qpyNoz9m-mCJYABETB"


        RegisterAsyncTask(New PageAsyncTask(Function() myGraphAsync()))
        'RegisterAsyncTask(New PageAsyncTask(New Func(Of Task)(Async Function()
        '                                                          Await myGraphAsync()
        '                                                      End Function)))


        Page.ExecuteRegisteredAsyncTasks()


    End Function


    Private Async Function myGraphAsync() As Threading.Tasks.Task(Of IAsyncResult)

        Dim graphClient = GraphHelper.GetAuthenticatedClient




        Dim ich = Await graphClient.Me.Request().GetAsync()

        Dim Events = Await graphClient.Me.Events.Request().Select("subject,organizer,start,end").OrderBy("createdDateTime DESC").GetAsync()

        Dim res = Await graphClient.Planner.Plans.Item("Frczk8wfmEGL8xL0X1dfOZYADb1w").Tasks.Request.GetAsync


        Dim myTask = New PlannerTask()
        myTask.PlanId = "Frczk8wfmEGL8xL0X1dfOZYADb1w" ' //a valid planner id
        myTask.BucketId = "pu2t9gaDEk2DKo9N64nnDZYALL-B"

        myTask.Title = "Ganz neue Aufgabe"
        Dim createdTask = Await graphClient.Planner.Tasks.Request().AddAsync(myTask)
    End Function
End Class