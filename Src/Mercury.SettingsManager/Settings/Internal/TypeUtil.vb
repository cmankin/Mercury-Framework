Imports System.Security
Imports System.Reflection
Imports System.Security.Permissions

Namespace Settings

    ''' <summary>
    ''' An internal type utility for settings classes.
    ''' </summary>
    ''' <remarks></remarks>
    Friend Class TypeUtil

        Shared Sub New()
        End Sub

        Private Shared s_FullTrustPermissionSet As PermissionSet

        Friend Shared ReadOnly Property IsCallerFullTrust As Boolean
            Get
                Dim result As Boolean = False
                Try
                    If (TypeUtil.s_FullTrustPermissionSet Is Nothing) Then
                        TypeUtil.s_FullTrustPermissionSet = New PermissionSet(Permissions.PermissionState.Unrestricted)
                    End If
                    TypeUtil.s_FullTrustPermissionSet.Demand()
                    result = True
                Catch ex As Exception
                End Try
                Return result
            End Get
        End Property

        Public Shared Function GetQualifiedTypeString(ByVal type As Type, ByVal throwOnError As Boolean) As String
            If type Is Nothing AndAlso throwOnError Then _
                Throw New ArgumentNullException("type")
            If type IsNot Nothing Then
                Return String.Format("{0}, {1}", type.FullName, ParseAssemblyFullName(type, 0))
            End If
            Return String.Empty
        End Function

        Private Shared Function ParseAssemblyFullName(ByVal type As Type, ByVal index As Integer) As String
            Dim parsedValues As String() = ParseAssemblyFullName(type)
            Return parsedValues(index)
        End Function

        Private Shared Function ParseAssemblyFullName(ByVal type As Type) As String()
            Return Split(type.Assembly.FullName, ",")
        End Function

        Private Shared Function GetLegacyType(ByVal typeString As String) As Type
            Dim result As Type = Nothing
            Try
                Dim assembly As Assembly = GetType(BaseSettingsRecord).Assembly
                result = assembly.GetType(typeString, False)
            Catch ex As Exception
            End Try
            Return result
        End Function

        Private Shared Function GetTypeImpl(ByVal typeString As String, ByVal throwOnError As Boolean) As Type
            Dim type As Type = Nothing
            Dim ex As Exception = Nothing
            Try
                type = System.Type.GetType(typeString, throwOnError)
            Catch ex2 As Exception
                ex = ex2
            End Try

            If (type Is Nothing) Then
                type = TypeUtil.GetLegacyType(typeString)
                If (type = Nothing AndAlso ex IsNot Nothing) Then _
                    Throw ex
            End If
            Return type
        End Function

        <ReflectionPermission(SecurityAction.Assert, Flags:=ReflectionPermissionFlag.NoFlags)>
        Friend Shared Function GetTypeWithReflectionPermission(ByVal typeString As String, ByVal throwOnError As Boolean) As Type
            Return TypeUtil.GetTypeImpl(typeString, throwOnError)
        End Function

        <ReflectionPermission(SecurityAction.Assert, Flags:=ReflectionPermissionFlag.MemberAccess)>
        Friend Shared Function CreateInstanceWithReflectionPermission(ByVal typeString As String) As Object
            Dim typeImpl As Type = TypeUtil.GetTypeImpl(typeString, True)
            Return Activator.CreateInstance(typeImpl, True)
        End Function

        <ReflectionPermission(SecurityAction.Assert, Flags:=ReflectionPermissionFlag.MemberAccess)>
        Friend Shared Function CreateInstanceWithReflectionPermission(ByVal type As Type) As Object
            Return Activator.CreateInstance(type, True)
        End Function

        Friend Shared Function GetConstructorWithReflectionPermission(ByVal type As Type, ByVal baseType As Type, ByVal throwOnError As Boolean) As ConstructorInfo

            If (type Is Nothing) Then _
                Return Nothing

            Dim bindingAttr As BindingFlags = BindingFlags.Instance Or BindingFlags.Public Or BindingFlags.NonPublic
            Dim constructor As ConstructorInfo = type.GetConstructor(bindingAttr, Nothing, CallingConventions.HasThis, System.Type.EmptyTypes, Nothing)
            If (constructor Is Nothing AndAlso throwOnError) Then

            End If

            Throw New NotImplementedException()
        End Function

        Friend Shared Function VerifyImplementsInterface(ByVal baseType As Type, ByVal interfaceType As Type, ByVal throwOnError As Boolean)
            If (interfaceType.IsInterface) Then
                Dim itf As Type = baseType.GetInterface(interfaceType.FullName)
                If itf IsNot Nothing Then _
                    Return baseType
            End If
            If (throwOnError) Then
                Throw New TypeLoadException(My.Resources.Settings_type_not_inherit_from_type)
            End If
            Return Nothing
        End Function

        Friend Shared Function VerifyAssignableType(ByVal baseType As Type, ByVal type As Type, ByVal throwOnError As Boolean) As Type
            If (baseType.IsAssignableFrom(type)) Then _
                Return type
            If (throwOnError) Then
                Throw New TypeLoadException(My.Resources.Settings_type_not_inherit_from_type)
            End If
            Return Nothing
        End Function

        <ReflectionPermission(SecurityAction.Assert, Flags:=ReflectionPermissionFlag.MemberAccess)>
        Private Shared Function HasAptcaBit(ByVal assembly As Assembly) As Boolean
            Dim customAttributes As Object() = assembly.GetCustomAttributes(GetType(AllowPartiallyTrustedCallersAttribute), False)
            Return (customAttributes IsNot Nothing AndAlso customAttributes.Length > 0)
        End Function

        Friend Shared Function IsTypeAllowedInConfig(ByVal t As Type) As Boolean
            If (TypeUtil.IsCallerFullTrust) Then _
                Return True
            Dim assembly As Assembly = t.Assembly
            Return (Not (assembly.GlobalAssemblyCache) OrElse TypeUtil.HasAptcaBit(assembly))
        End Function

    End Class

End Namespace