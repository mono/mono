//
// Emitter.cs
//
// Authors:
//	Oleg Tkachenko (oleg@tkachenko.com)
//	Atsushi Enomoto (atsushi@ximian.com)
// (C) 2003 Oleg Tkachenko
// (C) 2004 Novell inc.
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
using System.Text;

namespace Mono.Xml.Xsl {
	/// <summary>
	/// Abstract emitter. Implementations of this class deals with outputting
	/// result tree to specific output format, such as XML, HTML, Text. 
	/// Implementations for additional formats (e.g. XHTML) as well as custom 
	/// implementations may be supported either.
	/// </summary>
	internal abstract class Emitter {
		public abstract void WriteStartDocument (Encoding encoding, StandaloneType standalone);		
		public abstract void WriteEndDocument ();						
		public abstract void WriteDocType (string type, string publicId, string systemId);
		public abstract void WriteStartElement (string prefix, string localName, string nsURI);
		public abstract void WriteEndElement ();
		public virtual void WriteFullEndElement ()
		{
			WriteEndElement ();
		}

		public abstract void WriteAttributeString (string prefix, string localName, string nsURI, string value);					
		public abstract void WriteComment (string text);		
		public abstract void WriteProcessingInstruction (string name, string text);		
		public abstract void WriteString (string text);
		public abstract void WriteCDataSection (string text);
		public abstract void WriteRaw (string data);
		public abstract void Done ();

		public virtual void WriteWhitespace (string text)
		{
			WriteString (text);
		}
	}
}
