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
using System.IO;
using System.Text;
using System.Xml;
using System.Globalization;

namespace System.Xml
{
	internal class XmlParserInput
	{
		#region ctor
		public XmlParserInput (TextReader reader, string baseURI)
			: this (reader, baseURI, 1, 1)
		{
		}

		public XmlParserInput (TextReader reader, string baseURI, int line, int column)
		{
			this.reader = reader;
			this.line = line;
			this.column = column;
			this.baseURI = baseURI;
		}
		#endregion

		#region Public Methods
		// Read the next character and compare it against the
		// specified character.
		public void Close ()
		{
			this.reader.Close ();
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

		public void InsertParameterEntityBuffer (string value)
		{
			this.peBuffer.Insert (peBufferIndex, ' ');
			this.peBuffer.Insert (peBufferIndex + 1, value);
			this.peBuffer.Insert (peBufferIndex + value.Length + 1, ' ');
			peStored = true;
		}

		public int PeekChar ()
		{
			if (peStored)
				return peBuffer [peBufferIndex];

			if (has_peek)
				return peek_char;

			peek_char = reader.Read ();
			if (peek_char >= 0xD800 && peek_char <= 0xDBFF) {
				int i = reader.Read ();
				if (i >= 0xDC00 && i <= 0xDFFF)
					peek_char += i;
			}
			has_peek = true;
			return peek_char;
		}

		public int ReadChar ()
		{
			int ch;

			if (peStored) {
				ch = peBuffer [peBufferIndex];
				peBufferIndex++;
				if (peBufferIndex == peBuffer.Length) {
					peStored = false;
					peBuffer.Length = 0;
					peBufferIndex = 0;
				}
				// I decided not to add character to currentTag with respect to PERef value
				return ch;
			}

			if (has_peek) {
				ch = peek_char;
				has_peek = false;
			} else {
				ch = reader.Read ();
				if (ch >= 0xD800 && ch <= 0xDBFF) {
					int i = reader.Read ();
					if (i > 0xDC00 && i <= 0xDFFF)
						ch += i;
				}
			}

			if (ch == '\n') {
				line++;
				column = 1;
			} else {
				column++;
			}
			return ch;
		}

		#endregion

		#region Public Properties
		public string BaseURI {
			get { return baseURI; }
		}

		public bool HasPEBuffer {
			get { return peStored; }
		}
		
		public int LineNumber {
			get { return line; }
		}

		public int LinePosition {
			get { return column; }
		}

		public bool InitialState {
			get { return initialState; }
			set { initialState = value; }
		}

		#endregion

		#region Privates
		TextReader reader;
		bool has_peek;
		int peek_char;
		int line;
		int column;
		StringBuilder peBuffer = new StringBuilder ();
		string baseURI;
		bool peStored = false;
		bool initialState = true;
		int peBufferIndex;

		private int ParseCharReference (string name)
		{
			int ret = -1;
			if (name.Length > 0 && name [0] == '#') {
				if (name [1] == 'x')
					ret = int.Parse (name.Substring (2, name.Length - 2), NumberStyles.None & NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture);
				else
					ret = int.Parse (name.Substring (1, name.Length - 1), CultureInfo.InvariantCulture);
			}
			return ret;
		}

		private int ParseKnownEntityReference (string name)
		{
			switch (name) {
			case "quot": return '"';
			case "lt": return '<';
			case "gt": return '>';
			case "amp": return '&';
			case "apos": return '\'';
			}
			return -1;
		}

		private XmlException ReaderError (string message)
		{
			return new XmlException (message, null, line, column);
		}
		#endregion
	}
}
