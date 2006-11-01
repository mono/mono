//
// System.Xml.XmlParserInput
//
// Author:
//	Atsushi Enomoto (ginga@kit.hi-ho.ne.jp)
//
//	(C)2003 Atsushi Enomoto
//

//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
using System;
using System.Collections;
using System.IO;
using System.Text;
using System.Xml;
using System.Globalization;
using Mono.Xml;

namespace System.Xml
{
	internal class XmlParserInput
	{
		class XmlParserInputSource
		{
			public readonly string BaseURI;
			readonly TextReader reader;
			public int state;
			public bool isPE;
			int line;
			int column;

			public XmlParserInputSource (
				TextReader reader, string baseUri, bool pe,
				int line, int column)
			{
				BaseURI = baseUri;
				this.reader = reader;
				this.isPE = pe;
				this.line = line;
				this.column = column;
			}

			public int LineNumber {
				get { return line; }
			}

			public int LinePosition {
				get { return column; }
			}

			public void Close ()
			{
				reader.Close ();
			}

			public int Read ()
			{
				if (state == 2)
					return -1;
				if (isPE && state == 0) {
					state = 1;
					return ' ';
				}
				int v = reader.Read ();

				if (v == '\n') {
					line++;
					column = 1;
				} else if (v >= 0) {
					column++;
				}

				if (v < 0 && state == 1) {
					state = 2;
					return ' ';
				}
				return v;
			}
		}

		#region ctor
		public XmlParserInput (TextReader reader, string baseURI)
			: this (reader, baseURI, 1, 0)
		{
		}

		public XmlParserInput (TextReader reader, string baseURI, int line, int column)
		{
			this.source = new XmlParserInputSource (reader, baseURI, false, line, column);
		}
		#endregion

		#region Public Methods
		// Read the next character and compare it against the
		// specified character.
		public void Close ()
		{
			while (sourceStack.Count > 0)
				((XmlParserInputSource) sourceStack.Pop ()).Close ();
			source.Close ();
		}

		public void Expect (int expected)
		{
			int ch = ReadChar ();

			if (ch != expected) {
				throw ReaderError (
					String.Format (CultureInfo.InvariantCulture, 
						"expected '{0}' ({1:X}) but found '{2}' ({3:X})",
						(char)expected,
						expected,
						(char)ch,
						ch));
			}
		}

		public void Expect (string expected)
		{
			int len = expected.Length;
			for(int i=0; i< len; i++)
				Expect (expected[i]);
		}

		public void PushPEBuffer (DTDParameterEntityDeclaration pe)
		{
			sourceStack.Push (source);
			source = new XmlParserInputSource (
				new StringReader (pe.ReplacementText),
				pe.ActualUri, true, 1, 0);
		}

		private int ReadSourceChar ()
		{
			int v = source.Read ();
			while (v < 0 && sourceStack.Count > 0) {
				source = sourceStack.Pop () as XmlParserInputSource;
				v = source.Read ();
			}
			return v;
		}

		public int PeekChar ()
		{
			if (has_peek)
				return peek_char;

			peek_char = ReadSourceChar ();
			if (peek_char >= 0xD800 && peek_char <= 0xDBFF) {
				peek_char = 0x10000+((peek_char-0xD800)<<10);
				int i = ReadSourceChar ();
				if (i >= 0xDC00 && i <= 0xDFFF)
					peek_char += (i-0xDC00);
			}
			has_peek = true;
			return peek_char;
		}

		public int ReadChar ()
		{
			int ch;

			ch = PeekChar ();
			has_peek = false;

			return ch;
		}

		#endregion

		#region Public Properties
		public string BaseURI {
			get { return source.BaseURI; }
		}

		public bool HasPEBuffer {
			get { return sourceStack.Count > 0; }
		}
		
		public int LineNumber {
			get { return source.LineNumber; }
		}

		public int LinePosition {
			get { return source.LinePosition; }
		}

		public bool AllowTextDecl {
			get { return allowTextDecl; }
			set { allowTextDecl = value; }
		}

		#endregion

		#region Privates
		Stack sourceStack = new Stack ();
		XmlParserInputSource source;
		bool has_peek;
		int peek_char;
		bool allowTextDecl = true;

		private XmlException ReaderError (string message)
		{
			return new XmlException (message, null, LineNumber, LinePosition);
		}
		#endregion
	}
}
