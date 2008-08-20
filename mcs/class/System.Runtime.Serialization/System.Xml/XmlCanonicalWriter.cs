//
// XmlCanonicalWriter.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2005 Novell, Inc.  http://www.novell.com
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
#if USE_DEPRECATED
#if NET_2_0
using System;
using System.IO;

namespace System.Xml
{
	[MonoTODO]
	public abstract class XmlCanonicalWriter
	{
		protected XmlCanonicalWriter ()
		{
			throw new NotImplementedException ();
		}

		public abstract void Close ();

		public abstract void Flush ();

		public abstract void WriteBase64 (byte [] buffer, int index, int count);

		public abstract void WriteCharEntity (int ch);

		public abstract void WriteComment (string text);

		public abstract void WriteComment (byte [] data, int offset, int count);

		public abstract void WriteDeclaration ();

		public abstract void WriteEndAttribute ();

		public abstract void WriteEndElement (string prefix, string localName);

		public abstract void WriteEndElement (byte [] prefix, int offset1, int count1, byte [] localName, int offset2, int count2);

		public abstract void WriteEndStartElement (bool isEmpty);

		public abstract void WriteEscapedText (string text);

		public abstract void WriteEscapedText (byte [] text, int offset, int count);

		public abstract void WriteNode (XmlReader reader);

		public abstract void WriteStartAttribute (string prefix, string localName);

		public abstract void WriteStartAttribute (byte [] prefix, int offset1, int count1, byte [] localName, int offset2, int count2);

		public abstract void WriteStartElement (string prefix, string localName);

		public abstract void WriteStartElement (byte [] prefix, int offset1, int count1, byte [] localName, int offset2, int count2);

		public abstract void WriteText (string text);

		public abstract void WriteText (byte [] text, int offset, int count);

		public abstract void WriteText (int ch);

		public abstract void WriteXmlnsAttribute (
			string prefix, string namespaceUri);

		public abstract void WriteXmlnsAttribute (byte [] prefix, int offset1, int count1, byte [] namespaceUri, int offset2, int count2);
	}
}
#endif
#endif
