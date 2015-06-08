Imports System.Reflection

''' <summary>
''' Represents the basic container implementing the IContainer interface.
''' </summary>
''' <remarks></remarks>
Public Class MercuryContainer
    Implements IContainer

    ''' <summary>
    ''' Private collection of RegisteredObject entities.
    ''' </summary>
    ''' <remarks></remarks>
    Private registeredObjects As IList(Of RegisteredObject) = New List(Of RegisteredObject)

#Region "IContainer Interface Members"

    Public Sub Register(Of TToResolve, TConcrete)() Implements IContainer.Register
        Me.Register(Of TToResolve, TConcrete)(LifeTime.Singleton)
    End Sub

    Public Sub Register(Of TToResolve, TConcrete)(ByVal life As LifeTime) Implements IContainer.Register
        Register(GetType(TToResolve), GetType(TConcrete), life)
    End Sub

    Public Sub Register(Of TToResolve)(TConcrete As System.Type, life As LifeTime) Implements IContainer.Register
        Register(GetType(TToResolve), TConcrete, life)
    End Sub

    Public Sub Register(TToResolve As System.Type, TConcrete As System.Type, life As LifeTime) Implements IContainer.Register
        Dim addNew As Boolean = False
        ' Get current
        Dim regObj As RegisteredObject = Me.registeredObjects.FirstOrDefault(Function(o) o.TypeToResolve.Equals(TToResolve))
        If regObj Is Nothing Then _
            addNew = True

        ' Save
        regObj = New RegisteredObject(TToResolve, TConcrete, life)
        If addNew Then _
            Me.registeredObjects.Add(regObj)
    End Sub

    Public Sub Register(identifier As String, TToResolve As System.Type, TConcrete As System.Type, life As LifeTime) Implements IContainer.Register
        RegisterNamedObject(identifier, TToResolve, TConcrete, life)
    End Sub

    Public Function Resolve(identifier As String) As Object Implements IContainer.Resolve
        Return ResolveNamedObject(identifier)
    End Function

    Public Function Resolve(Of TToResolve)(identifier As String) As TToResolve Implements IContainer.Resolve
        Return CType(Resolve(identifier), TToResolve)
    End Function

    Public Function Resolve(ByVal typeToResolve As System.Type) As Object Implements IContainer.Resolve
        Return Me.ResolveObject(typeToResolve)
    End Function

    Public Function Resolve(Of TToResolve)() As TToResolve Implements IContainer.Resolve
        Return CType(Me.ResolveObject(GetType(TToResolve)), TToResolve)
    End Function

#End Region

#Region "Resolver Methods"

    Private Function ResolveNamedObject(ByVal identifier As String) As Object
        If Not (Me.InternalRegistrationMap.ContainsKey(identifier)) Then _
            Throw New TypeNotRegisteredException( _
                String.Format("No types with the specified '{0}' identifier have been registered.", identifier))
        Return GetInstance(Me.registeredObjects(Me.InternalRegistrationMap(identifier)))
    End Function

    Private Function ResolveObject(ByVal typeToResolve As Type) As Object
        Dim regObj As RegisteredObject = registeredObjects.FirstOrDefault(Function(o) o.TypeToResolve.Equals(typeToResolve))
        ' If object is not registered,
        If regObj Is Nothing Then
            Throw New TypeNotRegisteredException( _
                String.Format("The type {0} has not been registered.", typeToResolve.Name))
        End If
        ' Return
        Return Me.GetInstance(regObj)
    End Function

    Private Function GetInstance(ByVal regObj As RegisteredObject) As Object
        ' If no instance or lifetime is transient
        If (regObj.Instance Is Nothing) OrElse (regObj.Life = LifeTime.Transient) Then
            Dim params As IEnumerable(Of Object) = Me.ResolveConstructorParameters(regObj)
            regObj.CreateInstance(params.ToArray())
        End If
        ' Return instance
        Return regObj.Instance
    End Function

    Private Function ResolveConstructorParameters(ByVal regObj As RegisteredObject) As IEnumerable(Of Object)
        ' Cycle through constructors and resolve object
        Dim ret As IList(Of Object) = New List(Of Object)
        Dim info As ConstructorInfo = regObj.ConcreteType.GetConstructors.First()
        For Each param As ParameterInfo In info.GetParameters
            ret.Add(Me.ResolveObject(param.ParameterType))
        Next

        ' Return
        Return ret
    End Function

#End Region

#Region "Named Registrations"

    Private _internalRegistrationMap As IDictionary(Of String, Integer)

    Protected ReadOnly Property InternalRegistrationMap() As IDictionary(Of String, Integer)
        Get
            If Me._internalRegistrationMap Is Nothing Then _
                Me._internalRegistrationMap = New Dictionary(Of String, Integer)
            Return Me._internalRegistrationMap
        End Get
    End Property

    Protected Sub RegisterNamedObject(ByVal identifier As String, ByVal TToResolve As System.Type, _
                                      ByVal TConcrete As System.Type, ByVal life As LifeTime)
        Me.registeredObjects.Add(New RegisteredObject(TToResolve, TConcrete, life))

    End Sub

    Protected Sub UpdateInternalRegistrationMap(ByVal identifier As String, ByVal index As Integer)
        Me.InternalRegistrationMap.Add(identifier, index)
    End Sub

#End Region

End Class
