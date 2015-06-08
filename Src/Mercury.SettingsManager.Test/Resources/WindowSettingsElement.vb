Imports Mercury.Settings
Imports System.Windows
Imports System.Configuration

''' <summary>
''' Represents a configuration element containing window settings.
''' </summary>
''' <remarks></remarks>
Friend Class WindowSettingsElement
    Inherits ConfigurationElement
    Implements ISettingsHost

#Region "Constructors"

    Shared Sub New()
        ' Define properties
        WindowSettingsElement.windowStateProperty = New ConfigurationProperty("windowState", GetType(WindowState), Nothing, _
                                                               ConfigurationPropertyOptions.IsRequired)
        WindowSettingsElement.windowSizeProperty = New ConfigurationProperty("windowSize", GetType(Size), Nothing)
        WindowSettingsElement.windowLocationProperty = New ConfigurationProperty("windowLocation", GetType(Point), Nothing)

        ' Add to property collection
        WindowSettingsElement.ConfigurationPropertyCollection = New ConfigurationPropertyCollection()
        WindowSettingsElement.ConfigurationPropertyCollection.Add(WindowSettingsElement.windowStateProperty)
        WindowSettingsElement.ConfigurationPropertyCollection.Add(WindowSettingsElement.windowSizeProperty)
        WindowSettingsElement.ConfigurationPropertyCollection.Add(WindowSettingsElement.windowLocationProperty)
    End Sub

#End Region

#Region "Configuration Properties"

    ''' <summary>
    ''' A collection of configuration-element properties.
    ''' </summary>
    ''' <remarks></remarks>
    Friend Shared ConfigurationPropertyCollection As ConfigurationPropertyCollection
    ''' <summary>
    ''' A window size attribute.
    ''' </summary>
    ''' <remarks></remarks>
    Friend Shared windowSizeProperty As ConfigurationProperty
    ''' <summary>
    ''' A window state attribute.
    ''' </summary>
    ''' <remarks></remarks>
    Friend Shared windowStateProperty As ConfigurationProperty
    ''' <summary>
    ''' A window location attribute.
    ''' </summary>
    ''' <remarks></remarks>
    Friend Shared windowLocationProperty As ConfigurationProperty

    ''' <summary>
    ''' Gets or sets the window size setting.
    ''' </summary>
    <ConfigurationProperty("windowSize")> _
    Public Property WindowSize As Size
        Get
            Return CType(Me(WindowSettingsElement.windowSizeProperty), Size)
        End Get
        Set(value As Size)
            Me(WindowSettingsElement.windowSizeProperty) = value
            Me.Invalidate()
        End Set
    End Property

    ''' <summary>
    ''' Gets or sets the window state setting.
    ''' </summary>
    <ConfigurationProperty("windowState", IsRequired:=True)> _
    Public Property WindowState As WindowState
        Get
            Return CType(Me(WindowSettingsElement.windowStateProperty), WindowState)
        End Get
        Set(value As WindowState)
            Me(WindowSettingsElement.windowStateProperty) = value
            Me.Invalidate()
        End Set
    End Property

    ''' <summary>
    ''' Gets or sets the window location setting.
    ''' </summary>
    <ConfigurationProperty("windowLocation")> _
    Public Property WindowLocation As Point
        Get
            Return CType(Me(WindowSettingsElement.windowLocationProperty), Point)
        End Get
        Set(value As Point)
            Me(WindowSettingsElement.windowLocationProperty) = value
            Me.Invalidate()
        End Set
    End Property

    Protected Overrides ReadOnly Property Properties As System.Configuration.ConfigurationPropertyCollection
        Get
            Return WindowSettingsElement.ConfigurationPropertyCollection
        End Get
    End Property

    Public Overrides Function IsReadOnly() As Boolean
        Return False
    End Function

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