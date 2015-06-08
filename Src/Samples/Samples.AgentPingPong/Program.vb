Imports System.Threading
Imports Mercury.Messaging.Core
Imports Mercury.Messaging.Runtime
Imports Mercury.Messaging.Channels

Module Program

    Sub Main()
        Dim env As New RuntimeEnvironment("rt")

        Dim canClose As Boolean = False
        Do While Not (canClose)
            Console.WriteLine()
            Console.WriteLine("Start agents...")

            ' Start agents 
            Dim ref1 As LocalRef = env.Spawn(Of Paddle)()
            Dim ref2 As LocalRef = env.Spawn(Of Paddle)()

            Console.WriteLine("=>started agent 1 : {0}", ref1.ResId)
            Console.WriteLine("=>started agent 2: {0}", ref2.ResId)

            Console.WriteLine()
            Console.WriteLine()

            Dim startIndex As Integer = 0
            Do While startIndex = 0
                Console.WriteLine("Ping agent 1 or 2: ")
                startIndex = CInt(Console.ReadLine())
                If (startIndex <> 1) AndAlso (startIndex <> 2) Then _
                    startIndex = 0
            Loop

            Console.WriteLine("How many times?")
            Dim pings As Integer = CInt(Console.ReadLine())
            If pings > 0 Then
                Select Case startIndex
                    Case 1
                        ref1.Send(New Ping(ref2.ResId, pings - 1))
                    Case 2
                        ref2.Send(New Ping(ref1.ResId, pings - 1))
                End Select
            End If

            ' Poor man's blocking
            Dim flag As Boolean = True
            Do While flag
                Dim ref As LocalRef = env.GetRef(ref1.ResId)
                If ref IsNot Nothing Then
                    Thread.Sleep(500)
                Else
                    flag = False
                End If
            Loop

            '' Write out messages
            'Do While portMessages.ItemCount > 0
            '    Dim e As PreviewMessagePostEventArgs = TryCast(portMessages.Test(), PreviewMessagePostEventArgs)
            '    If e IsNot Nothing Then
            '        Dim msg1 As Ping = TryCast(e.Message, Ping)
            '        If msg1 IsNot Nothing Then
            '            Console.WriteLine("Agent {0} received Ping {1}.", e.ReceiverId, msg1.Counter)
            '        Else
            '            If TypeOf e.Message Is Pong Then
            '                Console.WriteLine("Agent {0} received Pong.", e.ReceiverId)
            '            ElseIf TypeOf e.Message Is Mercury.Messaging.Messages.Stop Then
            '                Console.WriteLine("Agent {0} received Stop message.", e.ReceiverId)
            '                Console.WriteLine("{0} is shutting down.", e.ReceiverId)
            '            End If
            '        End If
            '    End If
            'Loop

            ' Retry?
            Console.WriteLine("Retry? (y | n)")
            Dim retryString As String = CStr(Console.ReadLine()).ToLower()
            If Not (String.IsNullOrEmpty(retryString)) AndAlso (retryString = "n" OrElse retryString = "no") Then
                canClose = True
            End If

        Loop

        Console.WriteLine("Stopping...")
    End Sub

End Module