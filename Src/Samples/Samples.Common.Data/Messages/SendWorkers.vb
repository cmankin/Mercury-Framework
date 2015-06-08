Imports System.Net

Public Class SendWorkers

    Public Sub New()
    End Sub

    Public Sub New(ByVal workers As String())
        Me.Workers = workers
    End Sub

    Public Property Workers As String()

End Class