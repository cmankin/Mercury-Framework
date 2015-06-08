
''' <summary>
''' A collection of query settings.
''' </summary>
''' <remarks></remarks>
Friend Class QuerySettingsCollection
    Implements IList(Of QuerySettings)

#Region "Constructors"

    Public Sub New(ByVal internalCollection As QuerySettingsElementCollection)
        Me._internalCollection = internalCollection
    End Sub

#End Region

#Region "IList<T>"

    Protected _internalCollection As QuerySettingsElementCollection

    Public Sub Add(item As QuerySettings) Implements System.Collections.Generic.ICollection(Of QuerySettings).Add
        Me._internalCollection.AddOrUpdate(item.InternalSettings)
    End Sub

    Public Sub Clear() Implements System.Collections.Generic.ICollection(Of QuerySettings).Clear
        Me._internalCollection.Clear()
    End Sub

    Public Function Contains(item As QuerySettings) As Boolean Implements System.Collections.Generic.ICollection(Of QuerySettings).Contains
        Dim index As Integer = Me.IndexOf(item)
        If index > -1 Then _
            Return True
        Return False
    End Function

    Public Sub CopyTo(array() As QuerySettings, arrayIndex As Integer) Implements System.Collections.Generic.ICollection(Of QuerySettings).CopyTo
        Throw New NotImplementedException()
    End Sub

    Public ReadOnly Property Count As Integer Implements System.Collections.Generic.ICollection(Of QuerySettings).Count
        Get
            Return Me._internalCollection.Count
        End Get
    End Property

    Public ReadOnly Property IsReadOnly As Boolean Implements System.Collections.Generic.ICollection(Of QuerySettings).IsReadOnly
        Get
            Return False
        End Get
    End Property

    Public Function Remove(item As QuerySettings) As Boolean Implements System.Collections.Generic.ICollection(Of QuerySettings).Remove
        Me._internalCollection.Remove(item.InternalSettings)
        Return True
    End Function

    Public Function IndexOf(item As QuerySettings) As Integer Implements System.Collections.Generic.IList(Of QuerySettings).IndexOf
        Return Me._internalCollection.IndexOf(item.InternalSettings)
    End Function

    Public Sub Insert(index As Integer, item As QuerySettings) Implements System.Collections.Generic.IList(Of QuerySettings).Insert
        Throw New NotImplementedException()
    End Sub

    Default Public Property Item(index As Integer) As QuerySettings Implements System.Collections.Generic.IList(Of QuerySettings).Item
        Get
            If index > -1 AndAlso index < Me._internalCollection.Count Then _
                Return New QuerySettings(Me._internalCollection.Query(index))
            Return Nothing
        End Get
        Set(value As QuerySettings)
            If value IsNot Nothing Then
                If index > -1 AndAlso index < Me._internalCollection.Count Then _
                    Me._internalCollection.Query(index) = value.InternalSettings
            End If
        End Set
    End Property

    Default Public Property Item(ByVal key As String) As QuerySettings
        Get
            Dim current As QuerySettingsElement = Me._internalCollection.Query(key)
            If current IsNot Nothing Then _
                Return New QuerySettings(current)
            Return Nothing
        End Get
        Set(value As QuerySettings)
            Dim index As Integer = Me.IndexOf(value)
            If index > -1 Then _
                Me._internalCollection.Query(index) = value.InternalSettings
        End Set
    End Property

    Public Sub RemoveAt(index As Integer) Implements System.Collections.Generic.IList(Of QuerySettings).RemoveAt
        Me._internalCollection.RemoveAt(index)
    End Sub

#End Region

#Region "IEnumerator"

    Public Function GetEnumerator() As System.Collections.Generic.IEnumerator(Of QuerySettings) Implements System.Collections.Generic.IEnumerable(Of QuerySettings).GetEnumerator
        Return New QuerySettingsEnumerator(Me._internalCollection)
    End Function

    Public Function GetEnumerator1() As System.Collections.IEnumerator Implements System.Collections.IEnumerable.GetEnumerator
        Return New QuerySettingsEnumerator(Me._internalCollection)
    End Function

    ''' <summary>
    ''' Private enumerator item.
    ''' </summary>
    ''' <remarks></remarks>
    Private Class QuerySettingsEnumerator
        Implements IEnumerator(Of QuerySettings)

        Private iPos As Integer = -1
        Private internalCollection As QuerySettingsElementCollection

        Public Sub New(ByVal coll As QuerySettingsElementCollection)
            Me.internalCollection = coll
        End Sub

        Public ReadOnly Property Current As QuerySettings Implements System.Collections.Generic.IEnumerator(Of QuerySettings).Current
            Get
                Dim value As QuerySettingsElement = Me.internalCollection.Query(Me.iPos)
                If value IsNot Nothing Then _
                    Return New QuerySettings(value)
                Return Nothing
            End Get
        End Property

        Public ReadOnly Property Current1 As Object Implements System.Collections.IEnumerator.Current
            Get
                Dim value As QuerySettingsElement = Me.internalCollection.Query(Me.iPos)
                If value IsNot Nothing Then _
                    Return New QuerySettings(value)
                Return Nothing
            End Get
        End Property

        Public Function MoveNext() As Boolean Implements System.Collections.IEnumerator.MoveNext
            If (Me.iPos < Me.internalCollection.Count - 1) Then
                Me.iPos += 1
                Return True
            Else
                Return False
            End If
        End Function

        Public Sub Reset() Implements System.Collections.IEnumerator.Reset
            Me.iPos = -1
        End Sub

#Region "IDisposable Support"
        Private disposedValue As Boolean ' To detect redundant calls

        ' IDisposable
        Protected Overridable Sub Dispose(disposing As Boolean)
            If Not Me.disposedValue Then
                If disposing Then
                    ' TODO: dispose managed state (managed objects).
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

#End Region

End Class