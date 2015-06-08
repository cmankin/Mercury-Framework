Imports System.IO
Imports System.Xml
Imports System.Text
Imports Mercury.Document

Namespace OpenXml.Excel

    ''' <summary>
    ''' Represents a reader for Microsoft OpenXML Excel files.
    ''' </summary>
    ''' <remarks></remarks>
    Public Class ExcelFastReader
        Implements IDisposable

        ' OpenXml names
        Private Const SST_ROOT As String = "sst"
        Private Const SST_VALUE As String = "t"
        Private Const SHEET_DATA As String = "sheetData"
        Private Const XL_ROW As String = "row"
        Private Const XL_CELL As String = "c"
        Private Const XL_CELL_TYPE As String = "t"
        Private Const XL_CELL_VALUE As String = "v"
        Private Const XL_POS As String = "r"

        ' DataSet properties
        Private Const DEFAULT_COL_NAME = "column"

        ' Paths
        Private Const WORKSHEET_PATH As String = "xl/worksheets"
        Private Const SHAREDSTRING_FILE As String = "xl/sharedStrings.xml"

        ' Attributes
        Private _package As FilePackage
        Private _stringTable As IList(Of String)
        Private _currentWorksheet As String

        ' Reader attributes
        Private _canReadRow As Boolean = False
        Private xReader As XmlReader
        Private _currentRow As ICollection(Of String)
        Private _readBlock As IDictionary(Of String, Integer)

