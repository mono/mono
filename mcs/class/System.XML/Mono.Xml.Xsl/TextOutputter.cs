//
// TextOutputter.cs
//
// Authors:
//	Oleg Tkachenko (oleg@tkachenko.com)
//	
// (C) 2003 Oleg Tkachenko
//

using System;
using System.Xml;
using System.IO;

namespace Mono.Xml.Xsl {
	/// <summary>
	/// Outputter implementation for text output method
	/// </summary>
	public class TextOutputter : Outputter {
		private TextWriter _writer;
		
		public TextOutputter(TextWriter w) {
			_writer = w;
		}

		public override void WriteStartDocument() {}
		
		public override void WriteEndDocument() {}

		public override void WriteStartElement(string prefix, string localName, string nsURI) {}

		public override void WriteEndElement() {}

		public override void WriteAttributeString(string prefix, string localName, string nsURI, string value) {}

		public override void WriteStartAttribute(string prefix, string localName, string nsURI) {}

		public override void WriteEndAttribute() {}

		public override void WriteComment(string text) {}

		public override void WriteProcessingInstruction(string name, string text) {}

		public override void WriteString(string text) {
			_writer.Write(text);
		}

		public override void WriteRaw(string data) {
			_writer.Write(data);
		}

		public override void Done () {
			_writer.Flush ();
		}
	}
}
