//
// XmlWriterSettings.cs
//
// Author:
//   Atsushi Enomoto <atsushi@ximian.com>
//
// (C) 2004 Novell Inc.
//

#if NET_2_0

using System;
using System.IO;
using System.Text;
using System.Xml.Schema;

namespace System.Xml
{
	public sealed class XmlWriterSettings : ICloneable
	{
		private bool checkCharacters;
		private bool closeOutput;
		private ConformanceLevel conformance;
		private bool encodeXmlBinary;
		private Encoding encoding;
		private bool indent;
		private string indentChars;
		private string newLineChars;
		private bool newLineOnAttributes;
		private bool normalizeNewLines;
		private bool omitXmlDeclaration;

		public XmlWriterSettings ()
		{
			Reset ();
		}

		private XmlWriterSettings (XmlWriterSettings org)
		{
			checkCharacters = org.checkCharacters;
			closeOutput = org.closeOutput;
			conformance = org.conformance;
			encodeXmlBinary = org.encodeXmlBinary;
			encoding = org.encoding;
			indent = org.indent;
			indentChars = org.indentChars;
			newLineChars = org.newLineChars;
			newLineOnAttributes = org.newLineOnAttributes;
			normalizeNewLines = org.normalizeNewLines;
			omitXmlDeclaration = org.omitXmlDeclaration;
		}

		public event ValidationEventHandler ValidationEventHandler;

		public XmlWriterSettings Clone ()
		{
			return new XmlWriterSettings (this);
		}

		object ICloneable.Clone ()
		{
			return this.Clone ();
		}

		public void Reset ()
		{
			checkCharacters = true;
			closeOutput = false; // ? not documented
			conformance = ConformanceLevel.Document;
			encodeXmlBinary = false;
			encoding = Encoding.UTF8;
			indent = false;
			indentChars = "  ";
			// LAMESPEC: MS.NET says it is "\r\n", but it is silly decision.
			newLineChars = Environment.NewLine;
			newLineOnAttributes = false;
			normalizeNewLines = true;
			omitXmlDeclaration = false;
		}

		public bool CheckCharacters {
			get { return checkCharacters; }
			set { checkCharacters = value; }
		}

		public bool CloseOutput {
			get { return closeOutput; }
			set { closeOutput = value; }
		}

		public ConformanceLevel ConformanceLevel {
			get { return conformance; }
			set { conformance = value; }
		}

		public bool EncodeXmlBinary {
			get { return encodeXmlBinary; }
		}

		public Encoding Encoding {
			get { return encoding; }
		}

		public bool Indent {
			get { return indent; }
		}

		public string IndentChars {
			get { return indentChars; }
		}

		public string NewLineChars {
			get { return newLineChars; }
		}

		public bool NewLineOnAttributes {
			get { return newLineOnAttributes; }
		}

		public bool NormalizeNewLines {
			get { return normalizeNewLines; }
		}

		public bool OmitXmlDeclaration {
			get { return omitXmlDeclaration; }
		}
	}
}

#endif
