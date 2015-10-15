//------------------------------------------------------------------------------
// <copyright file="CodeAttachEventStatement.cs" company="Microsoft">
// 
// <OWNER>Microsoft</OWNER>
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.CodeDom {

    using System.Diagnostics;
    using System;
    using Microsoft.Win32;
    using System.Collections;
    using System.Runtime.InteropServices;

    /// <devdoc>
    ///    <para>
    ///       Represents a event attach statement.
    ///    </para>
    /// </devdoc>
    [
        ClassInterface(ClassInterfaceType.AutoDispatch),
        ComVisible(true),
        Serializable,
    ]
    public class CodeAttachEventStatement : CodeStatement {
        private CodeEventReferenceExpression eventRef;
        private CodeExpression listener;

        /// <devdoc>
        ///    <para>
        ///       Initializes a new instance of <see cref='System.CodeDom.CodeAttachEventStatement'/>.
        ///    </para>
        /// </devdoc>
        public CodeAttachEventStatement() {
        }

        /// <devdoc>
        ///    <para>
        ///       Initializes a new instance of the <see cref='System.CodeDom.CodeAttachEventStatement'/> class using the specified arguments.
        ///    </para>
        /// </devdoc>
        public CodeAttachEventStatement(CodeEventReferenceExpression eventRef, CodeExpression listener) {
            this.eventRef = eventRef;
            this.listener = listener;
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public CodeAttachEventStatement(CodeExpression targetObject, string eventName, CodeExpression listener) {
            this.eventRef = new CodeEventReferenceExpression(targetObject, eventName);
            this.listener = listener;
        }

        /// <devdoc>
        ///    <para>
        ///       The event to attach a listener to.
        ///    </para>
        /// </devdoc>
        public CodeEventReferenceExpression Event {
            get {
                if (eventRef == null) {
                    return new CodeEventReferenceExpression();
                }
                return eventRef;
            }
            set {
                eventRef = value;
            }
        }

        /// <devdoc>
        ///    <para>
        ///       The new listener.
        ///    </para>
        /// </devdoc>
        public CodeExpression Listener {
            get {
                return listener;
            }
            set {
                listener = value;
            }
        }
    }
}
