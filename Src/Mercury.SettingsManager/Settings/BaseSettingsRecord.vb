Imports System.IO
Imports System.Xml
Imports System.Xml.Serialization
Imports System.Text
Imports Mercury.Settings.Internal

Namespace Settings

    ''' <summary>
    ''' Represents a set of methods for actual access to settings data.
    ''' </summary>
    ''' <remarks></remarks>
    Friend Class BaseSettingsRecord

#Region "Constructors"

        ''' <summary>
        ''' Initializes a default instance of the BaseSettingsRecord class.
        ''' </summary>
        ''' <remarks></remarks>
        Public Sub New()
            Me._sectionRecords = New Dictionary(Of String, SectionRecord)()
            Me._inMemorySections = New Dictionary(Of String, Object)()
        End Sub

#End Region

#Region "Initialization & Properties"

        ''' <summary>
        ''' Initializes this settings record with the specified configuration.
        ''' </summary>
        ''' <param name="configurator">The settings configurator with which to initialize.</param>
        ''' <remarks></remarks>
        Friend Sub Init(ByVal configurator As SettingsConfigurator)
            If configurator Is Nothing Then _
                Throw New ArgumentNullException("configurator")
            Me._encoding = configurator.Encoding
            Me._filePath = configurator.Path
            Me._settingsRoot = configurator.SettingsRoot

            ' Init
            Me.InitFromFile(configurator.Path)
        End Sub

        ''' <summary>
        ''' Partially initializes the settings record from the specified settings file.
        ''' </summary>
        ''' <param name="settingsPath">The path to the settings file.</param>
        ''' <remarks></remarks>
        Private Sub InitFromFile(ByVal settingsPath As String)
            If String.IsNullOrEmpty(settingsPath) Then _
                Throw New ArgumentNullException("settingsPath")

            If Me._sectionRecords Is Nothing Then _
                Me._sectionRecords = New Dictionary(Of String, SectionRecord)()
            If String.IsNullOrEmpty(Me._filePath) Then _
                Me._filePath = settingsPath

            ' Reader
            Using reader As XmlTextReader = New XmlTextReader(settingsPath)
                reader.WhitespaceHandling = WhitespaceHandling.Significant

                ' Move to document element
                reader.MoveToContent()

                ' Move to first child of document root
                reader.Read()

                Do
                    ' Read
                    Dim current As SectionRecord = Nothing
                    ReadSectionRecord(reader, current)
                    If (current IsNot Nothing) Then
                        Me._sectionRecords.Add(current.Name, current)
                    End If

                    reader.Read()
                Loop While (reader.NodeType = XmlNodeType.Element)
            End Using
        End Sub

        Private _settingsRoot As IInternalSettingsRoot

        ''' <summary>
        ''' Gets the internal settings root.
        ''' </summary>
        ''' <returns>The internal settings root.</returns>
        ''' <remarks></remarks>
        Public ReadOnly Property SettingsRoot As IInternalSettingsRoot
            Get
                Return Me._settingsRoot
            End Get
        End Property

#End Region
        
#Region "Section Retrieval"

        ''' <summary>
        ''' Returns the section object associated with the specified section name.
        ''' </summary>
        ''' <param name="section">The name of the section to retrieve.</param>
        ''' <returns>The section object associated with the specified section name or null.</returns>
        ''' <remarks></remarks>
        Friend Overridable Function GetSection(ByVal section As String) As Object
            If Me._sectionRecords.ContainsKey(section) Then _
                Return GetSectionFromRecord(Me._sectionRecords(section))
            Return Nothing
        End Function

        ''' <summary>
        ''' Returns the section object described the specified section record.
        ''' </summary>
        ''' <param name="record">The section record describing the section to retrieve.</param>
        ''' <returns>The section object described the specified section record or null.</returns>
        ''' <remarks></remarks>
        Friend Overridable Function GetSectionFromRecord(ByVal record As SectionRecord) As Object
            ' Ensure valid type
            record.EnsureValidType()
            If Not (record.HasErrors) Then _
                Return GetSectionFromXml(record.Name, record.GetSectionType())
            Return Nothing
        End Function

        ''' <summary>
        ''' Returns the section with the specified name and type from the XML settings file.
        ''' </summary>
        ''' <param name="sectionName">The name of the section to retrieve.</param>
        ''' <param name="sectionType">The type of the section to retrieve.</param>
        ''' <returns>The section with the specified name and type from the XML settings file or null.</returns>
        ''' <remarks></remarks>
        Friend Overridable Function GetSectionFromXml(ByVal sectionName As String, ByVal sectionType As Type) As Object
            If String.IsNullOrEmpty(sectionName) Then _
                Throw New ArgumentNullException("sectionName")
            If sectionType Is Nothing Then _
                Throw New ArgumentNullException("sectionType")

            Dim instance As Object = Nothing
            Using reader As XmlTextReader = New XmlTextReader(Me._filePath)
                reader.WhitespaceHandling = WhitespaceHandling.Significant

                ' Initialize reader
                reader.MoveToContent()
                reader.Read()

                ' Queue element
                Dim flag As Boolean = QueueElement(reader, sectionName)
                If flag Then
                    ' Deserialize
                    ReadSectionFromXml(reader, sectionType, instance)
                End If
            End Using
            Return instance
        End Function

