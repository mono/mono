//------------------------------------------------------------------------------
// <copyright file="CodeComment.cs" company="Microsoft">
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
    ///    <para> Represents a comment.</para>
    /// </devdoc>
    [
        ClassInterface(ClassInterfaceType.AutoDispatch),
        ComVisible(true),
        Serializable,
    ]
    public class CodeComment : CodeObject {
        private string text;
        private bool docComment = false;

        /// <devdoc>
        ///    <para>
        ///       Initializes a new instance of <see cref='System.CodeDom.CodeComment'/>.
        ///    </para>
        /// </devdoc>
        public CodeComment() {
        }

        /// <devdoc>
        ///    <para>
        ///       Initializes a new instance of <see cref='System.CodeDom.CodeComment'/> with the specified text as
        ///       contents.
        ///    </para>
        /// </devdoc>
        public CodeComment(string text) {
            Text = text;
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public CodeComment(string text, bool docComment) {
            Text = text;
            this.docComment = docComment;
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public bool DocComment {
            get {
                return docComment;
            }
            set {
                docComment = value;
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Gets or setes
        ///       the text of the comment.
        ///    </para>
        /// </devdoc>
        public string Text {
            get {
                return (text == null) ? string.Empty : text;
            }
            set {
                text = value;
            }
        }
    }
}
