﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.296
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Mercury.Messaging.Properties {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class Strings {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Strings() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Mercury.Messaging.Properties.Strings", typeof(Strings).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to An expected acknowledgement was not received..
        /// </summary>
        internal static string Acknowledgement_Failed_Exception {
            get {
                return ResourceManager.GetString("Acknowledgement_Failed_Exception", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The specified type parameter &apos;{0}&apos; must be an interface..
        /// </summary>
        internal static string Interface_Type_Constraint {
            get {
                return ResourceManager.GetString("Interface_Type_Constraint", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Invalid or incomplete message was received..
        /// </summary>
        internal static string Invalid_Message_Exception {
            get {
                return ResourceManager.GetString("Invalid_Message_Exception", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Message delivery was not verifiably successful..
        /// </summary>
        internal static string Message_Delivery_Exception {
            get {
                return ResourceManager.GetString("Message_Delivery_Exception", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to A single, received message exceeded the allowable size limit..
        /// </summary>
        internal static string Message_Size_Overflow {
            get {
                return ResourceManager.GetString("Message_Size_Overflow", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The data packet size exceeds permitted lengths.  Message data may not exceed {0} bytes in size..
        /// </summary>
        internal static string Packet_Protocol_Size_Violation {
            get {
                return ResourceManager.GetString("Packet_Protocol_Size_Violation", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to A partial or non-terminating message was received on a remote runtime..
        /// </summary>
        internal static string Partial_Message_Exception {
            get {
                return ResourceManager.GetString("Partial_Message_Exception", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Message processing failed on the remote runtime..
        /// </summary>
        internal static string Remote_Process_Failure {
            get {
                return ResourceManager.GetString("Remote_Process_Failure", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The specified {0} type does not support a constructor with arguments: {1}..
        /// </summary>
        internal static string RTConstraintException_CompileTime_Constructors {
            get {
                return ResourceManager.GetString("RTConstraintException_CompileTime_Constructors", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to A constraint was violated on this runtime environment..
        /// </summary>
        internal static string RTConstraintException_General {
            get {
                return ResourceManager.GetString("RTConstraintException_General", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to A remote channel cannot send while waiting for a synchronous reply..
        /// </summary>
        internal static string RTConstraintException_Remote_Sync {
            get {
                return ResourceManager.GetString("RTConstraintException_Remote_Sync", resourceCulture);
            }
        }
    }
}