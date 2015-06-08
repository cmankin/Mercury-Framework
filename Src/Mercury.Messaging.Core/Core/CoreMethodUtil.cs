using System;
using System.Reflection;

namespace Mercury.Messaging.Core
{
    /// <summary>
    /// Represents a set of methods for dynamic method execution.
    /// </summary>
    internal static class CoreMethodUtil
    {
        internal static void ExecuteNonReturnMethod(Type onType, object onInstance, string methodName, 
            Type[] withGenericTypes, params object[] parameters)
        {
            ExecuteMethod(onType, onInstance, methodName, withGenericTypes, parameters);
        }

        internal static object ExecuteMethod(Type onType, object onInstance, string methodName,
            Type[] withGenericTypes, params object[] parameters)
        {
            if (onType == null)
                throw new ArgumentNullException("onType");
            if (onInstance == null)
                throw new ArgumentNullException("onInstance");
            if (methodName == null)
                throw new ArgumentNullException("methodName");

            // Get method
            MethodInfo method = GetMethod(onType, methodName, withGenericTypes);
            if (method == null)
                throw new MissingMethodException(onType.FullName, methodName);
            // Invoke
            return method.Invoke(onInstance, parameters);
        }

        internal static MethodInfo GetMethod(Type onType, string methodName)
        {
            return GetMethod(onType, methodName, null);
        }
        
        internal static MethodInfo GetMethod(Type onType, string methodName, Type[] withGenericTypes)
        {
            if (onType == null)
                throw new ArgumentNullException("onType");
            if (methodName == null)
                throw new ArgumentNullException("methodName");

            MethodInfo method = onType.GetMethod(methodName);
            if (withGenericTypes != null && withGenericTypes.Length > 0)
                return method.MakeGenericMethod(withGenericTypes);
            return method;
        }
    }
}
