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
	/// Outputter implementation for text output method.
	/// </summary>
	public class TextOutputter : Outputter {
		private TextWriter _writer;
		//Current output depth
		private int _depth;		
		//Ignore nested text nodes
		private bool _ignoreNestedText;		
		
		public TextOutputter (TextWriter w, bool ignoreNestedText) {
			_writer = w;
			_ignoreNestedText = ignoreNestedText;
		}

		public override void WriteStartDocument () {}
		
		public override void WriteEndDocument () {}

		public override void WriteStartElement (string prefix, string localName, string nsURI) {
			if (_ignoreNestedText) _depth++;
		}

		public override void WriteEndElement() {
			if (_ignoreNestedText) _depth--;
		}

		public override void WriteAttributeString (string prefix, string localName, string nsURI, string value) {}
		
		public override void WriteNamespaceDecl (string prefix, string nsUri) {}

		public override void WriteComment (string text) {}

		public override void WriteProcessingInstruction (string name, string text) {}

		public override void WriteString (string text) {
			WriteImpl (text);
		}

		public override void WriteRaw (string data) {
			WriteImpl (data);
		}

		private void WriteImpl(string text) {
			if (!_ignoreNestedText || _depth==0) _writer.Write (text);
		}

		public override void Done () {
			_writer.Flush ();
		}

		public override WriteState WriteState { get { return WriteState.Start; } }
	}
}
