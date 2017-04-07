//------------------------------------------------------------------------------
// <copyright file="CodeLinePragma.cs" company="Microsoft">
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
    ///       Represents line number information for an external file.
    ///    </para>
    /// </devdoc>
    [
        ClassInterface(ClassInterfaceType.AutoDispatch),
        ComVisible(true),
        Serializable,
    ]
    public class CodeLinePragma {
        private string fileName;
        private int lineNumber;

        public CodeLinePragma() {
        }

        /// <devdoc>
        ///    <para>
        ///       Initializes a new instance of <see cref='System.CodeDom.CodeLinePragma'/>.
        ///    </para>
        /// </devdoc>
        public CodeLinePragma(string fileName, int lineNumber) {
            FileName = fileName;
            LineNumber = lineNumber;
        }

        /// <devdoc>
        ///    <para>
        ///       Gets or sets
        ///       the filename of
        ///       the associated file.
        ///    </para>
        /// </devdoc>
        public string FileName {
            get {
                return (fileName == null) ? string.Empty : fileName;
            }
            set {
                fileName = value;
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Gets or sets the line number of the file for
        ///       the current pragma.
        ///    </para>
        /// </devdoc>
        public int LineNumber {
            get {
                return lineNumber;
            }
            set {
                lineNumber = value;
            }
        }
    }
}
