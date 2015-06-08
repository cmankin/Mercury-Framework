Imports System.Threading
Imports Mercury

''' <summary>
''' A set of flags for server workers to evaluate.
''' </summary>
''' <remarks></remarks>
Public Class ServerState

    Private _workersQueryHandled As Boolean

    Public ReadOnly Property WorkersQueryHandled As Boolean
        Get
            Return Me._workersQueryHandled
        End Get
    End Property

    Private _stateLock As Object = New Object()

    Private _flag As MutexFlag = New MutexFlag(False)

    Public Function HandleQuery() As Boolean
        Me.IncrementAttemptedHandleCount()

        If Not (Me.WorkersQueryHandled) Then
            SyncLock Me._stateLock
                If Not (Me.WorkersQueryHandled) Then
                    Me._workersQueryHandled = True
                    Return True
                End If
            End SyncLock
        End If
        Return False
    End Function

    Public Sub SetWorkersQueryHandled(ByVal value As Boolean)
        Me._workersQueryHandled = value
    End Sub

    Private _attemptedHandleCount As Integer

    Public ReadOnly Property AttemptedHandleCount As Integer
        Get
            Return Me._attemptedHandleCount
        End Get
    End Property

    Private Sub IncrementAttemptedHandleCount()
        Interlocked.Increment(Me._attemptedHandleCount)
    End Sub

    Public Sub ResetAttemptedHandleCount()
        Interlocked.Exchange(Me._attemptedHandleCount, 0)
    End Sub

End Class