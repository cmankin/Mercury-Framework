

Public Class Ping

    Public Sub New()
        Me.New(String.Empty, String.Empty, String.Empty, 0)
    End Sub

    Public Sub New(ByVal senderAddress As String, ByVal data As String, ByVal ip As String, ByVal port As Integer)
        Me.SenderAddress = senderAddress
        Me.Data = data
        Me.IP = ip
        Me.Port = port
    End Sub

    Public Property SenderAddress As String
    Public Property IP As String
    Public Property Port As Integer
    Public Property Data As String

End Class