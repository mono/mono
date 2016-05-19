//------------------------------------------------------------------------------
// <copyright file="CodeStatement.cs" company="Microsoft">
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
    using System.Runtime.Serialization;    

    /// <devdoc>
    ///    <para>
    ///       Represents a statement.
    ///    </para>
    /// </devdoc>
    [
        ClassInterface(ClassInterfaceType.AutoDispatch),
        ComVisible(true),
        Serializable,
    ]
    public class CodeStatement : CodeObject {
        private CodeLinePragma linePragma;
        
        // Optionally Serializable
        [OptionalField]
        private CodeDirectiveCollection startDirectives = null;
        [OptionalField]
        private CodeDirectiveCollection endDirectives = null;
        

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

        public CodeDirectiveCollection StartDirectives {
            get {
                if (startDirectives == null) {
                    startDirectives = new CodeDirectiveCollection();
                }
                return startDirectives;                
            }
        }

        public CodeDirectiveCollection EndDirectives {
            get {
                if (endDirectives == null) {
                    endDirectives = new CodeDirectiveCollection();
                }
                return endDirectives ;                
            }
        }        
    }
}
