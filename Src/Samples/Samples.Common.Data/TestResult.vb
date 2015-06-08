Imports Mercury.Messaging.Messages

''' <summary>
''' Represents the results of a test
''' </summary>
''' <remarks></remarks>
Public Class TestResult

    Public Sub New(ByVal faults As IList(Of Fault), ByVal elapsed As TimeFormat, ByVal agentCount As Integer)
        Me._faults = faults
        Me._elapsed = elapsed
        Me._agentCount = agentCount
    End Sub

    Private _agentCount As Integer

    Public ReadOnly Property AgentCount As Integer
        Get
            Return Me._agentCount
        End Get
    End Property

    Private _elapsed As TimeFormat

    Public ReadOnly Property Elapsed As TimeFormat
        Get
            Return Me._elapsed
        End Get
    End Property

    Private _faults As IList(Of Fault)

    Public ReadOnly Property Faults As IList(Of Fault)
        Get
            Return Me._faults
        End Get
    End Property

End Class