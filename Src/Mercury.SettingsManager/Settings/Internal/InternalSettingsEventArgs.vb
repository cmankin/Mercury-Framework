
Namespace Settings.Internal

    ''' <summary>
    ''' Describes event data for some internal settings events.
    ''' </summary>
    ''' <remarks></remarks>
    Public Class InternalSettingsEventArgs

        Public Sub New(ByVal settingsPath As String)
            Me._settingsPath = settingsPath
        End Sub

        Private _settingsPath As String

        ''' <summary>
        ''' Gets or sets the settings path related to the event.
        ''' </summary>
        ''' <returns>The settings path related to the event.</returns>
        ''' <remarks></remarks>
        Public Property SettingsPath As String
            Get
                Return Me._settingsPath
            End Get
            Set(value As String)
                Me._settingsPath = value
            End Set
        End Property

    End Class

End Namespace