Imports System.IO
Imports System.Windows.Forms
Imports System.Security.Cryptography
Imports Mercury.Security.Cryptography

Public Class MainWindow

    Public Sub New()

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        Me.HashSelection.SelectedIndex = -1
    End Sub

    Private Sub BrowseFileButton_Click(sender As System.Object, e As System.Windows.RoutedEventArgs) Handles BrowseFileButton.Click
        Dim result = MainWindow.OpenSingleFileBrowser()
        Me.FilePathTextBox.Text = result
    End Sub

    Private Sub ComputeButton_Click(sender As System.Object, e As System.Windows.RoutedEventArgs) Handles ComputeButton.Click
        If Me.HashSelection.SelectedIndex < 0 Then
            Me.SetOutput("No hash algorithm selected.")
        ElseIf String.IsNullOrEmpty(Me.FilePathTextBox.Text) Then
            Me.SetOutput("No file path specified.")
        Else
            Dim key = Me.KeyTextBox.Text
            Dim path = Trim(Me.FilePathTextBox.Text)
            Dim hash As String = Nothing
            Dim result = MainWindow.ComputeHash(Me.GetSelectedAlgorithKey(), path, hash)

            Dim output As String = Nothing
            If Not (String.IsNullOrEmpty(hash)) AndAlso Not (String.IsNullOrEmpty(key)) Then
                Dim comp = String.Equals(hash, key, StringComparison.InvariantCultureIgnoreCase)
                output = String.Format("{0}{1}Key match='{2}'", result, Environment.NewLine, comp)
            Else
                output = result
            End If

            Me.SetOutput(output)
        End If
    End Sub

    Protected Function GetSelectedAlgorithKey() As String
        If Me.HashSelection.SelectedItem IsNot Nothing Then
            Dim ci = TryCast(Me.HashSelection.SelectedItem, ComboBoxItem)
            Return CStr(ci.Content)
        End If
        Return Nothing
    End Function

    Protected Sub SetOutput(ByVal text As String)
        Me.OutputText.Text = text
    End Sub

    Protected Shared Function ComputeHash(ByVal hashId As String, ByVal filePath As String, ByRef output As String) As String
        Dim fStream As FileStream = Nothing
        Try
            fStream = File.Open(filePath, FileMode.Open, FileAccess.Read)
        Catch ex As Exception
            ' Swallow & report
            Return String.Format("Exception occurred while attempting to open the file: {0}", ex.Message)
        End Try

        Dim ha As HashAlgorithm = Nothing
        Select Case hashId
            Case "CRC"
                ha = CRC32.Create()
            Case "MD5"
                ha = MD5.Create()
            Case "SHA1"
                ha = SHA1.Create()
            Case "SHA256"
                ha = SHA256.Create()
            Case "SHA384"
                ha = SHA384.Create()
            Case "SHA512"
                ha = SHA512.Create()
            Case Else
                Return String.Format("Specified hash algorithm '{0}' is not supported.", hashId)
        End Select

        ' Compute
        Try
            output = ComputeHashFromStream(ha, fStream)
            Return String.Format("{0}: {1}", hashId, output)
        Catch ex As Exception
            ' Swallow & report
            Return String.Format("Exception occurred while attempting to compute the hash value: {0}", ex.Message)
        End Try
    End Function

    Friend Shared Function ComputeHashFromStream(ByVal algorithm As HashAlgorithm, ByVal stream As FileStream) As String
        If algorithm Is Nothing Then _
            Throw New ArgumentNullException("algorithm")
        If stream Is Nothing Then _
            Throw New ArgumentNullException("stream")

        ' Get hash value
        Dim hash = algorithm.ComputeHash(stream)
        ' Convert to hex
        Dim temp = BitConverter.ToString(hash)
        Return temp.Replace("-", "")
    End Function

    ''' <summary>
    ''' Opens a file browser window and returns the path to the selected file.
    ''' </summary>
    ''' <param name="title">File dialog box title.</param>
    ''' <param name="filter">The file name filter string, which determines the choices 
    ''' that appear in the "Save as file type" or "Files of type" box in the dialog box.</param>
    ''' <returns>The path to the selected file.</returns>
    ''' <remarks></remarks>
    Public Shared Function OpenSingleFileBrowser(Optional ByVal title As String = "Select a file to open.", _
                                                 Optional ByVal filter As String = "All files (*.*)|*.*") As String
        ' Selector
        Dim fileSelector As System.Windows.Forms.OpenFileDialog = New System.Windows.Forms.OpenFileDialog()
        fileSelector.Title = title
        fileSelector.Filter = filter
        fileSelector.Multiselect = False
        ' Show dialog and return selected value(s)
        If (fileSelector.ShowDialog() = Forms.DialogResult.OK) Then _
            Return fileSelector.FileName
        Return String.Empty
    End Function

End Class