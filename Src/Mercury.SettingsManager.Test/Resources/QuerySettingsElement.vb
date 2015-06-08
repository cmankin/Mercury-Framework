Imports System.Configuration
Imports Mercury.Settings

''' <summary>
''' Specifies a configuration element for query settings.
''' </summary>
''' <remarks></remarks>
Friend Class QuerySettingsElement
    Inherits ConfigurationElement
    Implements ISettingsHost

#Region "Constructors"

    Shared Sub New()
        ' Attributes
        QuerySettingsElement.SourceProperty = New ConfigurationProperty("source", GetType(String), String.Empty)
        QuerySettingsElement.TypeProperty = New ConfigurationProperty("type", GetType(String), String.Empty)
        QuerySettingsElement.CommandProperty = New ConfigurationProperty("command", GetType(String), String.Empty)
        QuerySettingsElement.ParameterProperty = New ConfigurationProperty("parameter", GetType(String), String.Empty)
        QuerySettingsElement.TimeoutProperty = New ConfigurationProperty("timeout", GetType(Integer), 30)
        QuerySettingsElement.OverrideTaskAssemblyProperty = New ConfigurationProperty("overrideTaskAssembly", GetType(String), String.Empty)
        QuerySettingsElement.OverrideServiceTypeProperty = New ConfigurationProperty("overrideServiceType", GetType(String), String.Empty)
        QuerySettingsElement.KeyProperty = New ConfigurationProperty("key", GetType(String), String.Empty,
                                                                     ConfigurationPropertyOptions.IsRequired)

        ' Add properties
        QuerySettingsElement.ConfigurationPropertyCollection = New ConfigurationPropertyCollection()
        QuerySettingsElement.ConfigurationPropertyCollection.Add(QuerySettingsElement.SourceProperty)
        QuerySettingsElement.ConfigurationPropertyCollection.Add(QuerySettingsElement.TypeProperty)
        QuerySettingsElement.ConfigurationPropertyCollection.Add(QuerySettingsElement.CommandProperty)
        QuerySettingsElement.ConfigurationPropertyCollection.Add(QuerySettingsElement.ParameterProperty)
        QuerySettingsElement.ConfigurationPropertyCollection.Add(QuerySettingsElement.TimeoutProperty)
        QuerySettingsElement.ConfigurationPropertyCollection.Add(QuerySettingsElement.OverrideTaskAssemblyProperty)
        QuerySettingsElement.ConfigurationPropertyCollection.Add(QuerySettingsElement.OverrideServiceTypeProperty)
        QuerySettingsElement.ConfigurationPropertyCollection.Add(QuerySettingsElement.KeyProperty)
    End Sub

#End Region

#Region "Configuration Properties"

    ''' <summary>
    ''' A collection of configuration-element properties.
    ''' </summary>
    ''' <remarks></remarks>
    Friend Shared ConfigurationPropertyCollection As ConfigurationPropertyCollection
    Friend Shared SourceProperty As ConfigurationProperty
    Friend Shared TypeProperty As ConfigurationProperty
    Friend Shared CommandProperty As ConfigurationProperty
    Friend Shared ParameterProperty As ConfigurationProperty
    Friend Shared TimeoutProperty As ConfigurationProperty
    Friend Shared KeyProperty As ConfigurationProperty
    ''' <summary>
    ''' A string specifying the assembly to load for the task service type.  
    ''' This should be a relative path from the executing assembly.
    ''' </summary>
    Friend Shared OverrideTaskAssemblyProperty As ConfigurationProperty
    Friend Shared OverrideServiceTypeProperty As ConfigurationProperty

    <ConfigurationProperty("source")>
    Public Property Source As String
        Get
            Return CType(Me(QuerySettingsElement.SourceProperty), String)
        End Get
        Set(value As String)
            Me(QuerySettingsElement.SourceProperty) = value
            Me.Invalidate()
        End Set
    End Property

    <ConfigurationProperty("type")>
    Public Property Type As String
        Get
            Return CType(Me(QuerySettingsElement.TypeProperty), String)
        End Get
        Set(value As String)
            Me(QuerySettingsElement.TypeProperty) = value
            Me.Invalidate()
        End Set
    End Property

    <ConfigurationProperty("command")>
    Public Property Command As String
        Get
            Return CType(Me(QuerySettingsElement.CommandProperty), String)
        End Get
        Set(value As String)
            Me(QuerySettingsElement.CommandProperty) = value
            Me.Invalidate()
        End Set
    End Property

    <ConfigurationProperty("parameter")>
    Public Property Parameter As String
        Get
            Return CType(Me(QuerySettingsElement.ParameterProperty), String)
        End Get
        Set(value As String)
            Me(QuerySettingsElement.ParameterProperty) = value
            Me.Invalidate()
        End Set
    End Property

    <ConfigurationProperty("timeout")>
    Public Property Timeout As Integer
        Get
            Return CType(Me(QuerySettingsElement.TimeoutProperty), Integer)
        End Get
        Set(value As Integer)
            Me(QuerySettingsElement.TimeoutProperty) = value
            Me.Invalidate()
        End Set
    End Property

    <ConfigurationProperty("key")>
    Public Property Key As String
        Get
            Return CType(Me(QuerySettingsElement.KeyProperty), String)
        End Get
        Set(value As String)
            Me(QuerySettingsElement.KeyProperty) = value
            Me.Invalidate()
        End Set
    End Property

    <ConfigurationProperty("overrideTaskAssembly")>
    Public Property OverrideTaskAssembly As String
        Get
            Return CType(Me(QuerySettingsElement.OverrideTaskAssemblyProperty), String)
        End Get
        Set(value As String)
            Me(QuerySettingsElement.OverrideTaskAssemblyProperty) = value
            Me.Invalidate()
        End Set
    End Property

    <ConfigurationProperty("overrideServiceType")>
    Public Property OverrideServiceType
        Get
            Return CType(Me(QuerySettingsElement.OverrideServiceTypeProperty), String)
        End Get
        Set(value)
            Me(QuerySettingsElement.OverrideServiceTypeProperty) = value
            Me.Invalidate()
        End Set
    End Property

    Protected Overrides ReadOnly Property Properties As System.Configuration.ConfigurationPropertyCollection
        Get
            Return QuerySettingsElement.ConfigurationPropertyCollection
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

    Public Event SettingsChanged(sender As Object, e As SettingsChangedEventArgs) Implements ISettingsHost.SettingsChanged

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

    ''' <summary>
    ''' Builds a query settings object from the specified configuration.
    ''' </summary>
    ''' <param name="config">The query settings configuration data from which to build.</param>
    ''' <returns>A query settings object constructed from the specified configuration.</returns>
    ''' <remarks></remarks>
    Public Shared Function GetQuerySettings(ByVal config As QuerySettingsElement) As QuerySettings
        Return New QuerySettings(config)
    End Function

End Class