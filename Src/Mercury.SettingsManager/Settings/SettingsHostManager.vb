Imports System.IO
Imports System.Xml
Imports System.Text
Imports System.Xml.Serialization
Imports Mercury.Settings.Internal

Namespace Settings

    ''' <summary>
    ''' A manager for general operations on a settings file.
    ''' </summary>
    ''' <remarks></remarks>
    Public Class SettingsHostManager

#Region "Shared Constructor"

        Shared Sub New()
            Reset()
        End Sub

#End Region
        
#Region "Manage Settings"

        Private Shared _settingsRoot As IInternalSettingsRoot

        ''' <summary>
        ''' Gets the internal settings root.
        ''' </summary>
        ''' <returns>The internal settings root.</returns>
        ''' <remarks></remarks>
        Public Shared ReadOnly Property SettingsRoot As IInternalSettingsRoot
            Get
                Return _settingsRoot
            End Get
        End Property

        ''' <summary>
        ''' A dictionary of current, in-memory sections.
        ''' </summary>
        ''' <remarks></remarks>
        Protected Shared InMemorySections As Dictionary(Of String, ISettingsHost)

        ''' <summary>
        ''' Adds the specified section and section name to the in-memory cache.  
        ''' If the section already exists in the cache, it is updated.
        ''' </summary>
        ''' <param name="sectionName">The name of the section to add.</param>
        ''' <param name="section">The section to add.</param>
        ''' <remarks></remarks>
        Public Shared Sub Append(ByVal sectionName As String, ByVal section As Object)
            Dim host As ISettingsHost = TryCast(section, ISettingsHost)
            If host Is Nothing Then _
                Throw New ArgumentException("Custom settings objects must implement ISettingsHost.")

            host.Invalidate()
            If InMemorySections.ContainsKey(sectionName) Then
                InMemorySections(sectionName) = section
            Else
                InMemorySections.Add(sectionName, section)
            End If
        End Sub

        ''' <summary>
        ''' Saves the specified section and section name to the in-memory cache.
        ''' </summary>
        ''' <param name="sectionName">The name of the section to save.</param>
        ''' <param name="section">The section to save.</param>
        ''' <remarks></remarks>
        Public Shared Sub Save(ByVal sectionName As String, ByVal section As Object)
            If section Is Nothing Then _
                Throw New ArgumentNullException("section")

            Dim record As BaseSettingsRecord = TryCast(SettingsRoot.GetSettingsRecord(SettingsPath), BaseSettingsRecord)
            If record IsNot Nothing Then
                record.Save(sectionName, section, False)
                Append(sectionName, section)
                CType(section, ISettingsHost).Reset()
            End If
        End Sub

        ''' <summary>
        ''' Saves all in-memory sections to the settings file.
        ''' </summary>
        ''' <remarks></remarks>
        Public Shared Sub Save()
            Dim record As BaseSettingsRecord = TryCast(SettingsRoot.GetSettingsRecord(SettingsPath), BaseSettingsRecord)
            If record IsNot Nothing Then
                If InMemorySections.Count > 0 Then
                    For Each kv As KeyValuePair(Of String, ISettingsHost) In InMemorySections
                        record.AppendToInMemorySettings(kv.Key, kv.Value)
                    Next
                End If
                record.Save(True)
                ConfigureAllInMemorySections(True)
            End If
        End Sub

        ''' <summary>
        ''' Gets the section with the specified name from the settings file.
        ''' </summary>
        ''' <param name="sectionName">The name of the section to retrieve.</param>
        ''' <returns>The section with the specified name or null.</returns>
        ''' <remarks></remarks>
        Public Shared Function GetSection(ByVal sectionName As String) As Object
            Dim record As BaseSettingsRecord = TryCast(SettingsRoot.GetSettingsRecord(SettingsPath), BaseSettingsRecord)
            If record IsNot Nothing Then
                Dim section As Object = record.GetSection(sectionName)
                If section IsNot Nothing Then
                    Append(sectionName, section)
                    CType(section, ISettingsHost).Reset()
                End If
                Return section
            End If
            Return Nothing
        End Function

        ''' <summary>
        ''' Gets a section from the in-memory cache. 
        ''' </summary>
        ''' <param name="sectionName">The name of the section to retrieve.</param>
        ''' <returns>The in-memory section with the specified name or null.</returns>
        ''' <remarks></remarks>
        Public Shared Function GetSectionFromMemory(ByVal sectionName As String) As ISettingsHost
            Dim result As ISettingsHost = Nothing
            If (InMemorySections.TryGetValue(sectionName, result)) Then _
                Return result
            Return Nothing
        End Function

        ''' <summary>
        ''' Determines whether the section associated with the specified name is located in-memory.
        ''' </summary>
        ''' <param name="sectionName">The name of the section to locate.</param>
        ''' <returns>True if the section associated with the specified name is located in-memory; otherwise, false.</returns>
        ''' <remarks></remarks>
        Public Shared Function IsInMemory(ByVal sectionName As String) As Boolean
            Return InMemorySections.ContainsKey(sectionName)
        End Function

        Protected Shared Sub ConfigureAllInMemorySections(ByVal isCurrent As Boolean)
            For Each sec As ISettingsHost In InMemorySections.Values
                If isCurrent Then
                    sec.Reset()
                Else
                    sec.Invalidate()
                End If
            Next
        End Sub

        Protected Shared Sub FireRootSettingsChanged()
            CType(SettingsRoot, InternalSettingsRoot).FireSettingsChanged(SettingsPath)
        End Sub

