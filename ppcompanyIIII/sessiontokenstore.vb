Imports Microsoft.Identity.Client
Imports Newtonsoft.Json
Imports ppcompanyIIII
Imports System.Threading
Imports System.Web

Public Class CachedUser
    Public Property DisplayName As String
    Public Property Email As String
    Public Property Avatar As String
End Class

Public Class SessionTokenStore
    Private Shared sessionLock As ReaderWriterLockSlim = New ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion)
    Private ReadOnly userId As String = String.Empty
    Private ReadOnly cacheId As String = String.Empty
    Private ReadOnly cachedUserId As String = String.Empty
    Private httpContext As HttpContextBase = Nothing
    Private tokenCache As TokenCache = New TokenCache()

    Public Sub New(ByVal userId As String, ByVal httpContext As HttpContextBase)
        Me.userId = userId
        cacheId = $"{userId}_TokenCache"
        cachedUserId = $"{userId}_UserCache"
        Me.httpContext = httpContext
        Load()
    End Sub

    Public Function GetMsalCacheInstance() As TokenCache
        tokenCache.SetBeforeAccess(AddressOf BeforeAccessNotification)
        tokenCache.SetAfterAccess(AddressOf AfterAccessNotification)
        Load()
        Return tokenCache
    End Function

    Public Function HasData() As Boolean
        Return (httpContext.Session(cacheId) IsNot Nothing AndAlso (CType(httpContext.Session(cacheId), Byte())).Length > 0)
    End Function

    Public Sub Clear()
        httpContext.Session.Remove(cacheId)
    End Sub

    Private Sub Load()
        sessionLock.EnterReadLock()
        tokenCache.Deserialize(CType(httpContext.Session(cacheId), Byte()))
        sessionLock.ExitReadLock()
    End Sub

    Private Sub Persist()
        sessionLock.EnterReadLock()
        tokenCache.HasStateChanged = False
        httpContext.Session(cacheId) = tokenCache.Serialize()
        sessionLock.ExitReadLock()
    End Sub

    Private Sub BeforeAccessNotification(ByVal args As TokenCacheNotificationArgs)
        Load()
    End Sub

    Private Sub AfterAccessNotification(ByVal args As TokenCacheNotificationArgs)
        If tokenCache.HasStateChanged Then
            Persist()
        End If
    End Sub

    Public Sub SaveUserDetails(ByVal user As CachedUser)
        sessionLock.EnterReadLock()
        httpContext.Session(cachedUserId) = JsonConvert.SerializeObject(user)
        sessionLock.ExitReadLock()
    End Sub

    Public Function GetUserDetails() As CachedUser
        sessionLock.EnterReadLock()
        Dim cachedUser = JsonConvert.DeserializeObject(Of CachedUser)(CStr(httpContext.Session(cachedUserId)))
        sessionLock.ExitReadLock()
        Return cachedUser
    End Function

End Class
