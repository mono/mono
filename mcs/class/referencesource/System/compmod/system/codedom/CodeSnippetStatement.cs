//------------------------------------------------------------------------------
// <copyright file="CodeSnippetStatement.cs" company="Microsoft">
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
    ///       Represents a snippet statement.
    ///    </para>
    /// </devdoc>
    [
        ClassInterface(ClassInterfaceType.AutoDispatch),
        ComVisible(true),
        Serializable,
    ]
    public class CodeSnippetStatement : CodeStatement {
        private string value;

        /// <devdoc>
        ///    <para>
        ///       Initializes a new instance of <see cref='System.CodeDom.CodeSnippetStatement'/>.
        ///    </para>
        /// </devdoc>
        public CodeSnippetStatement() {
        }
        
        /// <devdoc>
        ///    <para>
        ///       Initializes a new instance of <see cref='System.CodeDom.CodeSnippetStatement'/> using the specified snippet
        ///       of code.
        ///    </para>
        /// </devdoc>
        public CodeSnippetStatement(string value) {
            Value = value;
        }

        /// <devdoc>
        ///    <para>
        ///       Gets or sets the snippet statement.
        ///    </para>
        /// </devdoc>
        public string Value {
            get {
                return (value == null) ? string.Empty : value;
            }
            set {
                this.value = value;
            }
        }
    }
}
