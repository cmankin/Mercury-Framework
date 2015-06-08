Imports System.IO
Imports System.Xml
Imports System.Text

Namespace Settings.Internal

    ''' <summary>
    ''' Represents a root for accessing the base settings record.
    ''' </summary>
    ''' <remarks></remarks>
    Public NotInheritable Class InternalSettingsRoot
        Implements IInternalSettingsRoot

#Region "Constructors"

        Public Sub New()
            Me.New(Encoding.Unicode)
        End Sub

        Public Sub New(ByVal encoding As Encoding)
            Me._encoding = encoding
        End Sub

#End Region
        
#Region "IInternalSettingsRoot"

        Public Function GetSection(section As String, settingsPath As String) As Object Implements IInternalSettingsRoot.GetSection
            Dim record As BaseSettingsRecord = TryCast(GetSettingsRecord(settingsPath), BaseSettingsRecord)
            If (record IsNot Nothing) Then _
                Return record.GetSection(section)
            Return Nothing
        End Function

        Public Function GetSettingsRecord(settingsPath As String) As Object Implements IInternalSettingsRoot.GetSettingsRecord
            If Not (File.Exists(settingsPath)) Then _
                Me.AddSettings(settingsPath)

            Dim base As BaseSettingsRecord = New BaseSettingsRecord()
            base.Init(New SettingsConfigurator(settingsPath, Me._encoding, Me))
            Return base
        End Function

        Public Function AddSettings(settingsPath As String) As Boolean Implements IInternalSettingsRoot.AddSettings
            Dim flag As Boolean = False
            Try
                If Not (File.Exists(settingsPath)) Then
                    NewFile(settingsPath, Me._encoding)
                End If
                flag = True
            Catch ex As Exception
                flag = False
            End Try

            ' Fire event
            OnSettingsAdded(New InternalSettingsEventArgs(settingsPath))
            Return flag
        End Function

        Public Sub RemoveSettings(settingsPath As String) Implements IInternalSettingsRoot.RemoveSettings
            If File.Exists(settingsPath) Then
                File.Delete(settingsPath)
                OnSettingsRemoved(New InternalSettingsEventArgs(settingsPath))
            End If
        End Sub

#End Region

        Private _encoding As Encoding

        Public Event SettingsAdded(sender As Object, e As InternalSettingsEventArgs) Implements IInternalSettingsRoot.SettingsAdded

        Public Event SettingsChanged(sender As Object, e As InternalSettingsEventArgs) Implements IInternalSettingsRoot.SettingsChanged

        Public Event SettingsRemoved(sender As Object, e As InternalSettingsEventArgs) Implements IInternalSettingsRoot.SettingsRemoved

        Friend Sub FireSettingsChanged(ByVal settingsPath As String)
            Me.OnSettingsChanged(New InternalSettingsEventArgs(settingsPath))
        End Sub

        Private Sub OnSettingsChanged(ByVal e As InternalSettingsEventArgs)
            RaiseEvent SettingsChanged(Me, e)
        End Sub

        Private Sub OnSettingsAdded(ByVal e As InternalSettingsEventArgs)
            RaiseEvent SettingsAdded(Me, e)
        End Sub

        Private Sub OnSettingsRemoved(ByVal e As InternalSettingsEventArgs)
            RaiseEvent SettingsRemoved(Me, e)
        End Sub

        Friend Shared Sub NewFile(ByVal filePath As String, ByVal encoding As Encoding)
            If encoding Is Nothing Then _
                    Throw New ArgumentNullException("encoding")

            Try
                ' Create directory
                Dim dirPath As String = System.IO.Path.GetDirectoryName(filePath)
                Directory.CreateDirectory(dirPath)

                ' settings
                Dim writerSettings As New XmlWriterSettings()
                writerSettings.Encoding = encoding

                ' Create file
                Using sw As New StreamWriter(filePath)
                    Using writer As XmlWriter = XmlWriter.Create(sw, writerSettings)
                        ' Write start
                        writer.WriteStartDocument()

                        ' Write root
                        writer.WriteStartElement(BaseSettingsRecord.SETTINGS_ROOT_XML)
                        writer.WriteEndElement()

                        ' End document
                        writer.WriteEndDocument()
                    End Using
                End Using
            Catch ex As FileNotFoundException
                Throw New InvalidFilePathException(My.Resources.Invalid_File_Path, ex)
            Catch ex As DirectoryNotFoundException
                Throw New InvalidFilePathException(My.Resources.Invalid_File_Path, ex)
            Catch ex As PathTooLongException
                Throw New InvalidFilePathException(My.Resources.Invalid_File_Path, ex)
            Catch ex As ArgumentNullException
                Throw New InvalidFilePathException(My.Resources.Invalid_File_Path, ex)
            Catch ex As ArgumentException
                Throw New InvalidFilePathException(My.Resources.Invalid_File_Path, ex)
            Catch ex As IOException
                Throw New InvalidFilePathException(My.Resources.Invalid_File_Path, ex)
            End Try
        End Sub

    End Class

End Namespace