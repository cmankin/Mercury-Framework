
''' <summary>
''' Life time value for container object(s).
''' </summary>
''' <remarks></remarks>
Public Enum LifeTime
    ''' <summary>
    ''' Single instance.  Does not expire.
    ''' </summary>
    ''' <remarks></remarks>
    Singleton = 0

    ''' <summary>
    ''' Multiple instances.  Expires out of caller scope.
    ''' </summary>
    ''' <remarks></remarks>
    Transient = 1
End Enum