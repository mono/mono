//------------------------------------------------------------------------------
// <copyright file="InstanceDescriptor.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.ComponentModel.Design.Serialization {

    using System;
    using System.Collections;
    using System.Diagnostics;
    using System.Reflection;
    using System.Security.Permissions;

    /// <devdoc>
    ///     EventArgs for the ResolveNameEventHandler.  This event is used
    ///     by the serialization process to match a name to an object
    ///     instance.
    /// </devdoc>
    [HostProtection(SharedState = true)]
    [System.Security.Permissions.PermissionSetAttribute(System.Security.Permissions.SecurityAction.LinkDemand, Name = "FullTrust")]
    public sealed class InstanceDescriptor {
        private MemberInfo  member;
        private ICollection arguments;
        private bool isComplete;
        
        /// <devdoc>
        ///     Creates a new InstanceDescriptor.
        /// </devdoc>
        public InstanceDescriptor(MemberInfo member, ICollection arguments) : this(member, arguments, true) {
        }
        
        /// <devdoc>
        ///     Creates a new InstanceDescriptor.
        /// </devdoc>
        public InstanceDescriptor(MemberInfo member, ICollection arguments, bool isComplete) {
            this.member = member;
            this.isComplete = isComplete;
            
            if (arguments == null) {
                this.arguments = new object[0];
            }
            else {
                object[] args = new object[arguments.Count];
                arguments.CopyTo(args, 0);
                this.arguments = args;
            }
            
            if (member is FieldInfo) {
                FieldInfo fi = (FieldInfo)member;
                if (!fi.IsStatic) {
                    throw new ArgumentException(SR.GetString(SR.InstanceDescriptorMustBeStatic));
                }
                if (this.arguments.Count != 0) {
                    throw new ArgumentException(SR.GetString(SR.InstanceDescriptorLengthMismatch));
                }
            }
            else if (member is ConstructorInfo) {
                ConstructorInfo ci = (ConstructorInfo)member;
                if (ci.IsStatic) {
                    throw new ArgumentException(SR.GetString(SR.InstanceDescriptorCannotBeStatic));
                }
                if (this.arguments.Count != ci.GetParameters().Length) {
                    throw new ArgumentException(SR.GetString(SR.InstanceDescriptorLengthMismatch));
                }
            }
            else if (member is MethodInfo) {
                MethodInfo mi = (MethodInfo)member;
                if (!mi.IsStatic) {
                    throw new ArgumentException(SR.GetString(SR.InstanceDescriptorMustBeStatic));
                }
                if (this.arguments.Count != mi.GetParameters().Length) {
                    throw new ArgumentException(SR.GetString(SR.InstanceDescriptorLengthMismatch));
                }
            }
            else if (member is PropertyInfo) {
                PropertyInfo pi = (PropertyInfo)member;
                if (!pi.CanRead) {
                    throw new ArgumentException(SR.GetString(SR.InstanceDescriptorMustBeReadable));
                }
                MethodInfo mi = pi.GetGetMethod();
                if (mi != null && !mi.IsStatic) {
                    throw new ArgumentException(SR.GetString(SR.InstanceDescriptorMustBeStatic));
                }
            }
        }
    
        /// <devdoc>
        ///     The collection of arguments that should be passed to
        ///     MemberInfo in order to create an instance.
        /// </devdoc>
        public ICollection Arguments {
            get {
                return arguments;
            }
        }
        
        /// <devdoc>
        ///     Determines if the contents of this instance descriptor completely identify the instance.
        ///     This will normally be the case, but some objects may be too complex for a single method
        ///     or constructor to represent.  IsComplete can be used to identify these objects and take
        ///     additional steps to further describe their state.
        /// </devdoc>
        public bool IsComplete {
            get {
                return isComplete;
            }
        }
        
        /// <devdoc>
        ///     The MemberInfo object that was passed into the constructor
        ///     of this InstanceDescriptor.
        /// </devdoc>
        public MemberInfo MemberInfo {
            get {
                return member;
            }
        }
        
        /// <devdoc>
        ///     Invokes this instance descriptor, returning the object
        ///     the descriptor describes.
        /// </devdoc>
        public object Invoke() {
            object[] translatedArguments = new object[arguments.Count];
            arguments.CopyTo(translatedArguments, 0);
            
            // Instance descriptors can contain other instance
            // descriptors.  Translate them if necessary.
            //
            for(int i = 0; i < translatedArguments.Length; i++) {
                if (translatedArguments[i] is InstanceDescriptor) {
                    translatedArguments[i] = ((InstanceDescriptor)translatedArguments[i]).Invoke();
                }
            }
            
            if (member is ConstructorInfo) {
                return ((ConstructorInfo)member).Invoke(translatedArguments);
            }
            else if (member is MethodInfo) {
                return ((MethodInfo)member).Invoke(null, translatedArguments);
            }
            else if (member is PropertyInfo) {
                return ((PropertyInfo)member).GetValue(null, translatedArguments);
            }
            else if (member is FieldInfo) {
                return ((FieldInfo)member).GetValue(null);
            }
            else {
                Debug.Fail("Unrecognized reflection type in instance descriptor: " + member.GetType().Name);
            }
            
            return null;
        }
    }
}

