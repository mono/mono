//
// TextOutputter.cs
//
// Authors:
//	Oleg Tkachenko (oleg@tkachenko.com)
//	
// (C) 2003 Oleg Tkachenko
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
using System.Xml;
using System.IO;

namespace Mono.Xml.Xsl {
	/// <summary>
	/// Outputter implementation for text output method.
	/// </summary>
	internal class TextOutputter : Outputter {
		private TextWriter _writer;
		//Current output depth
		private int _depth;		
		//Ignore nested text nodes
		private bool _ignoreNestedText;		
		
		public TextOutputter (TextWriter w, bool ignoreNestedText) {
			_writer = w;
			_ignoreNestedText = ignoreNestedText;
		}

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

		public override void WriteWhitespace (string text) {
			WriteImpl (text);
		}

		private void WriteImpl(string text) {
			if (!_ignoreNestedText || _depth==0) _writer.Write (text);
		}

		public override void Done () {
			_writer.Flush ();
		}

		public override bool CanProcessAttributes {
			get { return false; }
		}

		public override bool InsideCDataSection { get { return false; } set { } }
	}
}
