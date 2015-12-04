//------------------------------------------------------------------------------
// <copyright file="CodeAssignStatement.cs" company="Microsoft">
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
    ///       Represents a simple assignment statement.
    ///    </para>
    /// </devdoc>
    [
        ClassInterface(ClassInterfaceType.AutoDispatch),
        ComVisible(true),
        Serializable,
    ]
    public class CodeAssignStatement : CodeStatement {
        private CodeExpression left;
        private CodeExpression right;

        /// <devdoc>
        ///    <para>
        ///       Initializes a new instance of <see cref='System.CodeDom.CodeAssignStatement'/>.
        ///    </para>
        /// </devdoc>
        public CodeAssignStatement() {
        }

        /// <devdoc>
        ///    <para>
        ///       Initializes a new instance of <see cref='System.CodeDom.CodeAssignStatement'/> that represents the
        ///       specified assignment values.
        ///    </para>
        /// </devdoc>
        public CodeAssignStatement(CodeExpression left, CodeExpression right) {
            Left = left;
            Right = right;
        }

        /// <devdoc>
        ///    <para>
        ///       Gets or sets
        ///       the variable to be assigned to.
        ///    </para>
        /// </devdoc>
        public CodeExpression Left {
            get {
                return left;
            }
            set {
                left = value;
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Gets or sets
        ///       the value to assign.
        ///    </para>
        /// </devdoc>
        public CodeExpression Right {
            get {
                return right;
            }
            set {
                right = value;
            }
        }
    }
}