#End Region
        
#Region "Persistance"

        ''' <summary>
        ''' Saves the current in-memory sections to the settings file.
        ''' </summary>
        ''' <param name="forceSaveAll">A value indicating whether to save all 
        ''' in-memory objects even if no changes have occurred.</param>
        ''' <remarks></remarks>
        Friend Overridable Sub Save(ByVal forceSaveAll As Boolean)
            ' Temp save path
            Dim saveFilePath As String = String.Format("{0}.tmp", Me._filePath)

            Try
                ' Reader
                Using reader As XmlTextReader = New XmlTextReader(Me._filePath)
                    reader.WhitespaceHandling = WhitespaceHandling.Significant

                    ' Writer
                    Using writer As XmlWriter = New XmlTextWriter(saveFilePath, Me._encoding)
                        ' Move to document element
                        reader.MoveToContent()

                        ' Write initial
                        writer.WriteStartElement(reader.Prefix, reader.LocalName, reader.NamespaceURI)
                        WriteAttributes(reader, writer)

                        ' Move to first child of document root
                        reader.Read()

                        ' Skip section records
                        If (reader.NodeType = XmlNodeType.Element AndAlso reader.LocalName = SETTINGS_SECTIONS) Then _
                            reader.Skip()

                        ' Write new section records
                        WriteSectionRecords(writer, Me._sectionRecords.Values)

                        Do
                            ' Get current section info
                            Dim elementName As String = reader.LocalName
                            Dim sr As SectionRecord = Nothing
                            If Me._sectionRecords.ContainsKey(elementName) Then
                                sr = Me._sectionRecords(elementName)
                            End If

                            ' If record can be written
                            If (sr IsNot Nothing AndAlso Not (sr.HasErrors)) Then
                                ' Read current section
                                Dim current As Object = Nothing
                                ReadSectionFromXml(reader, sr.GetSectionType(), current)
                                If (current IsNot Nothing) Then
                                    ' If in-memory version exists
                                    If Me._inMemorySections.ContainsKey(elementName) Then
                                        current = Me._inMemorySections(elementName)
                                    End If

                                    ' Get host
                                    Dim currentHost As ISettingsHost = TryCast(current, ISettingsHost)
                                    If currentHost Is Nothing Then _
                                        Throw New ArgumentException(My.Resources.Settings_Invalid_Parameter)

                                    ' If can be written...
                                    If Not (currentHost.IsCurrent) OrElse forceSaveAll Then
                                        ' Write
                                        WriteSection(writer, elementName, currentHost)
                                        currentHost.Reset()
                                    End If
                                    sr.IsPersisted = True
                                End If
                            End If

                            reader.Read()
                        Loop While (reader.NodeType = XmlNodeType.Element)

                        ' Append any unsaved sections
                        For Each sRec As SectionRecord In Me._sectionRecords.Values
                            If Not (sRec.IsPersisted) Then
                                sRec.IsPersisted = True
                                ' If in memory
                                If Me._inMemorySections.ContainsKey(sRec.Name) Then
                                    If sRec.HasErrors Then _
                                        Throw New ArgumentException(My.Resources.Settings_Section_Has_Errors)
                                    Dim host As ISettingsHost = TryCast(Me._inMemorySections(sRec.Name), ISettingsHost)
                                    If Not (host.IsCurrent) OrElse forceSaveAll Then
                                        WriteSection(writer, sRec.Name, Me._inMemorySections(sRec.Name))
                                        host.Reset()
                                    End If
                                End If
                            End If
                        Next
                    End Using
                End Using

                ' Replace file
                If (File.Exists(Me._filePath)) Then _
                    File.Delete(Me._filePath)
                FileSystem.Rename(saveFilePath, Me._filePath)
            Catch ex As Exception
                If (File.Exists(saveFilePath)) Then _
                    File.Delete(saveFilePath)
                Throw
            Finally
                Me._inMemorySections.Clear()
            End Try
        End Sub

        ''' <summary>
        ''' Saves the specified section with the specified section name to the settings file.
        ''' </summary>
        ''' <param name="sectionName">The name of the section element.</param>
        ''' <param name="section">The section to save.</param>
        ''' <param name="forceSaveAll">A value indicating whether to save all 
        ''' in-memory objects even if no changes have occurred.</param>
        ''' <remarks></remarks>
        Friend Overridable Sub Save(ByVal sectionName As String, ByVal section As Object, ByVal forceSaveAll As Boolean)
            If section IsNot Nothing AndAlso Not (String.IsNullOrEmpty(sectionName)) Then
                ' Append in memory
                AppendToInMemorySettings(sectionName, section)

                ' Attempt save
                Me.Save(forceSaveAll)
            End If
        End Sub

        ''' <summary>
        ''' Appends the specified section name and section to the list of in-memory objects.
        ''' </summary>
        ''' <param name="sectionName">The name of the section element.</param>
        ''' <param name="section">The section to append.</param>
        ''' <remarks>The list of in-memory objects is flushed and reset during every save.</remarks>
        Friend Sub AppendToInMemorySettings(ByVal sectionName As String, ByVal section As ISettingsHost)
            If section IsNot Nothing AndAlso Not (String.IsNullOrEmpty(sectionName)) Then
                ' Set/update section record
                Dim record As New SectionRecord(sectionName, section.GetType())
                If Not (Me._sectionRecords.ContainsKey(sectionName)) Then
                    Me._sectionRecords.Add(record.Name, record)
                Else
                    Me._sectionRecords(sectionName) = record
                End If

                ' Set/update in-memory instance.
                If Not (Me._inMemorySections.ContainsKey(sectionName)) Then
                    Me._inMemorySections.Add(sectionName, section)
                Else
                    Me._inMemorySections(sectionName) = section
                End If

                '' Get root
                'Dim root As InternalSettingsRoot = TryCast(Me.SettingsRoot, InternalSettingsRoot)

                '' Get host
                'Dim host As ISettingsHost = TryCast(section, ISettingsHost)
                'If host IsNot Nothing Then
                '    host.Invalidate()
                '    ' Fire change
                '    root.FireSettingsChanged(Me._filePath)
                'End If
            End If
        End Sub

