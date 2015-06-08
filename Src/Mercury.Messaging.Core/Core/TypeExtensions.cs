using System;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;
using Mercury;

namespace Mercury.Messaging.Core
{
    /// <summary>
    /// Core extensions to the System.Type object.
    /// </summary>
    public static class TypeExtensions
    {
        /// <summary>
        /// Returns an array of interface types directly implemented by this type.
        /// </summary>
        /// <param name="type">The type on which to perform the operation.</param>
        /// <returns>An array of interface types directly implemented by this type.</returns>
        public static Type[] GetDirectInterfaces(this Type type)
        {
            List<Type> allInterfaces = new List<Type>();
            List<Type> childInterfaces = new List<Type>();
            foreach (Type t in type.GetInterfaces())
            {
                allInterfaces.Add(t);
                foreach (Type ct in t.GetInterfaces())
                    childInterfaces.Add(ct);
            }
            return allInterfaces.Except(childInterfaces).ToArray();
        }

        /// <summary>
        /// Returns the first interface directly implemented by this type.
        /// </summary>
        /// <param name="type">The type on which to perform the operation.</param>
        /// <returns>The first interface directly implemented by this type.  
        /// If not interfaces exist, returns the base type.</returns>
        public static Type GetFirstInterfaceOrDefault(this Type type)
        {
            Type[] interfaces = type.GetDirectInterfaces();
            if (interfaces != null && interfaces.Length > 0)
                return interfaces[0];
            return type;
        }

        /// <summary>
        /// Returns a value indicating whether the specified type name 
        /// matches any directly implemented interface type names.
        /// </summary>
        /// <param name="type">The type on which to perform the operation.</param>
        /// <param name="partialTypeName">The partial, or full, type name to match.</param>
        /// <returns>True if the specified type name is directly implemented by this type; otherwise, false.</returns>
        public static bool HasDirectInterface(this Type type, string partialTypeName)
        {
            Type[] interfaces = type.GetDirectInterfaces();
            if (interfaces != null)
            {
                foreach (Type t in interfaces)
                {
                    if (t.FullName.Mid(0, partialTypeName.Length) == partialTypeName)
                        return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Returns a value indicating whether the specified type definition matches any implemented interfaces.
        /// </summary>
        /// <param name="type">The type on which to perform the operation.</param>
        /// <param name="typeDefinition">The type definition to find.</param>
        /// <returns>True if the specified type definition is an interface on this type; otherwise, false.</returns>
        public static bool HasGenericInterface(this Type type, Type typeDefinition)
        {
            Type iType = type.GetGenericInterfaceFromTypeDefinition(typeDefinition);
            if (iType != null)
                return true;
            return false;
        }

        /// <summary>
        /// Returns a value indicating whether this type is or is derived from the specified type definition.
        /// </summary>
        /// <param name="type">The type on which to perform the operation.</param>
        /// <param name="typeDefinition">The type definition for the inherited type.</param>
        /// <returns>True if this type is derived from the specified type definition; otherwise, false.</returns>
        public static bool IsDerivedFrom(this Type type, Type typeDefinition)
        {
            // Evaluate type
            if (type == typeDefinition)
                return true;

            // Evaluate base types
            Type baseType = type.BaseType;
            while (baseType != null)
            {
                if (baseType == typeDefinition)
                    return true;
                baseType = baseType.BaseType;
            }

            // Evaluate interfaces
            Type[] interfaces = type.GetInterfaces();
            if (interfaces != null)
            {
                foreach (Type t in interfaces)
                {
                    if (t == typeDefinition)
                        return true;

                    if (t.IsGenericType && t.GetGenericTypeDefinition() == typeDefinition)
                        return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Returns the interface type that matches the specified partial type name.
        /// </summary>
        /// <param name="type">The type on which to perform the operation.</param>
        /// <param name="partialTypeName">The partial, or full, type name to match.</param>
        /// <param name="ignoreCase">A value indicating whether to ignore casing.</param>
        /// <returns>The interface type that matches the specified partial type name.</returns>
        public static Type GetInterfaceFromPartial(this Type type, string partialTypeName, bool ignoreCase)
        {
            Type[] interfaces = type.GetInterfaces();
            if (interfaces != null)
            {
                foreach (Type t in interfaces)
                {
                    if (ignoreCase)
                    {
                        if (t.FullName.Mid(0, partialTypeName.Length).ToUpper(CultureInfo.InvariantCulture) == partialTypeName.ToUpper(CultureInfo.InvariantCulture))
                            return t;
                    }
                    else
                    {
                        if (t.FullName.Mid(0, partialTypeName.Length) == partialTypeName)
                            return t;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Returns the generic interface with the specified type definition on this type.
        /// </summary>
        /// <param name="type">The type on which to perform the operation.</param>
        /// <param name="typeDefinition">The type definition to find.</param>
        /// <returns>The generic interface with the specified type definition 
        /// on this type, or null if no matching definition is found.</returns>
        public static Type GetGenericInterfaceFromTypeDefinition(this Type type, Type typeDefinition)
        {
            Type[] interfaces = type.GetInterfaces();
            if (interfaces != null)
            {
                foreach (Type t in interfaces)
                {
                    if (t.IsGenericType && t.GetGenericTypeDefinition() == typeDefinition)
                        return t;
                }
            }
            return null;
        }
    }
}
