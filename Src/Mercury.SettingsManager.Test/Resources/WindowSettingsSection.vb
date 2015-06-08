Imports System.Windows
Imports System.Configuration
Imports Mercury.Settings

''' <summary>
''' Represents a configuration section containing window settings.
''' </summary>
''' <remarks></remarks>
Friend Class WindowSettingsSection
    Inherits ConfigurationSection
    Implements ISettingsHost

#Region "Constructors"

    Shared Sub New()
        ' Define properties
        WindowSettingsSection.MainWindowSettingsProperty = _
            New ConfigurationProperty("mainWindowSettings", GetType(WindowSettingsElement), Nothing, _
                                      ConfigurationPropertyOptions.IsRequired)
        ' Add to property collection
        WindowSettingsSection.ConfigurationPropertyCollection = New ConfigurationPropertyCollection()
        WindowSettingsSection.ConfigurationPropertyCollection.Add(WindowSettingsSection.MainWindowSettingsProperty)
    End Sub

#End Region

#Region "Configuration Properties"

    ''' <summary>
    ''' A collection of configuration-element properties.
    ''' </summary>
    ''' <remarks></remarks>
    Friend Shared ConfigurationPropertyCollection As ConfigurationPropertyCollection
    ''' <summary>
    ''' A window settings element for the primary application window.
    ''' </summary>
    ''' <remarks></remarks>
    Friend Shared MainWindowSettingsProperty As ConfigurationProperty

    ''' <summary>
    ''' Gets or sets the settings element for the primary application window.
    ''' </summary>
    <ConfigurationProperty("mainWindowSettings")> _
    Public Property MainWindowSettings As WindowSettingsElement
        Get
            Return CType(Me(WindowSettingsSection.MainWindowSettingsProperty), WindowSettingsElement)
        End Get
        Set(value As WindowSettingsElement)
            Me(WindowSettingsSection.MainWindowSettingsProperty) = value
            Me.Invalidate()
        End Set
    End Property

    Protected Overrides ReadOnly Property Properties As System.Configuration.ConfigurationPropertyCollection
        Get
            Return WindowSettingsSection.ConfigurationPropertyCollection
        End Get
    End Property

#End Region

#Region "ISettingsHost"

    Public Sub Invalidate() Implements Settings.ISettingsHost.Invalidate
        Me._isCurrent = False
        OnSettingsChanged()
    End Sub

    Private _isCurrent As Boolean = False

    Public ReadOnly Property IsCurrent As Boolean Implements Settings.ISettingsHost.IsCurrent
        Get
            Return Me._isCurrent
        End Get
    End Property

    Public Sub Reset1() Implements ISettingsHost.Reset
        Me._isCurrent = True
        OnSettingsChanged()
    End Sub

    Public Sub Deserialize(reader As System.Xml.XmlReader, serializeCollectionKey As Boolean) Implements Settings.ISettingsHost.Deserialize
        Me.DeserializeElement(reader, serializeCollectionKey)
    End Sub

    Public Function Serialize(writer As System.Xml.XmlWriter, elementName As String) As Boolean Implements Settings.ISettingsHost.Serialize
        Return Me.SerializeToXmlElement(writer, elementName)
    End Function

    Public Event SettingsChanged(sender As Object, e As SettingsChangedEventArgs) Implements Settings.ISettingsHost.SettingsChanged

    Protected Sub OnSettingsChanged()
        RaiseEvent SettingsChanged(Me, New SettingsChangedEventArgs(Me.IsCurrent))
    End Sub

    Protected Sub ChildSettingsChangedHandler(ByVal sender As Object, ByVal e As SettingsChangedEventArgs)
        If e.IsCurrent Then
            Me.Reset1()
        Else
            Me.Invalidate()
        End If
    End Sub

#End Region

End Class