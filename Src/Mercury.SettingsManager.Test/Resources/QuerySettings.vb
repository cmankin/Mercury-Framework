
''' <summary>
''' Represents settings for a query.
''' </summary>
''' <remarks></remarks>
Friend Class QuerySettings

    ''' <summary>
    ''' Initializes a default instance of the QuerySettings 
    ''' class with the specified internal settings element.
    ''' </summary>
    ''' <param name="internalSettings">The internal QuerySettingsElement to use.</param>
    ''' <remarks></remarks>
    Public Sub New(ByVal internalSettings As QuerySettingsElement)
        Me.InternalSettings = internalSettings
    End Sub

    Protected Friend InternalSettings As QuerySettingsElement

    ''' <summary>
    ''' Gets or sets the source connection information.
    ''' </summary>
    ''' <returns>The source connection information.</returns>
    ''' <remarks></remarks>
    Public Property Source As String
        Get
            Return Me.InternalSettings.Source
        End Get
        Set(value As String)
            Me.InternalSettings.Source = value
        End Set
    End Property

    ''' <summary>
    ''' Gets or sets the type of data source to query.
    ''' </summary>
    ''' <returns>The type of data source to query.</returns>
    ''' <remarks></remarks>
    Public Property Type As String
        Get
            Return Me.InternalSettings.Type
        End Get
        Set(value As String)
            Me.InternalSettings.Type = value
        End Set
    End Property

    ''' <summary>
    ''' Gets or sets the command associated with this query.
    ''' </summary>
    ''' <returns>The command associated with this query.</returns>
    ''' <remarks></remarks>
    Public Property Command As String
        Get
            Return Me.InternalSettings.Command
        End Get
        Set(value As String)
            Me.InternalSettings.Command = value
        End Set
    End Property

    ''' <summary>
    ''' Gets or sets a parameter associated with this query.
    ''' </summary>
    ''' <returns>A parameter associated with this query.</returns>
    ''' <remarks></remarks>
    Public Property Parameter As String
        Get
            Return Me.InternalSettings.Parameter
        End Get
        Set(value As String)
            Me.InternalSettings.Parameter = value
        End Set
    End Property

    ''' <summary>
    ''' Gets or sets the key identifier for this query.
    ''' </summary>
    ''' <returns>The key identifier for this query.</returns>
    ''' <remarks></remarks>
    Public Property Key As String
        Get
            Return Me.InternalSettings.Key
        End Get
        Set(value As String)
            Me.InternalSettings.Key = value
        End Set
    End Property

    ''' <summary>
    ''' Gets or sets the timeout value for this query.
    ''' </summary>
    ''' <returns>The timeout value for this query.</returns>
    ''' <remarks></remarks>
    Public Property Timeout As Integer
        Get
            Return Me.InternalSettings.Timeout
        End Get
        Set(value As Integer)
            Me.InternalSettings.Timeout = value
        End Set
    End Property

    ''' <summary>
    ''' Gets or sets the path to the assembly that will override the default service 
    ''' type assembly.  This path is relative to the executing assembly.
    ''' </summary>
    ''' <returns>The path to the assembly that will override the default service type assembly.</returns>
    ''' <remarks></remarks>
    Public Property OverrideTaskAssembly As String
        Get
            Return Me.InternalSettings.OverrideTaskAssembly
        End Get
        Set(value As String)
            Me.InternalSettings.OverrideTaskAssembly = value
        End Set
    End Property

    ''' <summary>
    ''' Gets or sets the type string for the service type that will 
    ''' override the default service in the associated task.
    ''' </summary>
    ''' <returns>The type string for the service type that will 
    ''' override the default service in the associated task.</returns>
    ''' <remarks></remarks>
    Public Property OverrideServiceType As String
        Get
            Return Me.InternalSettings.OverrideServiceType
        End Get
        Set(value As String)
            Me.InternalSettings.OverrideServiceType = value
        End Set
    End Property

    ''' <summary>
    ''' Creates and returns a QuerySettings object from the specified inputs.
    ''' </summary>
    ''' <param name="key">The key identifier for this query.</param>
    ''' <param name="source">The source connection information.</param>
    ''' <param name="type">The type of data source to query.</param>
    ''' <param name="parameter">A parameter associated with the query.</param>
    ''' <param name="timeout">The timeout value for this query.</param>
    ''' <param name="overrideTaskAssembly">The path to the assembly that will override the default service type assembly.</param>
    ''' <returns>A QuerySettings object derived from the specified inputs.</returns>
    ''' <remarks></remarks>
    Public Shared Function Create(ByVal key As String, ByVal source As String, ByVal type As String, ByVal parameter As String,
        ByVal timeout As Integer, ByVal overrideTaskAssembly As String, ByVal overrideServiceType As String) As QuerySettings
        Dim settings As QuerySettingsElement = New QuerySettingsElement()
        settings.Key = key
        settings.Source = source
        settings.Type = type
        settings.Parameter = parameter
        settings.Timeout = timeout
        settings.OverrideTaskAssembly = overrideTaskAssembly
        settings.OverrideServiceType = overrideServiceType
        Return New QuerySettings(settings)
    End Function

    Public Shared Function GetElement(ByVal querySettings As QuerySettings) As QuerySettingsElement
        If querySettings IsNot Nothing Then _
            Return querySettings.InternalSettings
        Return Nothing
    End Function

End Class