//
// TextEmitter.cs
//
// Authors:
//	Oleg Tkachenko (oleg@tkachenko.com)
//	Atsushi Enomoto (atsushi@ximian.com)
//	
// (C) 2003 Oleg Tkachenko
// (C) 2004 Atsushi Enomoto
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

namespace Mono.Xml.Xsl 
{
	/// <summary>
	/// Emitetr, which emits result tree according to "text" output method.
	/// </summary>
	internal class TextEmitter : Emitter 
	{
		TextWriter writer;
			
		public TextEmitter (TextWriter writer) {
			this.writer = writer;
		}

		#region # Emitter's methods implementaion			
		
		public override void WriteStartDocument (Encoding encoding, StandaloneType standalone) {
			//Do nothing
		}
		
		public override void WriteEndDocument () {
			//Do nothing
		}

		public override void WriteDocType (string type, string publicId, string systemId) {
			//Do nothing
		}

		public override void WriteStartElement (string prefix, string localName, string nsURI) {
			//Do nothing
		}

		public override void WriteEndElement () {
			//Do nothing
		}

		public override void WriteAttributeString (string prefix, string localName, string nsURI, string value) {
			//Do nothing
		}
		
		public override void WriteComment (string text) {
			//Do nothing
		}

		public override void WriteProcessingInstruction (string name, string text) {
			//Do nothing
		}

		public override void WriteString (string text) {
			writer.Write (text);
		}

		public override void WriteRaw (string data) {
			writer.Write (data);
		}

		public override void WriteCDataSection (string text) {
			writer.Write (text);
		}

		public override void Done () {
			//Do nothing
		}
		#endregion
	}
}
