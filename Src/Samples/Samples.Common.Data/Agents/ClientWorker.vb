Imports System.Net
Imports System.Text
Imports Mercury
Imports Mercury.Messaging.Core
Imports Mercury.Messaging.Runtime
Imports Mercury.Messaging.Routing
Imports Mercury.Messaging.Messages
Imports Mercury.Messaging.Channels

''' <summary>
''' Generic worker agent
''' </summary>
''' <remarks></remarks>
Public Class ClientWorker
    Implements Agent

    Public Sub New(ByVal port As AgentPort)
        Dim environment As RuntimeEnvironment = port.Environment

        port.Receive(Of Ack)(
            Sub(msg)
                Console.WriteLine(String.Format("Received ack from {0} at {1}:{2}", msg.SenderAddress, msg.IP, msg.Port))
                Console.WriteLine()
            End Sub)

        port.Receive(Of Ping)(
            Sub(msg)
                Try
                    Dim messageData As String = msg.Data
                    Dim repeatCount As Integer = 0
                    If String.Equals(messageData, "-overflow [server]", StringComparison.OrdinalIgnoreCase) Then
                        Dim maxChars = 20970572
                        Dim builder = New StringBuilder(maxChars, maxChars)
                        Dim arr = "a".ToCharArray()
                        builder.Append(arr(0), maxChars)
                        messageData = builder.ToString()
                    ElseIf String.Equals(messageData, "-largemsg [server]", StringComparison.OrdinalIgnoreCase) Then
                        Dim maxChars = 20000000
                        Dim builder = New StringBuilder(maxChars, maxChars)
                        Dim arr = "a".ToCharArray()
                        builder.Append(arr(0), maxChars)
                        messageData = builder.ToString()
                    ElseIf messageData.Contains(" /r") Then
                        Dim startIndex = InStr(messageData, " /r")
                        Dim endIndex = messageData.Length
                        If startIndex < endIndex Then
                            Dim test = Mid(messageData, startIndex + 3, (endIndex + 1) - startIndex)
                            test = Trim(test)
                            If IsNumeric(test) Then
                                repeatCount = Math.Abs(CInt(test))
                                messageData = Left(messageData, startIndex - 1)
                            End If
                        End If
                    End If

                    Dim remoteEngine = TryCast(port.Environment.RoutingEngine, IRemoteRouting)
                    If remoteEngine IsNot Nothing Then
                        remoteEngine.ExpirePostedOperations()
                    End If

                    Dim newPing = New Ping(port.Id, messageData, environment.EndPoint.Address.ToString(), environment.EndPoint.Port)

                    ' Find sender address
                    Dim id As String = FindWorker(msg.SenderAddress)
                    ' Get channel and send ping
                    Dim channel As RemotingChannel = TryCast(environment.GetRef(id, msg.IP, msg.Port), RemotingChannel)
                    If (channel IsNot Nothing) Then
                        Console.WriteLine(String.Format("Sending ping to {0} at {1}:{2}", id, msg.IP, msg.Port))
                        channel.PostErrors = True
                        channel.ErrorHandler = AddressOf _HandleErrors
                        If repeatCount > 0 Then
                            Dim pingList = New List(Of Ping)(repeatCount)
                            For i As Integer = 0 To repeatCount - 1
                                pingList.Add(newPing)
                            Next
                            channel.Send(Of IList(Of Ping))(pingList)
                        Else
                            channel.Send(Of Ping)(newPing)
                        End If
                        Console.WriteLine("Operation ID:{0}", channel.LastOperationId)
                    End If
                    Console.WriteLine()
                Catch ex As Exception
                    Console.WriteLine(ex.Message)
                End Try
            End Sub)

        port.Receive(Of GetWorkers)(
            Sub(msg)
                Dim sendType As Type = Type.GetType(msg.DestinationTypeName)
                Dim endPoint As IPEndPoint = New IPEndPoint(IPAddress.Parse(msg.IP), msg.Port)
                Dim channel As LocalRef = environment.GetRef(sendType, endPoint, 30.Seconds())

                ' Send
                Dim newMessage As GetWorkers = New GetWorkers(environment.EndPoint, port.Id)
                Dim fut As Future(Of String()) = CType(channel, InternalResource).GetFuture(Of String())()
                fut.Send(newMessage)

                ' Get workers
                Me._workers = fut.Get
                'Dim req As Request(Of GetWorkers) =
                '    New RequestBase(Of GetWorkers)(Nothing, newMessage, Guid.NewGuid().ToString(),
                '                                   New Uri(port.Id), endPoint, Nothing)
                'channel.Send(Of Request(Of GetWorkers))(req)
            End Sub)

        port.Receive(Of SendWorkers)(
            Sub(msg)
                Me._workers = msg.Workers
            End Sub)
    End Sub

    Private _workers As String()

    Public ReadOnly Property Workers As String()
        Get
            Return Me._workers
        End Get
    End Property

    Protected Function FindWorker(ByVal id As String) As String
        If (IsNumeric(id)) Then
            Dim index As Integer = CInt(id)
            If (Me.Workers IsNot Nothing) Then
                If (index > -1 AndAlso index < Me.Workers.Length) Then _
                    Return Me.Workers(index)
            End If
        End If
        Return id
    End Function

    Protected Sub _HandleErrors(ByVal operationId As Integer, ByVal ex As Exception)
        If (ex IsNot Nothing) Then
            Console.WriteLine("Error occurred on operation {0}: {1}", operationId, ex)
        End If
    End Sub

End Class