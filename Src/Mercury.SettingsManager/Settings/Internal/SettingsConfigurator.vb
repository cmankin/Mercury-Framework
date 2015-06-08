Imports System.Text

Namespace Settings.Internal

    ''' <summary>
    ''' Provides configuration for a settings record.
    ''' </summary>
    ''' <remarks></remarks>
    Friend NotInheritable Class SettingsConfigurator

        Public Sub New(ByVal path As String, ByVal encoding As Encoding, ByVal root As IInternalSettingsRoot)
            Me._path = path
            Me._encoding = encoding
            Me._settingsRoot = root
        End Sub

        Private _settingsRoot As IInternalSettingsRoot

        Public ReadOnly Property SettingsRoot As IInternalSettingsRoot
            Get
                Return Me._settingsRoot
            End Get
        End Property

        Private _path As String

        Public ReadOnly Property Path As String
            Get
                Return Me._path
            End Get
        End Property

        Private _encoding As Encoding

        Public ReadOnly Property Encoding As Encoding
            Get
                Return Me._encoding
            End Get
        End Property

    End Class

End Namespace