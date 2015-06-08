
''' <summary>
''' Generalized time formatting to string.
''' </summary>
''' <remarks></remarks>
Public Class TimeFormat

    Public Sub New(ByVal time As TimeSpan)
        Me._time = time
    End Sub

    Private _time As TimeSpan

    Public ReadOnly Property Time As TimeSpan
        Get
            Return Me._time
        End Get
    End Property

    Public Overrides Function ToString() As String
        Dim current As String = String.Empty

        If (Me.Time.Days > 0) Then _
            current += String.Format("{0} Days ", Me.Time.Days)
        If (Me.Time.Hours > 0) Then _
            current += String.Format("{0} Hours ", Me.Time.Hours)
        If (Me.Time.Minutes > 0) Then _
            current += String.Format("{0} Minutes ", Me.Time.Minutes)
        If (Me.Time.Seconds > 0) Then _
            current += String.Format("{0} Seconds ", Me.Time.Seconds)
        If (Me.Time.Milliseconds > 0) Then _
            current += String.Format("{0} Milliseconds ", Me.Time.Milliseconds)

        Return current
    End Function

End Class