//------------------------------------------------------------------------------
// <copyright file="ContextStack.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.ComponentModel.Design.Serialization {

    using System;
    using System.Collections;
    using System.Security.Permissions;

    /// <devdoc>
    ///     A context stack is an object that can be used by serializers
    ///     to push various context objects.  Serialization is often
    ///     a deeply nested operation, involving many different 
    ///     serialization classes.  These classes often need additional
    ///     context information when performing serialization.  As
    ///     an example, an object with a property named "Enabled" may have
    ///     a data type of System.Boolean.  If a serializer is writing
    ///     this value to a data stream it may want to know what property
    ///     it is writing.  It won't have this information, however, because
    ///     it is only instructed to write the boolean value.  In this 
    ///     case the parent serializer may push a PropertyDescriptor
    ///     pointing to the "Enabled" property on the context stack.
    ///     What objects get pushed on this stack are up to the
    ///     individual serializer objects.
    /// </devdoc>
    [HostProtection(SharedState = true)]
    [System.Security.Permissions.PermissionSetAttribute(System.Security.Permissions.SecurityAction.LinkDemand, Name = "FullTrust")]
    public sealed class ContextStack {
        private ArrayList contextStack;
    
        /// <devdoc>
        ///     Retrieves the current object on the stack, or null
        ///     if no objects have been pushed.
        /// </devdoc>
        public object Current {
            get {
                if (contextStack != null && contextStack.Count > 0) {
                    return contextStack[contextStack.Count - 1];
                }
                return null;
            }
        }
        
        /// <devdoc>
        ///     Retrieves the object on the stack at the given
        ///     level, or null if no object exists at that level.
        /// </devdoc>
        public object this[int level] {
            get {
                if (level < 0) {
                    throw new ArgumentOutOfRangeException("level");
                }
                if (contextStack != null && level < contextStack.Count) {
                    return contextStack[contextStack.Count - 1 - level];
                }
                return null;
            }
        }
        
        /// <devdoc>
        ///     Retrieves the first object on the stack that 
        ///     inherits from or implements the given type, or
        ///     null if no object on the stack implements the type.
        /// </devdoc>
        public object this[Type type] {
            get {
                if (type == null) {
                    throw new ArgumentNullException("type");
                }
                
                if (contextStack != null) {
                    int level = contextStack.Count;
                    while(level > 0) {
                        object value = contextStack[--level];
                        if (type.IsInstanceOfType(value)) {
                            return value;
                        }
                    }
                }
                
                return null;
            }
        }

        /// <devdoc>
        ///     Appends an object to the end of the stack, rather than pushing it
        ///     onto the top of the stack.  This method allows a serializer to communicate
        ///     with other serializers by adding contextual data that does not have to
        ///     be popped in order.  There is no way to remove an object that was 
        ///     appended to the end of the stack without popping all other objects.
        /// </devdoc>
        public void Append(object context) {
            if (context == null) {
                throw new ArgumentNullException("context");
            }
            
            if (contextStack == null) {
                contextStack = new ArrayList();
            }
            contextStack.Insert(0, context);
        }
        
        /// <devdoc>
        ///     Pops the current object off of the stack, returning
        ///     its value.
        /// </devdoc>
        public object Pop() {
            object context = null;
            
            if (contextStack != null && contextStack.Count > 0) {
                int idx = contextStack.Count - 1;
                context = contextStack[idx];
                contextStack.RemoveAt(idx);
            }
            
            return context;
        }
        
        /// <devdoc>
        ///     Pushes the given object onto the stack.
        /// </devdoc>
        public void Push(object context) {
            if (context == null) {
                throw new ArgumentNullException("context");
            }
            
            if (contextStack == null) {
                contextStack = new ArrayList();
            }
            contextStack.Add(context);
        }
    }
}

