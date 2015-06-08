Imports System.Configuration
Imports Mercury.Settings

''' <summary>
''' Represents a collection of data source elements.
''' </summary>
''' <remarks></remarks>
Friend Class QuerySettingsElementCollection
    Inherits ConfigurationElementCollection
    Implements ISettingsHost

    Private Const SOURCE_ELEMENT_TAG As String = "query"

#Region "Constructors"

    Public Sub New()
    End Sub

    Shared Sub New()
        QuerySettingsElementCollection.ConfigurationPropertyCollection = New ConfigurationPropertyCollection()
    End Sub

#End Region

#Region "Configuration Properties"

    ''' <summary>
    ''' A collection of configuration-element properties.
    ''' </summary>
    ''' <remarks></remarks>
    Friend Shared ConfigurationPropertyCollection As ConfigurationPropertyCollection

    Protected Overrides ReadOnly Property Properties As System.Configuration.ConfigurationPropertyCollection
        Get
            Return QuerySettingsElementCollection.ConfigurationPropertyCollection
        End Get
    End Property

#End Region

#Region "Base Class Overrides"

    Protected Overloads Overrides Function CreateNewElement() As System.Configuration.ConfigurationElement
        Dim element As New QuerySettingsElement()
        AddHandler element.SettingsChanged, AddressOf ChildSettingsChangedHandler
        Return element
    End Function

    Protected Overrides Function GetElementKey(element As System.Configuration.ConfigurationElement) As Object
        Return CType(element, QuerySettingsElement).Key
    End Function

    Public Overrides ReadOnly Property CollectionType As System.Configuration.ConfigurationElementCollectionType
        Get
            Return ConfigurationElementCollectionType.BasicMap
        End Get
    End Property

    Protected Overrides ReadOnly Property ElementName As String
        Get
            Return SOURCE_ELEMENT_TAG
        End Get
    End Property

    Public Function IndexOf(ByVal querySettings As QuerySettingsElement) As Integer
        Return BaseIndexOf(querySettings)
    End Function

    Public Sub Clear()
        MyBase.BaseClear()
        Me.Invalidate()
    End Sub

    Public Sub AddOrUpdate(ByVal querySettings As QuerySettingsElement, Optional ByVal index As Integer = -1)
        ' Add handler
        AddHandler querySettings.SettingsChanged, AddressOf ChildSettingsChangedHandler

        ' Resolve index
        If index < 0 Then _
            index = IndexOf(querySettings)

        ' Add or replace
        Dim current As QuerySettingsElement = Nothing
        If index > -1 AndAlso index < MyBase.Count Then
            RemoveAt(index)
            MyBase.BaseAdd(index, querySettings)
        Else
            MyBase.BaseAdd(querySettings)
        End If

        ' Invalidate
        Me.Invalidate()
    End Sub

    Public Sub Remove(ByVal querySettings As QuerySettingsElement)
        Dim index As Integer = IndexOf(querySettings)
        If index > -1 Then
            RemoveAt(index)
        End If
    End Sub

    Public Sub RemoveAt(ByVal index As Integer)
        Dim current As QuerySettingsElement = MyBase.BaseGet(index)
        If current IsNot Nothing Then _
            RemoveHandler current.SettingsChanged, AddressOf ChildSettingsChangedHandler
        MyBase.BaseRemoveAt(index)
        Me.Invalidate()
    End Sub

#End Region

#Region "Indexers"

    Public Property Query(ByVal index As Integer) As QuerySettingsElement
        Get
            Return CType(MyBase.BaseGet(index), QuerySettingsElement)
        End Get
        Set(value As QuerySettingsElement)
            AddOrUpdate(value, index)
        End Set
    End Property

    Public ReadOnly Property Query(ByVal key As String) As QuerySettingsElement
        Get
            Return CType(MyBase.BaseGet(key), QuerySettingsElement)
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