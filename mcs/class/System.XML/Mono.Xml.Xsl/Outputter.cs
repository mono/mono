//
// Outputter.cs
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

namespace Mono.Xml.Xsl {
	/// <summary>
	/// Abstract XSLT outputter. 
	/// Transformation classes build result tree using only with this class API.
	/// Implementations of this class outputs result tree to an Emitter, which emits 
	/// it further down to real consumers.
	/// </summary>
	internal abstract class Outputter {
		public void WriteStartElement (string localName, string nsURI)
		{
			WriteStartElement (null, localName, nsURI);
		}
		
		public abstract void WriteStartElement (string prefix, string localName, string nsURI);
		public abstract void WriteEndElement ();
		public virtual void WriteFullEndElement ()
		{
			WriteEndElement ();
		}
		
		public void WriteAttributeString (string localName, string value)
		{
			WriteAttributeString ("", localName, "", value);
		}
		
		public abstract void WriteAttributeString (string prefix, string localName, string nsURI, string value);
		public abstract void WriteNamespaceDecl (string prefix, string nsUri);		
						
		public abstract void WriteComment (string text);
		
		public abstract void WriteProcessingInstruction (string name, string text);
		
		public abstract void WriteString (string text);
		public abstract void WriteRaw (string data);
		public abstract void WriteWhitespace (string text);
		
		public abstract void Done ();

		public abstract bool CanProcessAttributes { get; }

		public abstract bool InsideCDataSection { get; set; }
	}
}
