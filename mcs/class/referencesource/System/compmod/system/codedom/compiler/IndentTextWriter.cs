//------------------------------------------------------------------------------
// <copyright file="IndentTextWriter.cs" company="Microsoft">
// 
// <OWNER>[....]</OWNER>
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.CodeDom.Compiler {

    using System.Diagnostics;
    using System;
    using System.IO;
    using System.Text;
    using System.Security.Permissions;
    using System.Globalization;

    /// <devdoc>
    ///    <para>Provides a text writer that can indent new lines by a tabString token.</para>
    /// </devdoc>
    public class IndentedTextWriter : TextWriter {
        private TextWriter writer;
        private int indentLevel;
        private bool tabsPending;
        private string tabString;

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public const string DefaultTabString = "    ";

        /// <devdoc>
        ///    <para>
        ///       Initializes a new instance of <see cref='System.CodeDom.Compiler.IndentedTextWriter'/> using the specified
        ///       text writer and default tab string.
        ///    </para>
        /// </devdoc>
        public IndentedTextWriter(TextWriter writer) : this(writer, DefaultTabString) {
        }

        /// <devdoc>
        ///    <para>
        ///       Initializes a new instance of <see cref='System.CodeDom.Compiler.IndentedTextWriter'/> using the specified
        ///       text writer and tab string.
        ///    </para>
        /// </devdoc>
        public IndentedTextWriter(TextWriter writer, string tabString): base(CultureInfo.InvariantCulture) {
            this.writer = writer;
            this.tabString = tabString;
            indentLevel = 0;
            tabsPending = false;
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public override Encoding Encoding {
            get {
                return writer.Encoding;
            }
        }
                                                
        /// <devdoc>
        ///    <para>
        ///       Gets or sets the new line character to use.
        ///    </para>
        /// </devdoc>
        public override string NewLine {
            get {
                return writer.NewLine;
            }

            set {
                writer.NewLine = value;
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Gets or sets the number of spaces to indent.
        ///    </para>
        /// </devdoc>
        public int Indent {
            get {
                return indentLevel;
            }
            set {
                Debug.Assert(value >= 0, "Bogus Indent... probably caused by mismatched Indent++ and Indent--");
                if (value < 0) {
                    value = 0;
                }
                indentLevel = value;
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Gets or sets the TextWriter to use.
        ///    </para>
        /// </devdoc>
        public TextWriter InnerWriter {
            get {
                return writer;
            }
        }

        internal string TabString {
            get { return tabString; }
        }

        /// <devdoc>
        ///    <para>
        ///       Closes the document being written to.
        ///    </para>
        /// </devdoc>
        public override void Close() {
            writer.Close();
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public override void Flush() {
            writer.Flush();
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        protected virtual void OutputTabs() {
            if (tabsPending) {
                for (int i=0; i < indentLevel; i++) {
                    writer.Write(tabString);
                }
                tabsPending = false;
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Writes a string
        ///       to the text stream.
        ///    </para>
        /// </devdoc>
        public override void Write(string s) {
            OutputTabs();
            writer.Write(s);
        }

        /// <devdoc>
        ///    <para>
        ///       Writes the text representation of a Boolean value to the text stream.
        ///    </para>
        /// </devdoc>
        public override void Write(bool value) {
            OutputTabs();
            writer.Write(value);
        }

        /// <devdoc>
        ///    <para>
        ///       Writes a character to the text stream.
        ///    </para>
        /// </devdoc>
        public override void Write(char value) {
            OutputTabs();
            writer.Write(value);
        }

        /// <devdoc>
        ///    <para>
        ///       Writes a
        ///       character array to the text stream.
        ///    </para>
        /// </devdoc>
        public override void Write(char[] buffer) {
            OutputTabs();
            writer.Write(buffer);
        }

        /// <devdoc>
        ///    <para>
        ///       Writes a subarray
        ///       of characters to the text stream.
        ///    </para>
        /// </devdoc>
        public override void Write(char[] buffer, int index, int count) {
            OutputTabs();
            writer.Write(buffer, index, count);
        }

        /// <devdoc>
        ///    <para>
        ///       Writes the text representation of a Double to the text stream.
        ///    </para>
        /// </devdoc>
        public override void Write(double value) {
            OutputTabs();
            writer.Write(value);
        }

        /// <devdoc>
        ///    <para>
        ///       Writes the text representation of
        ///       a Single to the text
        ///       stream.
        ///    </para>
        /// </devdoc>
        public override void Write(float value) {
            OutputTabs();
            writer.Write(value);
        }

        /// <devdoc>
        ///    <para>
        ///       Writes the text representation of an integer to the text stream.
        ///    </para>
        /// </devdoc>
        public override void Write(int value) {
            OutputTabs();
            writer.Write(value);
        }

        /// <devdoc>
        ///    <para>
        ///       Writes the text representation of an 8-byte integer to the text stream.
        ///    </para>
        /// </devdoc>
        public override void Write(long value) {
            OutputTabs();
            writer.Write(value);
        }

        /// <devdoc>
        ///    <para>
        ///       Writes the text representation of an object
        ///       to the text stream.
        ///    </para>
        /// </devdoc>
        public override void Write(object value) {
            OutputTabs();
            writer.Write(value);
        }

        /// <devdoc>
        ///    <para>
        ///       Writes out a formatted string, using the same semantics as specified.
        ///    </para>
        /// </devdoc>
        public override void Write(string format, object arg0) {
            OutputTabs();
            writer.Write(format, arg0);
        }

        /// <devdoc>
        ///    <para>
        ///       Writes out a formatted string,
        ///       using the same semantics as specified.
        ///    </para>
        /// </devdoc>
        public override void Write(string format, object arg0, object arg1) {
            OutputTabs();
            writer.Write(format, arg0, arg1);
        }

        /// <devdoc>
        ///    <para>
        ///       Writes out a formatted string,
        ///       using the same semantics as specified.
        ///    </para>
        /// </devdoc>
        public override void Write(string format, params object[] arg) {
            OutputTabs();
            writer.Write(format, arg);
        }

        /// <devdoc>
        ///    <para>
        ///       Writes the specified
        ///       string to a line without tabs.
        ///    </para>
        /// </devdoc>
        public void WriteLineNoTabs(string s) {
            writer.WriteLine(s);
        }

        /// <devdoc>
        ///    <para>
        ///       Writes the specified string followed by
        ///       a line terminator to the text stream.
        ///    </para>
        /// </devdoc>
        public override void WriteLine(string s) {
            OutputTabs();
            writer.WriteLine(s);
            tabsPending = true;
        }

        /// <devdoc>
        ///    <para>
        ///       Writes a line terminator.
        ///    </para>
        /// </devdoc>
        public override void WriteLine() {
            OutputTabs();
            writer.WriteLine();
            tabsPending = true;
        }

        /// <devdoc>
        ///    <para>
        ///       Writes the text representation of a Boolean followed by a line terminator to
        ///       the text stream.
        ///    </para>
        /// </devdoc>
        public override void WriteLine(bool value) {
            OutputTabs();
            writer.WriteLine(value);
            tabsPending = true;
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public override void WriteLine(char value) {
            OutputTabs();
            writer.WriteLine(value);
            tabsPending = true;
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public override void WriteLine(char[] buffer) {
            OutputTabs();
            writer.WriteLine(buffer);
            tabsPending = true;
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public override void WriteLine(char[] buffer, int index, int count) {
            OutputTabs();
            writer.WriteLine(buffer, index, count);
            tabsPending = true;
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public override void WriteLine(double value) {
            OutputTabs();
            writer.WriteLine(value);
            tabsPending = true;
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public override void WriteLine(float value) {
            OutputTabs();
            writer.WriteLine(value);
            tabsPending = true;
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public override void WriteLine(int value) {
            OutputTabs();
            writer.WriteLine(value);
            tabsPending = true;
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public override void WriteLine(long value) {
            OutputTabs();
            writer.WriteLine(value);
            tabsPending = true;
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public override void WriteLine(object value) {
            OutputTabs();
            writer.WriteLine(value);
            tabsPending = true;
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public override void WriteLine(string format, object arg0) {
            OutputTabs();
            writer.WriteLine(format, arg0);
            tabsPending = true;
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public override void WriteLine(string format, object arg0, object arg1) {
            OutputTabs();
            writer.WriteLine(format, arg0, arg1);
            tabsPending = true;
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public override void WriteLine(string format, params object[] arg) {
            OutputTabs();
            writer.WriteLine(format, arg);
            tabsPending = true;
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        [CLSCompliant(false)]
        public override void WriteLine(UInt32 value) {
            OutputTabs();
            writer.WriteLine(value);
            tabsPending = true;
        }

        internal void InternalOutputTabs() {
            for (int i=0; i < indentLevel; i++) {
                writer.Write(tabString);               
            }
        }
    }

    internal class Indentation {
        private IndentedTextWriter writer;
        private int indent;
        private string s;

        internal Indentation(IndentedTextWriter writer, int indent) {
            this.writer = writer;
            this.indent = indent;
            s = null;
        }

        internal string IndentationString {
            get {
                if ( s == null) {
                    string tabString = writer.TabString;
                    StringBuilder sb = new StringBuilder(indent * tabString.Length);
                    for( int i = 0; i < indent; i++) {
                        sb.Append(tabString);
                    }
                    s = sb.ToString();
                }
                return s;
            }
        }        
    }
}
