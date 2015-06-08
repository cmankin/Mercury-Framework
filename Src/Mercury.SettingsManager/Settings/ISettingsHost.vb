Imports System.Xml
Imports System.Configuration

Namespace Settings

    ''' <summary>
    ''' Represents a host for settings
    ''' </summary>
    ''' <remarks></remarks>
    Public Interface ISettingsHost
        ''' <summary>
        ''' Gets a value indicating whether settings are current.
        ''' </summary>
        ''' <returns>True if the settings are current with the settings file; otherwise, false.</returns>
        ''' <remarks></remarks>
        ReadOnly Property IsCurrent As Boolean

        ''' <summary>
        ''' An event that occurs when settings on an item have changed.
        ''' </summary>
        Event SettingsChanged(ByVal sender As Object, ByVal e As SettingsChangedEventArgs)

        ''' <summary>
        ''' Invalidates the settings host to a non-current state.
        ''' </summary>
        ''' <remarks></remarks>
        Sub Invalidate()

        ''' <summary>
        ''' Resets the settings host to a current state.
        ''' </summary>
        ''' <remarks></remarks>
        Sub Reset()

        ''' <summary>
        ''' Deserializes the settings host from the specified XML reader.
        ''' </summary>
        ''' <param name="reader">The XML reader from which to deserialize.</param>
        ''' <param name="serializeCollectionKey"></param>
        ''' <remarks></remarks>
        Sub Deserialize(ByVal reader As XmlReader, ByVal serializeCollectionKey As Boolean)

        ''' <summary>
        ''' Serializes the settings host as the specified element name with the specified XML writer.
        ''' </summary>
        ''' <param name="writer">The XML writer to use.</param>
        ''' <param name="elementName">The name of the serialized element.</param>
        ''' <returns>True if serialization was successful; otherwise, false.</returns>
        ''' <remarks></remarks>
        Function Serialize(ByVal writer As XmlWriter, ByVal elementName As String) As Boolean
    End Interface

End Namespace