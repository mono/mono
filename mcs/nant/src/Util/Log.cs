// NAnt - A .NET build tool
// Copyright (C) 2001 Gerry Shaw
//
// This program is free software; you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
//
// Gerry Shaw (gerry_shaw@yahoo.com)

namespace SourceForge.NAnt {

    using System;
    using System.Collections;
    using System.IO;
    using System.Text;

    public class TextWriterCollection : ArrayList {
    }

    public class Log {

        static bool _autoFlush  = false;
        static int _indentLevel = 0;
        static int _indentSize  = 4;

        static TextWriterCollection _listeners;

        protected Log() {
        }

        ~Log() {
            // make sure we release any open file handles
            Close();
        }

        public static bool AutoFlush {
            get { return _autoFlush; }
            set { _autoFlush = value; }
        }

        public static int IndentLevel {
            get { return _indentLevel; }
            set { _indentLevel = value; }
        }

        public static int IndentSize {
            get { return _indentSize; }
            set { _indentSize = value; }
        }

        public static TextWriterCollection Listeners {
            get {
                if (_listeners == null) {
                    _listeners = new TextWriterCollection();
                    _listeners.Add(Console.Out);
                }
                return _listeners;
            }
        }

        public static void Close() {
            foreach (TextWriter writer in Listeners) {
                // never close the Console.Out writer
                if (writer != Console.Out) {
                    writer.Close();
                }
            }
        }

        public static void Flush() {
            foreach (TextWriter writer in Listeners) {
                writer.Flush();
            }
        }

        public static void Indent() {
            IndentLevel++;
        }

        public static void Unindent() {
            if (IndentLevel <= 0) {
                throw new InvalidOperationException("IndentLevel must be greater than zero before calling Unindent()");
            }
            IndentLevel--;
        }

        /// <summary>
        /// Flag to indicate next string will start on a new line so that it can be indented.
        /// </summary>
        private static bool _newline = true;

        private static void PreprocessValue(ref string value) {
            // if we are starting a new line then first indent the string
            if (_newline) {
                if (IndentLevel > 0) {
                    StringBuilder sb = new StringBuilder(value);
                    sb.Insert(0, " ", IndentLevel * IndentSize);
                    value = sb.ToString();
                }
                _newline = false;
            }
        }

        public static void Write(string value) {
            PreprocessValue(ref value);
            foreach (TextWriter writer in Listeners) {
                writer.Write(value);
            }

            if (AutoFlush) {
                foreach (TextWriter writer in Listeners) {
                    writer.Flush();
                }
            }
        }

        public static void WriteLine() {
            WriteLine(String.Empty);
        }

        public static void WriteLine(string value) {
            PreprocessValue(ref value);
            foreach (TextWriter writer in Listeners) {
                writer.WriteLine(value);
            }

            if (AutoFlush) {
                foreach (TextWriter writer in Listeners) {
                    writer.Flush();
                }
            }

            // make sure we indent the next line
            _newline = true;
        }

        public static void Write(string format, params object[] arg) {
            Write(String.Format(format, arg));
        }

        public static void WriteLine(string format, params object[] arg) {
            WriteLine(String.Format(format, arg));
        }
    }
}