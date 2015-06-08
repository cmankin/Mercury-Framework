
''' <summary>
''' Represents a basic container.
''' </summary>
''' <remarks></remarks>
Public Interface IContainer

    ''' <summary>
    ''' Registers an abstract type with its concrete type implementation.
    ''' </summary>
    ''' <typeparam name="TToResolve">Abstract type to resolve.</typeparam>
    ''' <typeparam name="TConcrete">Mapped concrete type.</typeparam>
    ''' <remarks></remarks>
    Sub Register(Of TToResolve, TConcrete)()

    ''' <summary>
    ''' Registers an abstract type with its concrete type implementation 
    ''' and with the specified object LifeTime.
    ''' </summary>
    ''' <typeparam name="TToResolve">Abstract type to resolve.</typeparam>
    ''' <typeparam name="TConcrete">Mapped concrete type.</typeparam>
    ''' <param name="life">Object lifetime.</param>
    ''' <remarks></remarks>
    Sub Register(Of TToResolve, TConcrete)(ByVal life As LifeTime)

    ''' <summary>
    ''' Registers an abstract type with its concrete type implementation 
    ''' and with the specified object LifeTime.
    ''' </summary>
    ''' <typeparam name="TToResolve">Abstract type to resolve.</typeparam>
    ''' <param name="TConcrete">Mapped concrete type.</param>
    ''' <param name="life">Object lifetime.</param>
    ''' <remarks></remarks>
    Sub Register(Of TToResolve)(ByVal TConcrete As Type, ByVal life As LifeTime)

    ''' <summary>
    ''' Registers an abstract type with its concrete type implementation 
    ''' and with the specified object LifeTime.
    ''' </summary>
    ''' <param name="TToResolve">Abstract type to resolve.</param>
    ''' <param name="TConcrete">Mapped concrete type.</param>
    ''' <param name="life">Object lifetime.</param>
    ''' <remarks></remarks>
    Sub Register(ByVal TToResolve As Type, ByVal TConcrete As Type, ByVal life As LifeTime)

    ''' <summary>
    ''' Registers an abstract type with its concrete type implementation 
    ''' and with the specified identifier and object LifeTime.
    ''' </summary>
    ''' <param name="identifier">The identifier of the concrete type.</param>
    ''' <param name="TToResolve">Abstract type to resolve.</param>
    ''' <param name="TConcrete">Mapped concrete type.</param>
    ''' <param name="life">Object lifetime.</param>
    ''' <remarks></remarks>
    Sub Register(ByVal identifier As String, ByVal TToResolve As Type, ByVal TConcrete As Type, ByVal life As LifeTime)

    ''' <summary>
    ''' Resolves an abstract type to its concrete implementation.
    ''' </summary>
    ''' <typeparam name="TToResolve">Abstract type to resolve.</typeparam>
    ''' <returns>Abstract type wired to implementation.</returns>
    ''' <remarks></remarks>
    Function Resolve(Of TToResolve)() As TToResolve

    ''' <summary>
    ''' Resolves an abstract type to its concrete implementation specified by the string identifier.
    ''' </summary>
    ''' <typeparam name="TToResolve">Abstract type to resolve.</typeparam>
    ''' <param name="identifier">The identifier of the concrete type.</param>
    ''' <returns>Abstract type wired to implementation.</returns>
    ''' <remarks></remarks>
    Function Resolve(Of TToResolve)(ByVal identifier As String) As TToResolve

    ''' <summary>
    ''' Resolves an abstract type to its concrete implementation.
    ''' </summary>
    ''' <param name="typeToResolve">Abstract Type object to resolve.</param>
    ''' <returns>Object implementing the abstract type.</returns>
    ''' <remarks></remarks>
    Function Resolve(ByVal typeToResolve As Type) As Object

    ''' <summary>
    ''' Resolves an abstract type to its concrete implementation specified by the string identifier.
    ''' </summary>
    ''' <param name="identifier">The identifier of the concrete type.</param>
    ''' <returns>Object implementing the abstract type.</returns>
    ''' <remarks></remarks>
    Function Resolve(ByVal identifier As String) As Object

End Interface
