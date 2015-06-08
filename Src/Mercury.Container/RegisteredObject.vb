
''' <summary>
''' Represents an abstract/concrete pair of objects registered to a container.
''' </summary>
''' <remarks></remarks>
Public Class RegisteredObject

    ' Attributes
    Private _typeToResolve As Type
    Private _concreteType As Type
    Private _instance As Object
    Private _life As LifeTime

    ''' <summary>
    ''' Initializes a default instance of the class with the 
    ''' specified resolve type, concrete type, and LifeTime.
    ''' </summary>
    ''' <param name="resolveT">Type of object to resolve.</param>
    ''' <param name="concreteT">Type of concrete object to return.</param>
    ''' <param name="life">LifeTime of the object.</param>
    ''' <remarks></remarks>
    Public Sub New(ByVal resolveT As Type, ByVal concreteT As Type, ByVal life As LifeTime)
        Me._typeToResolve = resolveT
        Me._concreteType = concreteT
        Me._life = life
    End Sub

#Region "Properties"

    ''' <summary>
    ''' Gets the Type of the object to resolve to a concrete object.
    ''' </summary>
    ''' <value>Type of object to resolve to concrete.</value>
    ''' <returns>Type of object to resolve to concrete.</returns>
    ''' <remarks></remarks>
    Public ReadOnly Property TypeToResolve() As Type
        Get
            Return Me._typeToResolve
        End Get
    End Property

    ''' <summary>
    ''' Gets the Type of the concrete object.
    ''' </summary>
    ''' <value>Type of concrete object.</value>
    ''' <returns>Type of concrete object.</returns>
    ''' <remarks></remarks>
    Public ReadOnly Property ConcreteType() As Type
        Get
            Return Me._concreteType
        End Get
    End Property

    ''' <summary>
    ''' Gets the LifeTime of the concrete type instance.
    ''' </summary>
    ''' <value>LifeTime of concrete instance.</value>
    ''' <returns>LifeTime of concrete instance.</returns>
    ''' <remarks></remarks>
    Public ReadOnly Property Life() As LifeTime
        Get
            Return Me._life
        End Get
    End Property

    ''' <summary>
    ''' Gets an instance of the concrete type.
    ''' </summary>
    ''' <value>Concrete type instance.</value>
    ''' <returns>Concrete type instance.</returns>
    ''' <remarks></remarks>
    Public ReadOnly Property Instance() As Object
        Get
            Return Me._instance
        End Get
    End Property

#End Region

    ''' <summary>
    ''' Creates an instance of the concrete type 
    ''' with the specified arguments.
    ''' </summary>
    ''' <param name="args">Arguments to apply to concrete type.</param>
    ''' <remarks></remarks>
    Public Sub CreateInstance(ByVal ParamArray args As Object())
        Me._instance = Activator.CreateInstance(Me.ConcreteType, args)
    End Sub

End Class
