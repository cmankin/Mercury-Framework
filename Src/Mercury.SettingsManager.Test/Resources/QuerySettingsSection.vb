Imports System.Configuration
Imports Mercury.Settings

''' <summary>
''' Represents a configuration section containing query settings.
''' </summary>
''' <remarks></remarks>
Friend Class QuerySettingsSection
    Inherits ConfigurationSection
    Implements ISettingsHost

#Region "Constructors"

    Shared Sub New()
        ' Add collection
        QuerySettingsSection.SettingsProperty = New ConfigurationProperty("", GetType(QuerySettingsElementCollection),
                                                            Nothing, ConfigurationPropertyOptions.IsDefaultCollection)

        ' Add to property collection
        QuerySettingsSection.ConfigurationPropertyCollection = New ConfigurationPropertyCollection()
        QuerySettingsSection.ConfigurationPropertyCollection.Add(QuerySettingsSection.SettingsProperty)
    End Sub

#End Region

#Region "Configuration Properties"

    ''' <summary>
    ''' A collection of configuration-element properties.
    ''' </summary>
    ''' <remarks></remarks>
    Friend Shared ConfigurationPropertyCollection As ConfigurationPropertyCollection

    Friend Shared SettingsProperty As ConfigurationProperty

    ''' <summary>
    ''' Gets or sets the settings collection.
    ''' </summary>
    <ConfigurationProperty("", IsDefaultCollection:=True)>
    Public Property Settings As QuerySettingsElementCollection
        Get
            Return CType(Me(QuerySettingsSection.SettingsProperty), QuerySettingsElementCollection)
        End Get
        Set(value As QuerySettingsElementCollection)
            Dim current As QuerySettingsElementCollection =
                CType(Me(QuerySettingsSection.SettingsProperty), QuerySettingsElementCollection)
            If current IsNot Nothing Then _
                RemoveHandler current.SettingsChanged, AddressOf ChildSettingsChangedHandler

            Me(QuerySettingsSection.SettingsProperty) = value
            AddHandler value.SettingsChanged, AddressOf ChildSettingsChangedHandler
            Me.Invalidate()
        End Set
    End Property

    Protected Overrides ReadOnly Property Properties As System.Configuration.ConfigurationPropertyCollection
        Get
            Return QuerySettingsSection.ConfigurationPropertyCollection
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
        If Me.Settings Is Nothing Then
            Me.Settings = New QuerySettingsElementCollection()
        Else
            AddHandler Me.Settings.SettingsChanged, AddressOf ChildSettingsChangedHandler
        End If
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