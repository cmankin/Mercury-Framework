Imports System.Net

Public Class GetWorkers

    Public Sub New()
    End Sub

    Public Sub New(ByVal endPoint As IPEndPoint, ByVal destination As String)
        Me.New(endPoint.Address.ToString(), endPoint.Port, destination, String.Empty)
    End Sub

    Public Sub New(ByVal endPoint As IPEndPoint, ByVal destination As String, ByVal destinationType As Type)
        Me.New(endPoint.Address.ToString(), endPoint.Port, destination, destinationType.AssemblyQualifiedName)
    End Sub

    Public Sub New(ByVal ip As String, ByVal port As Integer, ByVal destination As String, ByVal destinationTypeName As String)
        Me.IP = ip
        Me.Port = port
        Me.Destination = destination
        If Not (String.IsNullOrEmpty(destinationTypeName)) Then _
            Me.DestinationTypeName = destinationTypeName
    End Sub

    Public Property IP As String
    Public Property Port As Integer
    Public Property Destination As String
    Public Property DestinationTypeName As String

End Class