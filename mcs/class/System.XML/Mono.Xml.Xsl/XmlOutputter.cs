//
// XmlOutputter.cs
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
using System.Collections;

namespace Mono.Xml.Xsl {
	/// <summary>
	/// Outputter implementation for XML output method
	/// </summary>
	internal class XmlOutputter : Outputter {
		private XmlWriter _writer;
		private Hashtable _outputs;
		private XslOutput _currentOutput;

		public XmlOutputter(XmlWriter w, Hashtable o) {
			_writer = w;
			_outputs = o;
			_currentOutput = (XslOutput)o[String.Empty];
		}

		public XmlOutputter(TextWriter w, Hashtable o)
			: this(new XmlTextWriter(w), o) {}

		public override void WriteStartDocument() {
			if (_currentOutput != null && _currentOutput.OmitXmlDeclaration)
				return;
			if (_currentOutput == null || _currentOutput.Standalone == null)
				_writer.WriteStartDocument();
			else 
				_writer.WriteStartDocument(_currentOutput.Standalone == "yes");		
		}
		
		public override void WriteEndDocument() {
			_writer.WriteEndDocument();		
		}

		public override void WriteStartElement(string prefix, string localName, string nsURI) {
			_writer.WriteStartElement(prefix, localName, nsURI);
		}

		public override void WriteEndElement() {
			_writer.WriteEndElement();
		}

		public override void WriteAttributeString(string prefix, string localName, string nsURI, string value) {
			_writer.WriteAttributeString(prefix, localName, nsURI, value);
		}

		public override void WriteStartAttribute(string prefix, string localName, string nsURI) {
			_writer.WriteStartAttribute(prefix, localName, nsURI);
		}

		public override void WriteEndAttribute() {
			_writer.WriteEndAttribute();	
		}

		public override void WriteComment(string text) {
			_writer.WriteComment(text);
		}

		public override void WriteProcessingInstruction(string name, string text) {
			_writer.WriteProcessingInstruction(name, text);
		}

		public override void WriteString(string text) {
			_writer.WriteString(text);
		}

		public override void WriteRaw(string data) {
			_writer.WriteRaw(data);
		}

		public override void Done () {
			_writer.Flush ();
		}
	}
}