#End Region

#Region "Manage File"

        Private Shared _settingsPath As String

        ''' <summary>
        ''' Gets the path to the settings file.
        ''' </summary>
        ''' <returns>The path to the settings file.</returns>
        ''' <remarks></remarks>
        Public Shared ReadOnly Property SettingsPath As String
            Get
                Return _settingsPath
            End Get
        End Property

        Private Shared _encoding As Encoding

        ''' <summary>
        ''' Gets the file encoding to use on the settings file.
        ''' </summary>
        ''' <returns>The file encoding to use on the settings file.</returns>
        ''' <remarks></remarks>
        Public Shared ReadOnly Property Encoding As Encoding
            Get
                Return _encoding
            End Get
        End Property

        ''' <summary>
        ''' Sets the file encoding to be used on the settings file.
        ''' </summary>
        ''' <param name="encoding">The encoding to use.</param>
        ''' <remarks></remarks>
        Public Shared Sub SetEncoding(ByVal encoding As Encoding)
            _encoding = encoding
            _settingsRoot = New InternalSettingsRoot(encoding)
        End Sub

        ''' <summary>
        ''' Opens and retargets the manager to manage settings at the specified file path.
        ''' </summary>
        ''' <param name="path">The path to the settings file to manage.</param>
        ''' <remarks></remarks>
        Public Shared Sub OpenSettingsFile(ByVal path As String)
            _settingsPath = path
        End Sub

        ''' <summary>
        ''' Deletes the settings file.
        ''' </summary>
        ''' <remarks></remarks>
        Public Shared Sub RemoveSettings()
            RemoveSettings(SettingsPath)
        End Sub

        ''' <summary>
        ''' Deletes the settings file at the specified path.
        ''' </summary>
        ''' <param name="settingsPath">The path to the settings file to delete.</param>
        ''' <remarks></remarks>
        Public Shared Sub RemoveSettings(ByVal settingsPath As String)
            SettingsRoot.RemoveSettings(settingsPath)
        End Sub

        ''' <summary>
        ''' Resets the manager to its default state.
        ''' </summary>
        ''' <remarks></remarks>
        Public Shared Sub Reset()
            InMemorySections = New Dictionary(Of String, ISettingsHost)()
            SetEncoding(Encoding.Unicode)
        End Sub

#End Region

    End Class

End Namespace