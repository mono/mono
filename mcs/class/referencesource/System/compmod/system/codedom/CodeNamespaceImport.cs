//------------------------------------------------------------------------------
// <copyright file="CodeNamespaceImport.cs" company="Microsoft">
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
    ///       Represents a namespace import into the current namespace.
    ///    </para>
    /// </devdoc>
    [
        ClassInterface(ClassInterfaceType.AutoDispatch),
        ComVisible(true),
        Serializable,
    ]
    public class CodeNamespaceImport : CodeObject {
        private string nameSpace;
        private CodeLinePragma linePragma;

        /// <devdoc>
        ///    <para>
        ///       Initializes a new instance of <see cref='System.CodeDom.CodeNamespaceImport'/>.
        ///    </para>
        /// </devdoc>
        public CodeNamespaceImport() {
        }

        /// <devdoc>
        ///    <para>
        ///       Initializes a new instance of <see cref='System.CodeDom.CodeNamespaceImport'/> using the specified namespace
        ///       to import.
        ///    </para>
        /// </devdoc>
        public CodeNamespaceImport(string nameSpace) {
            Namespace = nameSpace;
        }

        /// <devdoc>
        ///    <para>
        ///       The line the statement occurs on.
        ///    </para>
        /// </devdoc>
        public CodeLinePragma LinePragma {
            get {
                return linePragma;
            }
            set {
                linePragma = value;
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Gets or sets
        ///       the namespace to import.
        ///    </para>
        /// </devdoc>
        public string Namespace {
            get {
                return (nameSpace == null) ? string.Empty : nameSpace;
            }
            set {
                nameSpace = value;
            }
        }
    }
}
