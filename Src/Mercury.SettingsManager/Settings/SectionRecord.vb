Imports System.Xml
Imports System.Xml.Serialization

Namespace Settings

    ''' <summary>
    ''' Represents a section in the settings file.
    ''' </summary>
    ''' <remarks></remarks>
    Friend Class SectionRecord
        Implements IXmlSerializable

#Region "Constructors"

        Friend Sub New()
        End Sub

        Friend Sub New(ByVal name As String, ByVal type As Type)
            Me.New(name, TypeUtil.GetQualifiedTypeString(type, False))
        End Sub

        Friend Sub New(ByVal name As String, ByVal typeString As String)
            Me.Name = name
            Me.TypeString = typeString
        End Sub

#End Region

#Region "Properties"

        Private _name As String

        ''' <summary>
        ''' Gets or sets the name of the section.
        ''' </summary>
        ''' <returns>The name of the section.</returns>
        ''' <remarks></remarks>
        Friend Property Name As String
            Get
                Return Me._name
            End Get
            Set(value As String)
                Me._name = value
            End Set
        End Property

        Private _typeString As String

        ''' <summary>
        ''' Gets or sets the type string for the section object.
        ''' </summary>
        ''' <returns>The type string for the section object.</returns>
        ''' <remarks></remarks>
        Friend Property TypeString As String
            Get
                Return Me._typeString
            End Get
            Set(value As String)
                Me._typeString = value
                EnsureValidType()
            End Set
        End Property

        Private _isPersisted As Boolean = False

        ''' <summary>
        ''' Gets or sets a value indicating whether this section 
        ''' record has been persisted within a current operation.
        ''' </summary>
        ''' <value></value>
        ''' <returns>True if this section has been persisted; otherwise, false.</returns>
        ''' <remarks></remarks>
        Friend Property IsPersisted As Boolean
            Get
                Return Me._isPersisted
            End Get
            Set(value As Boolean)
                Me._isPersisted = value
            End Set
        End Property

        Private _hasErrors As Boolean

        ''' <summary>
        ''' Gets a value indicating whether this record has errors.
        ''' </summary>
        ''' <returns>True if the record has errors; otherwise, false.</returns>
        ''' <remarks></remarks>
        Friend ReadOnly Property HasErrors As Boolean
            Get
                Return Me._hasErrors
            End Get
        End Property

#End Region
        
#Region "Type Methods"

        Friend Function GetSectionType() As Type
            Dim type As Type = Nothing
            If (EnsureValidType(type)) Then _
                Return type
            Return Nothing
        End Function

        Friend Sub EnsureValidType()
            Dim type As Type = Nothing
            EnsureValidType(type)
        End Sub

        Protected Friend Function EnsureValidType(ByRef type As Type) As Boolean
            Me._hasErrors = False
            type = TypeUtil.GetTypeWithReflectionPermission(Me.TypeString, True)
            If type Is Nothing Then
                Me._hasErrors = True
            Else
                Me._hasErrors = False
            End If

            Return Not (Me.HasErrors)
        End Function


#End Region
        
#Region "IXmlSerializable"

        Public Function GetSchema() As System.Xml.Schema.XmlSchema Implements System.Xml.Serialization.IXmlSerializable.GetSchema
            Return Nothing
        End Function

        Public Sub ReadXml(reader As System.Xml.XmlReader) Implements System.Xml.Serialization.IXmlSerializable.ReadXml
            If reader.NodeType = XmlNodeType.Element AndAlso reader.HasAttributes AndAlso reader.LocalName = "section" Then
                reader.MoveToAttribute("name")
                Me.Name = reader.Value

                reader.MoveToAttribute("type")
                Me.TypeString = reader.Value
            End If
        End Sub

        Public Sub WriteXml(writer As System.Xml.XmlWriter) Implements System.Xml.Serialization.IXmlSerializable.WriteXml
            ' Write attributes
            writer.WriteAttributeString("name", Me.Name)
            writer.WriteAttributeString("type", Me.TypeString)
        End Sub

#End Region

    End Class

End Namespace