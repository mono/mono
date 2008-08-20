//
// XmlC14NWriter.cs
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
	public sealed class XmlC14NWriter : XmlCanonicalWriter
	{
		bool include_comments;

		public XmlC14NWriter (Stream stream)
		{
			throw new NotImplementedException ();
		}

		public XmlC14NWriter (Stream stream, bool includeComments,
			params string [] inclusivePrefixes)
		{
			throw new NotImplementedException ();
		}

		public bool IncludeComments {
			get { return include_comments; }
			set {
				throw new NotImplementedException ();
			}
		}

		public override void Close ()
		{
			Flush ();
		}

		public override void Flush ()
		{
			throw new NotImplementedException ();
		}

		public void SetOutput (Stream stream)
		{
			throw new NotImplementedException ();
		}

		public void SetOutput (Stream stream, bool includeComments,
			params string [] inclusivePrefixes)
		{
			throw new NotImplementedException ();
		}

		public override void WriteBase64 (byte [] buffer, int index, int count)
		{
			throw new NotImplementedException ();
		}

		public override void WriteCharEntity (int ch)
		{
			throw new NotImplementedException ();
		}

		public override void WriteComment (string text)
		{
			throw new NotImplementedException ();
		}

		public override void WriteComment (byte [] data, int offset, int count)
		{
			throw new NotImplementedException ();
		}

		public override void WriteDeclaration ()
		{
			throw new NotImplementedException ();
		}

		public override void WriteEndAttribute ()
		{
			throw new NotImplementedException ();
		}

		public override void WriteEndElement (string prefix, string localName)
		{
			throw new NotImplementedException ();
		}

		public override void WriteEndElement (byte [] prefix, int offset1, int count1, byte [] localName, int offset2, int count2)
		{
			throw new NotImplementedException ();
		}

		public override void WriteEndStartElement (bool isEmpty)
		{
			throw new NotImplementedException ();
		}

		public override void WriteEscapedText (string text)
		{
			throw new NotImplementedException ();
		}

		public override void WriteEscapedText (byte [] text, int offset, int count)
		{
			throw new NotImplementedException ();
		}

		public override void WriteNode (XmlReader reader)
		{
			throw new NotImplementedException ();
		}

		public override void WriteStartAttribute (string prefix, string localName)
		{
			throw new NotImplementedException ();
		}

		public override void WriteStartAttribute (byte [] prefix, int offset1, int count1, byte [] localName, int offset2, int count2)
		{
			throw new NotImplementedException ();
		}

		public override void WriteStartElement (string prefix, string localName)
		{
			throw new NotImplementedException ();
		}

		public override void WriteStartElement (byte [] prefix, int offset1, int count1, byte [] localName, int offset2, int count2)
		{
			throw new NotImplementedException ();
		}

		public override void WriteText (string text)
		{
			throw new NotImplementedException ();
		}

		public override void WriteText (int ch)
		{
			throw new NotImplementedException ();
		}

		public override void WriteText (byte [] text, int offset, int count)
		{
			throw new NotImplementedException ();
		}

		public override void WriteXmlnsAttribute (
			string prefix, string namespaceUri)
		{
			throw new NotImplementedException ();
		}

		public override void WriteXmlnsAttribute (byte [] prefix, int offset1, int count1, byte [] namespaceUri, int offset2, int count2)
		{
			throw new NotImplementedException ();
		}
	}
}
#endif
#endif
