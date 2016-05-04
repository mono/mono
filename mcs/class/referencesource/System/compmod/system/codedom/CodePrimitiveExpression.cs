//------------------------------------------------------------------------------
// <copyright file="CodePrimitiveExpression.cs" company="Microsoft">
// 
// <OWNER>[....]</OWNER>
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
    ///       Represents a primitive value.
    ///    </para>
    /// </devdoc>
    [
        ClassInterface(ClassInterfaceType.AutoDispatch),
        ComVisible(true),
        Serializable,
    ]
    public class CodePrimitiveExpression : CodeExpression {
        private object value;

        /// <devdoc>
        ///    <para>
        ///       Initializes a new instance of <see cref='System.CodeDom.CodePrimitiveExpression'/>.
        ///    </para>
        /// </devdoc>
        public CodePrimitiveExpression() {
        }

        /// <devdoc>
        ///    <para>
        ///       Initializes a new instance of <see cref='System.CodeDom.CodePrimitiveExpression'/> using the specified
        ///       object.
        ///    </para>
        /// </devdoc>
        public CodePrimitiveExpression(object value) {
            Value = value;
        }

        /// <devdoc>
        ///    <para>
        ///       Gets or sets the object to represent.
        ///    </para>
        /// </devdoc>
        public object Value {
            get {
                return value;
            }
            set {
                this.value = value;
            }
        }
    }
}
