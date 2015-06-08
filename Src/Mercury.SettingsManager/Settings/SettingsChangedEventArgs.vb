
Namespace Settings

    ''' <summary>
    ''' Event data for the settings changed event.
    ''' </summary>
    ''' <remarks></remarks>
    Public Class SettingsChangedEventArgs

        Public Sub New(ByVal isCurrent As Boolean)
            Me._isCurrent = isCurrent
        End Sub

        Private _isCurrent As Boolean

        ''' <summary>
        ''' Gets a value indicating the state of the changed component.
        ''' </summary>
        ''' <returns>True if the state of the component is current; otherwise, false.</returns>
        ''' <remarks></remarks>
        Public ReadOnly Property IsCurrent As Boolean
            Get
                Return Me._isCurrent
            End Get
        End Property
    End Class

End Namespace