#Region "Constructors"

        ''' <summary>
        ''' Initializes a default instance of the <c>ExcelFastReader</c> class with the specified file path.
        ''' </summary>
        ''' <param name="path">The path to the Excel file to read.</param>
        ''' <remarks></remarks>
        Public Sub New(ByVal path As String)
            Me.New(path, True)
        End Sub

        ''' <summary>
        ''' Initializes a default instance of the <c>ExcelFastReader</c> class with the 
        ''' specified file path and string table cache state.
        ''' </summary>
        ''' <param name="path">The path to the Excel file to read.</param>
        ''' <param name="isCachedStringTable">A value indicating whether the <c>StringTable</c> should be cached.  
        ''' If the <c>StringTable</c> is not cached all string values in the file will appear as the numeric 
        ''' equivalent to the position in which they are located in the table.  This table may be cached after 
        ''' object construction by calling the <see>CacheLocalStringTable</see> method.</param>
        ''' <remarks></remarks>
        Public Sub New(ByVal path As String, ByVal isCachedStringTable As Boolean)
            Me.New(path, isCachedStringTable, String.Empty)
        End Sub

        ''' <summary>
        ''' Initializes a default instance of the <c>ExcelFastReader</c> class with the 
        ''' specified file path, string table cache state, and current worksheet name.
        ''' </summary>
        ''' <param name="path">The path to the Excel file to read.</param>
        ''' <param name="isCachedStringTable">A value indicating whether the <c>StringTable</c> should be cached.  
        ''' If the <c>StringTable</c> is not cached all string values in the file will appear as the numeric 
        ''' equivalent to the position in which they are located in the table.  This table may be cached after 
        ''' object construction by calling the <see>CacheLocalStringTable</see> method.</param>
        ''' <param name="currentWorksheet">The name of the worksheet to begin reading.</param>
        ''' <remarks></remarks>
        Public Sub New(ByVal path As String, ByVal isCachedStringTable As Boolean, ByVal currentWorksheet As String)
            Me.New(path, isCachedStringTable, currentWorksheet, False)
        End Sub

        ''' <summary>
        ''' Initializes a default instance of the <c>ExcelFastReader</c> class with the 
        ''' specified file path, string table cache state, current worksheet name, and 
        ''' a value indicating whether the first row contains column headers.
        ''' </summary>
        ''' <param name="path">The path to the Excel file to read.</param>
        ''' <param name="isCachedStringTable">A value indicating whether the <c>StringTable</c> should be cached.  
        ''' If the <c>StringTable</c> is not cached all string values in the file will appear as the numeric 
        ''' equivalent to the position in which they are located in the table.  This table may be cached after 
        ''' object construction by calling the <see>CacheLocalStringTable</see> method.</param>
        ''' <param name="currentWorksheet">The name of the worksheet to begin reading.</param>
        ''' <param name="firstRowHeaders">A value indicating whether the first row of data contains column headers.</param>
        ''' <remarks></remarks>
        Public Sub New(ByVal path As String, ByVal isCachedStringTable As Boolean, _
                       ByVal currentWorksheet As String, ByVal firstRowHeaders As Boolean)
            Me._package = New FilePackage(path)
            Me.CurrentWorksheet = currentWorksheet
            Me.FirstRowHeaders = firstRowHeaders
            If isCachedStringTable Then _
                CacheLocalStringTable()
        End Sub

#End Region

#Region "Properties"

        ''' <summary>
        ''' Gets a value indicating whether the current state of the reader allows by-row reading.
        ''' </summary>
        ''' <value></value>
        ''' <returns>True if the current state of the reader allows by-row reading; otherwise, false.</returns>
        ''' <remarks></remarks>
        Public ReadOnly Property CanReadRow
            Get
                Return Me._canReadRow
            End Get
        End Property

        ''' <summary>
        ''' Gets the current row of data.
        ''' </summary>
        ''' <value>The current row of data.</value>
        ''' <returns>The current row of data.</returns>
        ''' <remarks></remarks>
        Public ReadOnly Property CurrentRow As ICollection(Of String)
            Get
                Return Me._currentRow
            End Get
        End Property

        ''' <summary>
        ''' Gets or sets the name of the current worksheet from which to read data.
        ''' </summary>
        ''' <value>The name of the current worksheet from which to read data.</value>
        ''' <returns>The name of the current worksheet from which to read data.</returns>
        ''' <remarks></remarks>
        Public Property CurrentWorksheet As String
            Get
                Return Me._currentWorksheet
            End Get
            Set(value As String)
                Me._currentWorksheet = value
                SetRowReader(Not (String.IsNullOrEmpty(value)))
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets a value indicating whether the first row of the Excel file contains column headers.
        ''' </summary>
        ''' <value>A value indicating whether the first row of the Excel file contains column headers.</value>
        ''' <returns>A value indicating whether the first row of the Excel file contains column headers.</returns>
        ''' <remarks></remarks>
        Public Property FirstRowHeaders As Boolean

        ''' <summary>
        ''' Gets the <c>FilePackage</c> for the Excel file.
        ''' </summary>
        ''' <returns>The <c>FilePackage</c> for the Excel file.</returns>
        ''' <remarks></remarks>
        Public ReadOnly Property Package As FilePackage
            Get
                Return Me._package
            End Get
        End Property

        ''' <summary>
        ''' Gets the cached string table used in the <c>Excel</c> file. 
        ''' If the table is not already cached, it will attempt to 
        ''' cache and return the result.
        ''' </summary>
        ''' <returns>The cached string table used in the <c>Excel</c> file.</returns>
        ''' <remarks></remarks>
        Public ReadOnly Property StringTable() As IList(Of String)
            Get
                If Me._stringTable Is Nothing Then _
                    CacheLocalStringTable()
                Return _stringTable
            End Get
        End Property

        Protected ReadOnly Property ReadBlock As IDictionary(Of String, Integer)
            Get
                Return Me._readBlock
            End Get
        End Property

#End Region

#Region "Helpers"

        ''' <summary>
        ''' Caches the <c>StringTable</c> for the current Excel document.
        ''' </summary>
        ''' <remarks></remarks>
        Public Sub CacheLocalStringTable()
            Me._stringTable = New List(Of String)
            Dim xml As String = GetContent(SHAREDSTRING_FILE)
            Using reader As XmlReader = XmlTextReader.Create(New StringReader(xml))
                Do While reader.Read
                    If reader.NodeType = XmlNodeType.Element AndAlso reader.LocalName = SST_ROOT Then
                        Do While reader.Read
                            If reader.NodeType = XmlNodeType.Element AndAlso reader.LocalName = SST_VALUE Then
                                If reader.Read Then _
                                    Me.StringTable.Add(reader.Value)
                            End If
                        Loop
                    End If
                Loop
            End Using
        End Sub

        Private Sub FillNullCells(ByVal blockPos As Integer, ByRef row As ICollection(Of String))
            Dim nextPos As Integer = blockPos
            Do While (nextPos - (row.Count - 1)) > 1
                row.Add(String.Empty)
            Loop
        End Sub

        ''' <summary>
        ''' Gets the <c>XML</c> content of the file at the specified path.
        ''' </summary>
        ''' <param name="path">Relative path from the package root to the file resource.</param>
        ''' <returns>The <c>XML</c> content of the file at the specified path.</returns>
        ''' <remarks></remarks>
        Protected Function GetContent(ByVal path As String) As String
            Me.Package.ExtractContentInfo()
            Return Me.Package.ExtractContent(path)
        End Function

        Private Sub SetRowReader(ByVal canRead As Boolean)
            Me._canReadRow = canRead
            If canRead Then
                Me.xReader = XmlTextReader.Create(New StringReader(GetContent( _
                                                    String.Format("{0}/{1}.xml", WORKSHEET_PATH, Me.CurrentWorksheet))))
            Else    ' Set reader to null
                Me.xReader = Nothing
            End If
        End Sub

        ''' <summary>
        ''' Returns a specified <c>String</c> with all numeric values removed.
        ''' </summary>
        ''' <param name="value">The <c>String</c> to trim.</param>
        ''' <returns>A specified <c>String</c> with all numeric values removed.</returns>
        ''' <remarks></remarks>
        Protected Friend Function TrimNumber(ByVal value As String) As String
            Dim builder As StringBuilder = New StringBuilder()
            For Each ch As Char In value.ToCharArray()
                If Not (IsNumeric(ch)) Then _
                    builder.Append(ch)
            Next
            Return builder.ToString()
        End Function

#End Region

#Region "Reader Methods"

        ''' <summary>
        ''' Closes the reader and sets it to null.
        ''' </summary>
        ''' <remarks></remarks>
        Public Sub CloseReader()
            Me.xReader.Close()
            Me.xReader = Nothing
        End Sub

        ''' <summary>
        ''' Reads a row of data in an Excel worksheet, fills the specified <c>ICollection</c> 
        ''' with a collection of <c>String</c> data objects for each cell in the row and 
        ''' returns a value indicating whether the row was successfully read.  If a cell 
        ''' does not have a value an empty string ("") is inserted in place.
        ''' </summary>
        ''' <param name="row">The <c>ICollection</c> to fill with cell data for the current row.</param>
        ''' <param name="generateBlock">A value indicating whether this read should set the block data for the import.  
        ''' Generating a block captures the columns included in the row and assigns a positional value to each 
        ''' successive column encountered.  Successive reads will be compared against the block, and if any cells are 
        ''' skipped, blank data will be filled in to line up the data correctly.  If a cell is out of bounds, the 
        ''' procedure will throw an error.  If this is undesirable, do not generate the block.</param>
        ''' <returns>True if the row was successfully read; otherwise, false.</returns>
        ''' <remarks></remarks>
        Public Function ReadRow(ByRef row As ICollection(Of String), _
                                Optional ByVal generateBlock As Boolean = False) As Boolean
            ' If not able to read row
            If Not (CanReadRow) OrElse Me.xReader Is Nothing Then _
                Throw New InvalidOperationException("Current worksheet must be set to enable Read() operations.")
            If generateBlock Then _
                Me._readBlock = New Dictionary(Of String, Integer)
            ' Return val
            Dim retVal As Boolean = True
            ' Get row data
            Me._currentRow = New List(Of String)
            Dim isEndRow As Boolean
            Dim previousPos As String = String.Empty
            Dim contentType As String
            ' If can read another row
            If Me.xReader.ReadToFollowing(XL_ROW) Then
                Dim currPos As String = String.Empty
                Do While Not (isEndRow)
                    If Not (Me.xReader.Read) Then _
                        Exit Do
                    If Me.xReader.NodeType = XmlNodeType.EndElement AndAlso Me.xReader.LocalName = XL_ROW Then
                        isEndRow = True
                    Else    ' Get cell data
                        If Me.xReader.NodeType = XmlNodeType.Element AndAlso Me.xReader.LocalName = XL_CELL Then
                            ' Set default content type
                            contentType = String.Empty
                            ' If content type attribute
                            If Me.xReader.MoveToAttribute(XL_CELL_TYPE) Then _
                                contentType = Me.xReader.Value
                            ' Check row position
                            If Me.xReader.MoveToAttribute(XL_POS) Then
                                currPos = TrimNumber(Me.xReader.Value)
                                ' If row used to generate block
                                If generateBlock Then
                                    Me.ReadBlock.Add(currPos, Me._currentRow.Count)
                                Else    ' Validate against block
                                    If Me.ReadBlock IsNot Nothing Then
                                        If Not (Me.ReadBlock.ContainsKey(currPos)) Then _
                                            Throw New FormatException("Column index is out of bounds.  Excel document is poorly formatted")
                                        FillNullCells(Me.ReadBlock(currPos), Me._currentRow)
                                    End If
                                End If
                                'CompareFillNullCells(currPos, previousPos, Me._currentRow)
                                'previousPos = currPos
                            End If
                            ' Get row value
                            If Me.xReader.ReadToFollowing(XL_CELL_VALUE) Then
                                ' If string content, attempt lookup from table
                                If contentType = "s" AndAlso Me.StringTable IsNot Nothing Then
                                    If Me.xReader.Read Then _
                                        Me._currentRow.Add(Me.StringTable(CInt(Me.xReader.Value)))
                                ElseIf contentType = "b" Then
                                    If Me.xReader.Read Then _
                                        Me._currentRow.Add(If(Me.xReader.Value = "1", "True", "False"))
                                Else    ' Read actual value
                                    If Me.xReader.Read Then _
                                        Me._currentRow.Add(Me.xReader.Value)
                                End If
                            End If
                        End If
                    End If
                Loop
            Else    ' No row data for sheet
                Me._currentRow = Nothing
                retVal = False
            End If
            row = Me._currentRow
            Return retVal
        End Function

        ''' <summary>
        ''' Reads a row of data in an Excel worksheet and returns a collection 
        ''' of <c>String</c> data objects for each cell in the row.  If a cell 
        ''' does not have a value an empty string ("") is inserted in place.
        ''' </summary>
        ''' <returns>A collection of <c>String</c> data objects for each cell in the row.</returns>
        ''' <remarks></remarks>
        Public Function ReadRow() As ICollection(Of String)
            Dim retVal As ICollection(Of String) = Nothing
            If Not (ReadRow(retVal)) Then _
                retVal = Nothing
            Return retVal
        End Function

        ''' <summary>
        ''' Internally reads the Excel file and returns a <c>DataSet</c> 
        ''' with a <c>DataTable</c> for each worksheet containing data.
        ''' </summary>
        ''' <returns>A <c>DataSet</c> with a <c>DataTable</c> for each worksheet containing data.</returns>
        ''' <remarks></remarks>
        Public Function ReadIntoDataSet() As DataSet
            Dim ds As DataSet = New DataSet(Me.Package.Name)
            Dim entries As ICollection(Of PackageFileInfo) = Me.Package.GetFileEntries(WORKSHEET_PATH)

            ' Iterate worksheets, create tables, add to dataset
            For Each info As PackageFileInfo In entries
                Me.CurrentWorksheet = info.GetNameWithoutExtension()
                Dim dt As DataTable = New DataTable(Me.CurrentWorksheet)

                ' Set columns
                Using reader As XmlReader = XmlTextReader.Create(New StringReader(GetContent(info.RelativePath)))
                    reader.ReadToFollowing(SHEET_DATA)
                    If Not (reader.ReadToDescendant(XL_ROW)) Then _
                        Continue For
                    reader.ReadToDescendant(XL_CELL)
                    Dim counter As Integer = 0
                    Do
                        counter += 1
                        dt.Columns.Add(String.Format("{0}{1}", DEFAULT_COL_NAME, counter))
                    Loop While reader.ReadToNextSibling(XL_CELL)
                End Using

                ' Initialize new reader for read
                Using rowReader As New ExcelFastReader(Me.Package.Path, True, Me.CurrentWorksheet)
                    Dim row As ICollection(Of String) = Nothing
                    ' If first row has headers
                    If Me.FirstRowHeaders AndAlso rowReader.ReadRow(row, True) Then
                        dt = New DataTable(Me.CurrentWorksheet)
                        For Each s As String In row
                            dt.Columns.Add(s)
                        Next
                    End If
                    ' Read rows
                    Do While rowReader.ReadRow(row)
                        dt.Rows.Add(row.ToArray())
                    Loop
                End Using

                ' Add to data set
                ds.Tables.Add(dt)
            Next
            Return ds
        End Function

#End Region      

#Region "IDisposable Support"
        Private disposedValue As Boolean ' To detect redundant calls

        ' IDisposable
        Protected Overridable Sub Dispose(disposing As Boolean)
            If Not Me.disposedValue Then
                If disposing Then
                    Me.CloseReader()
                End If
            End If
            Me.disposedValue = True
        End Sub

        ' This code added by Visual Basic to correctly implement the disposable pattern.
        Public Sub Dispose() Implements IDisposable.Dispose
            ' Do not change this code.  Put cleanup code in Dispose(ByVal disposing As Boolean) above.
            Dispose(True)
            GC.SuppressFinalize(Me)
        End Sub
#End Region

    End Class

End Namespace