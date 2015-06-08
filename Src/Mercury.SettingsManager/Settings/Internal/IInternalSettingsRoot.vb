
Namespace Settings.Internal

    ''' <summary>
    ''' Internal interface to support a settings root object.
    ''' </summary>
    ''' <remarks></remarks>
    Public Interface IInternalSettingsRoot

        ''' <summary>
        ''' Represents a handle for a settings changed event.
        ''' </summary>
        ''' <remarks></remarks>
        Event SettingsChanged(ByVal sender As Object, ByVal e As InternalSettingsEventArgs)

        ''' <summary>
        ''' Represents a handle for a settings removed event.
        ''' </summary>
        ''' <remarks></remarks>
        Event SettingsRemoved(ByVal sender As Object, ByVal e As InternalSettingsEventArgs)

        ''' <summary>
        ''' Represents a handle for a settings added event.
        ''' </summary>
        ''' <remarks></remarks>
        Event SettingsAdded(ByVal sender As Object, ByVal e As InternalSettingsEventArgs)

        ''' <summary>
        ''' Returns a <see cref="System.Object"/> representing the data in a section of a settings file.
        ''' </summary>
        ''' <param name="section">The section to retrieve.</param>
        ''' <param name="settingsPath">The path to the settings file.</param>
        ''' <returns>A <see cref="System.Object"/> representing the data in a section of a settings file.</returns>
        ''' <remarks></remarks>
        Function GetSection(ByVal section As String, ByVal settingsPath As String)

        ''' <summary>
        ''' Gets the settings record on the settings file at the specified path.
        ''' </summary>
        ''' <param name="settingsPath">The path to the settings file.</param>
        ''' <returns>The settings record on the settings file at the specified path.</returns>
        ''' <remarks></remarks>
        Function GetSettingsRecord(ByVal settingsPath As String)

        ''' <summary>
        ''' Finds and deletes the settings file at the specified file path.
        ''' </summary>
        ''' <param name="settingsPath">The path to the settings file.</param>
        ''' <remarks></remarks>
        Sub RemoveSettings(ByVal settingsPath As String)

        ''' <summary>
        ''' Attempts to add an empty settings file at the specified file path.
        ''' </summary>
        ''' <param name="settingsPath">The path to the settings file.</param>
        ''' <returns>True if the new file was created; otherwise, false.</returns>
        ''' <remarks></remarks>
        Function AddSettings(ByVal settingsPath As String) As Boolean
    End Interface

End Namespace