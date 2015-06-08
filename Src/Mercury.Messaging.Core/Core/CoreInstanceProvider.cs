using System;
using System.Reflection;
using Mercury.Messaging.Runtime;

namespace Mercury.Messaging.Core
{
    /// <summary>
    /// Provides instance implementations from specified configurations.
    /// </summary>
    internal static class CoreInstanceProvider
    {
        internal static object Create(Type type, params object[] args)
        {
            // If agent, verify correct implementation
            if (type.GetInterface(typeof(Agent).FullName)!=null)
            {
                if (args.Length < 1)
                    throw new ArgumentException("Insufficient number of arguments for an agent constructor.", "args");

                if (!typeof(AgentPort).IsInstanceOfType(args[0]))
                    throw new ArgumentException("First constructor argument for an agent must be an AgentPort.", "args");
            }

            return CoreInstanceProvider.CreateInstance(type, args);
        }

        internal static TReturn Create<TReturn, TBase>(params object[] args) 
            where TBase : class, TReturn
            where TReturn : class
        {
            return CoreInstanceProvider.Create(typeof(TBase), args) as TReturn;
        }

        internal static object CreateInstance(Type instanceType, params object[] args)
        {
            return Activator.CreateInstance(instanceType, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null, args, null);
        }

        internal static string TypeArrayToString(Type[] types)
        {
            string typeString = string.Empty;
            foreach (Type t in types)
            {
                if (string.IsNullOrEmpty(typeString))
                    typeString = t.ToString();
                else
                    typeString += "," + t.ToString();
            }
            return typeString;
        }
    }
}
