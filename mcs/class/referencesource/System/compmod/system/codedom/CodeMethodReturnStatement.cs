//------------------------------------------------------------------------------
// <copyright file="CodeMethodReturnStatement.cs" company="Microsoft">
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
    ///       Represents a return statement.
    ///    </para>
    /// </devdoc>
    [
        ClassInterface(ClassInterfaceType.AutoDispatch),
        ComVisible(true),
        Serializable,
    ]
    public class CodeMethodReturnStatement : CodeStatement {
        private CodeExpression expression;

        /// <devdoc>
        ///    <para>
        ///       Initializes a new instance of <see cref='System.CodeDom.CodeMethodReturnStatement'/>.
        ///    </para>
        /// </devdoc>
        public CodeMethodReturnStatement() {
        }

        /// <devdoc>
        ///    <para>
        ///       Initializes a new instance of <see cref='System.CodeDom.CodeMethodReturnStatement'/> using the specified expression.
        ///    </para>
        /// </devdoc>
        public CodeMethodReturnStatement(CodeExpression expression) {
            Expression = expression;
        }

        /// <devdoc>
        ///    <para>
        ///       Gets or sets the expression that indicates the return statement.
        ///    </para>
        /// </devdoc>
        public CodeExpression Expression {
            get {
                return expression;
            }
            set {
                expression = value;
            }
        }
    }
}
