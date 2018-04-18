//------------------------------------------------------------------------------
// <copyright file="CodeAttributeArgument.cs" company="Microsoft">
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
    ///       Represents an argument for use in a custom attribute declaration.
    ///    </para>
    /// </devdoc>
    [
        ClassInterface(ClassInterfaceType.AutoDispatch),
        ComVisible(true),
        Serializable,
    ]
    public class CodeAttributeArgument {
        private string name;
        private CodeExpression value;

        /// <devdoc>
        ///    <para>
        ///       Initializes a new instance of <see cref='System.CodeDom.CodeAttributeArgument'/>.
        ///    </para>
        /// </devdoc>
        public CodeAttributeArgument() {
        }

        /// <devdoc>
        ///    <para>
        ///       Initializes a new instance of <see cref='System.CodeDom.CodeAttributeArgument'/> using the specified value.
        ///    </para>
        /// </devdoc>
        public CodeAttributeArgument(CodeExpression value) {
            Value = value;
        }

        /// <devdoc>
        ///    <para>
        ///       Initializes a new instance of <see cref='System.CodeDom.CodeAttributeArgument'/> using the specified name and
        ///       value.
        ///    </para>
        /// </devdoc>
        public CodeAttributeArgument(string name, CodeExpression value) {
            Name = name;
            Value = value;
        }

        /// <devdoc>
        ///    <para>
        ///       The name of the attribute.
        ///    </para>
        /// </devdoc>
        public string Name {
            get {
                return (name == null) ? string.Empty : name;
            }
            set {
                name = value;
            }
        }

        /// <devdoc>
        ///    <para>
        ///       The argument for the attribute.
        ///    </para>
        /// </devdoc>
        public CodeExpression Value {
            get {
                return value;
            }
            set {
                this.value = value;
            }
        }
    }
}
