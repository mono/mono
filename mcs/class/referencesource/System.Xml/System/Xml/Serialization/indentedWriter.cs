//------------------------------------------------------------------------------
// <copyright file="IndentedWriter.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">Microsoft</owner>                                                                
//------------------------------------------------------------------------------

namespace System.Xml.Serialization {

    using System.IO;
    
    /// <include file='doc\IndentedWriter.uex' path='docs/doc[@for="IndentedWriter"]/*' />
    /// <devdoc>
    ///     This class will write to a stream and manage indentation.
    /// </devdoc>
    internal class IndentedWriter {
        TextWriter writer;
        bool needIndent;
        int indentLevel;
        bool compact;
        
        internal IndentedWriter(TextWriter writer, bool compact) {
            this.writer = writer;
            this.compact = compact;
        }

        internal int Indent {
            get {
                return indentLevel;
            }
            set {
                indentLevel = value;
            }
        }
        
        internal void Write(string s) {
            if (needIndent) WriteIndent();
            writer.Write(s);
        }
        
        internal void Write(char c) {
            if (needIndent) WriteIndent();
            writer.Write(c);
        }
        
        internal void WriteLine(string s) {
            if (needIndent) WriteIndent();
            writer.WriteLine(s);
            needIndent = true;
        }
        
        internal void WriteLine() {
            writer.WriteLine();
            needIndent = true;
        }

        internal void WriteIndent() {
            needIndent = false;
            if (!compact) {
                for (int i = 0; i < indentLevel; i++) {
                    writer.Write("    ");
                }
            }
        }
    }
}
