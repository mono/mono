//
// System.Xml.XmlParserInput
//
// Author:
//	Atsushi Enomoto (ginga@kit.hi-ho.ne.jp)
//
//	(C)2003 Atsushi Enomoto
//
using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Globalization;

namespace Mono.Xml.Native
{
	public class XmlParserInput
	{
		#region ctor
		public XmlParserInput (TextReader reader, string baseURI)
			: this (reader, baseURI, 1, 1)
		{
		}

		public XmlParserInput (TextReader reader, string baseURI, int line, int column)
		{
			this.reader = reader;

			StreamReader sr = reader as StreamReader;
			if (sr != null)
				can_seek = sr.BaseStream.CanSeek;

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
					String.Format (
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
			this.peBuffer.Insert (0, ' ' + value + ' ');
		}

		public int PeekChar ()
		{
			if (peBuffer.Length > 0)
				return peBuffer [0];

			if (can_seek)
				return reader.Peek ();

			if (has_peek)
				return peek_char;

			peek_char = reader.Read ();
			has_peek = true;
			return peek_char;
		}

		public int ReadChar ()
		{
			int ch;

			if (peBuffer.Length > 0) {
				ch = peBuffer [0];
				peBuffer.Remove (0, 1);
				// I decided not to add character to currentTag with respect to PERef value
				return ch;
			}

			if (has_peek) {
				ch = peek_char;
				has_peek = false;
			} else {
				ch = reader.Read ();
			}

			if (ch == '\n') {
				line++;
				column = 1;
			} else {
				column++;
			}
			currentMarkup.Append ((char) ch);
			return ch;
		}
		#endregion

		#region Public Properties
		public string BaseURI {
			get { return baseURI; }
		}

		public StringBuilder CurrentMarkup {
			get { return this.currentMarkup; }
		}

		public int LineNumber {
			get { return line; }
		}

		public int LinePosition {
			get { return column; }
		}
		public string Name 
		{
			get {
				return currentMarkup.ToString (parsedNameStart, parsedNameEnd - parsedNameStart);
			}
		}

		public string Value {
			get {
				return currentMarkup.ToString (parsedValueStart, parsedValueEnd - parsedValueStart);
			}
		}
		#endregion

		#region Privates
		private void ReadNameOrNmToken(bool isNameToken)
		{
			parsedNameStart = currentMarkup.Length;
			if(isNameToken) {
				if (!XmlChar.IsNameChar (PeekChar ()))
					throw ReaderError ("a name did not start with a legal character " + PeekChar ());
			}
			else {
				if (!XmlChar.IsFirstNameChar (PeekChar ()))
					throw ReaderError ("a name did not start with a valid character " + PeekChar () + "(" + (char) PeekChar () + ")");
			}

			ReadChar ();

			while (XmlChar.IsNameChar (PeekChar ())) {
				ReadChar ();
			}

			parsedNameEnd = currentMarkup.Length;
		}

		// Privates
		TextReader reader;
		bool can_seek;
		bool has_peek;
		int peek_char;
		int line;
		int column;
		StringBuilder currentMarkup = new StringBuilder ();
		int parsedNameStart;
		int parsedNameEnd;
		int parsedValueStart;
		int parsedValueEnd;
		StringBuilder peBuffer = new StringBuilder ();
		string baseURI;

		private int ParseCharReference (string name)
		{
			int ret = -1;
			if (name.Length > 0 && name [0] == '#') {
				if (name [1] == 'x')
					ret = int.Parse (name.Substring (2, name.Length - 2), NumberStyles.None & NumberStyles.AllowHexSpecifier);
				else
					ret = int.Parse (name.Substring (1, name.Length - 1));
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
			return new XmlException (message, line, column);
		}
		#endregion
	}
}