#End Region
        
#Region "Helpers"

        ''' <summary>
        ''' Writes a set of attributes from the reader node to the writer.
        ''' </summary>
        Friend Shared Sub WriteAttributes(ByVal reader As XmlReader, ByVal writer As XmlWriter)
            If (reader.MoveToFirstAttribute()) Then
                Do
                    writer.WriteAttributeString(reader.Prefix, reader.LocalName, reader.NamespaceURI, reader.Value)
                Loop While (reader.MoveToNextAttribute())
                reader.MoveToElement()
            End If
        End Sub

        ''' <summary>
        ''' Writes a collection of section records with the specified XML writer.
        ''' </summary>
        Friend Shared Sub WriteSectionRecords(ByVal writer As XmlWriter, ByVal records As ICollection(Of SectionRecord))
            If records Is Nothing Then _
                Throw New ArgumentNullException("records")

            writer.WriteStartElement(BaseSettingsRecord.SETTINGS_SECTIONS)
            For Each rec As SectionRecord In records
                writer.WriteStartElement(SECTION_RECORD_TAG)
                rec.WriteXml(writer)
                writer.WriteEndElement()
            Next
            writer.WriteEndElement()
        End Sub

        ''' <summary>
        ''' Writes a section element with the specified XML writer.
        ''' </summary>
        Friend Shared Sub WriteSection(ByVal writer As XmlWriter, ByVal sectionName As String, ByVal section As ISettingsHost)
            If (section Is Nothing) Then _
                Throw New ArgumentNullException("section")

            section.Serialize(writer, sectionName)
        End Sub

        ''' <summary>
        ''' Reads a section element from the specified XML reader.
        ''' </summary>
        Friend Overridable Sub ReadSectionFromXml(ByVal reader As XmlReader, ByVal sectionType As Type, ByRef section As Object)
            If reader Is Nothing Then _
                Throw New ArgumentNullException("reader")
            If sectionType Is Nothing Then _
                Throw New ArgumentNullException("sectionType")

            Dim baseType As Type = TypeUtil.VerifyImplementsInterface(sectionType, GetType(ISettingsHost), True)
            Dim instance As ISettingsHost = TryCast(TypeUtil.CreateInstanceWithReflectionPermission(baseType), ISettingsHost)
            If instance IsNot Nothing Then
                instance.Deserialize(reader, False)
                section = instance
            End If
        End Sub

        ''' <summary>
        ''' Reads a section record from the specified XML reader.
        ''' </summary>
        Protected Friend Shared Sub ReadSectionRecord(ByVal reader As XmlReader, ByRef record As SectionRecord)
            If (reader IsNot Nothing) Then
                If (reader.NodeType = XmlNodeType.Element AndAlso reader.LocalName = SECTION_RECORD_TAG) Then
                    Dim rec2 As SectionRecord = New SectionRecord()
                    rec2.ReadXml(reader)
                    record = rec2
                End If
            End If
        End Sub

        ''' <summary>
        ''' Moves the reader to the element with the specified local name.
        ''' </summary>
        ''' <param name="reader">The XML reader to use.</param>
        ''' <param name="elementName">The name of the element to queue.</param>
        ''' <returns>True if the element was found and the move was successful; otherwise, false.</returns>
        ''' <remarks></remarks>
        Protected Function QueueElement(ByVal reader As XmlReader, ByVal elementName As String) As Boolean
            Dim isEnd As Boolean = False
            Do
                If (reader.NodeType = XmlNodeType.EndElement) Then _
                    Return False
                If (reader.NodeType = XmlNodeType.Element) Then
                    If (reader.LocalName = elementName) Then _
                        Return True

                    If (Not (reader.IsEmptyElement)) Then
                        reader.Skip()
                        Continue Do
                    End If
                End If

                If Not (reader.Read()) Then _
                    isEnd = True
            Loop While Not (isEnd)
            Return False
        End Function

#End Region

#Region "Data"

        Protected _filePath As String
        Protected _sectionRecords As Dictionary(Of String, SectionRecord)
        Protected Friend _inMemorySections As Dictionary(Of String, Object)
        Protected _encoding As Encoding

        Friend Const SETTINGS_SECTIONS As String = "settingsSections"
        Friend Const SECTION_RECORD_TAG As String = "section"
        Friend Const SETTINGS_ROOT_XML As String = "Settings"

#End Region

    End Class

End Namespace