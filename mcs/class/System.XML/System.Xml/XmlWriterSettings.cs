//
// XmlWriterSettings.cs
//
// Author:
//   Atsushi Enomoto <atsushi@ximian.com>
//
// (C) 2004 Novell Inc.
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

#if NET_2_0

using System;
using System.IO;
using System.Text;
using System.Xml.Schema;

namespace System.Xml
{
	public sealed class XmlWriterSettings
	{
		private bool checkCharacters;
		private bool closeOutput;
		private ConformanceLevel conformance;
		private Encoding encoding;
		private bool indent;
		private string indentChars;
		private string newLineChars;
		private bool newLineOnAttributes;
		private NewLineHandling newLineHandling;
		private bool omitXmlDeclaration;
		private XmlOutputMethod outputMethod;

		public XmlWriterSettings ()
		{
			Reset ();
		}

		public XmlWriterSettings Clone ()
		{
			return (XmlWriterSettings) MemberwiseClone ();
		}

		public void Reset ()
		{
			checkCharacters = true;
			closeOutput = false; // ? not documented
			conformance = ConformanceLevel.Document;
			encoding = Encoding.UTF8;
			indent = false;
			indentChars = "  ";
			// LAMESPEC: MS.NET says it is "\r\n", but it is silly decision.
			newLineChars = Environment.NewLine;
			newLineOnAttributes = false;
			newLineHandling = NewLineHandling.None;
			omitXmlDeclaration = false;
			outputMethod = XmlOutputMethod.AutoDetect;
		}

		// It affects only on XmlTextWriter
		public bool CheckCharacters {
			get { return checkCharacters; }
			set { checkCharacters = value; }
		}

		// It affects only on XmlTextWriter
		public bool CloseOutput {
			get { return closeOutput; }
			set { closeOutput = value; }
		}

		// It affects only on XmlTextWriter????
		public ConformanceLevel ConformanceLevel {
			get { return conformance; }
			set { conformance = value; }
		}

		public Encoding Encoding {
			get { return encoding; }
			set { encoding = value; }
		}

		// It affects only on XmlTextWriter
		public bool Indent {
			get { return indent; }
			set { indent = value; }
		}

		// It affects only on XmlTextWriter
		public string IndentChars {
			get { return indentChars; }
			set { indentChars = value; }
		}

		// It affects only on XmlTextWriter
		public string NewLineChars {
			get { return newLineChars; }
			set {
				if (value == null)
					throw new ArgumentNullException ("value");
				newLineChars = value;
			}
		}

		// It affects only on XmlTextWriter
		public bool NewLineOnAttributes {
			get { return newLineOnAttributes; }
			set { newLineOnAttributes = value; }
		}

		// It affects only on XmlTextWriter
		public NewLineHandling NewLineHandling {
			get { return newLineHandling; }
			set { newLineHandling = value; }
		}

		// It affects only on XmlTextWriter
		public bool OmitXmlDeclaration {
			get { return omitXmlDeclaration; }
			set { omitXmlDeclaration = value; }
		}

		// does it affect only on XmlTextWriter?
		public XmlOutputMethod OutputMethod {
			get { return outputMethod; }
			//set { outputMethod = value; }
		}

#if MOONLIGHT || NET_4_0
		public
#else
		internal
#endif
		NamespaceHandling NamespaceHandling { get; set; }
	}
}

#endif
