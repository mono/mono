//
// XmlDictionaryWriter.cs
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
#if NET_2_0
using System;
using System.IO;
using System.Text;

namespace System.Xml
{
	internal class XmlSimpleDictionaryWriter : XmlDictionaryWriter
	{
		XmlWriter writer;

		// FIXME: find out how soapCompliant argument is used.
		public XmlSimpleDictionaryWriter (XmlWriter writer)
		{
			this.writer = writer;
		}

		public override void Close ()
		{
			writer.Close ();
		}

		public override void Flush ()
		{
			writer.Flush ();
		}

		public override string LookupPrefix (string ns)
		{
			return writer.LookupPrefix (ns);
		}

		public override void WriteBase64 (byte [] buffer, int index, int count)
		{
			writer.WriteBase64 (buffer, index, count);
		}

		public override void WriteBinHex (byte [] buffer, int index, int count)
		{
			writer.WriteBinHex (buffer, index, count);
		}

		public override void WriteCData (string text)
		{
			writer.WriteCData (text);
		}

		public override void WriteCharEntity (char ch)
		{
			writer.WriteCharEntity (ch);
		}

		public override void WriteChars (char [] buffer, int index, int count)
		{
			writer.WriteChars (buffer, index, count);
		}

		public override void WriteComment (string text)
		{
			writer.WriteComment (text);
		}

		public override void WriteDocType (string name, string pubid, string sysid, string subset)
		{
			writer.WriteDocType (name, pubid, sysid, subset);
		}

		public override void WriteEndAttribute ()
		{
			writer.WriteEndAttribute ();
		}

		public override void WriteEndDocument ()
		{
			writer.WriteEndDocument ();
		}

		public override void WriteEndElement ()
		{
			Depth--;
			NSIndex = 0;
			writer.WriteEndElement ();
		}

		public override void WriteEntityRef (string name)
		{
			writer.WriteEntityRef (name);
		}

		public override void WriteFullEndElement ()
		{
			writer.WriteFullEndElement ();
		}

		public override void WriteName (string name)
		{
			writer.WriteName (name);
		}

		public override void WriteNmToken (string name)
		{
			writer.WriteNmToken (name);
		}

		public override void WriteNode (XmlReader reader, bool defattr)
		{
			writer.WriteNode (reader, defattr);
		}

		public override void WriteProcessingInstruction (string name, string text)
		{
			writer.WriteProcessingInstruction (name, text);
		}

		public override void WriteQualifiedName (string localName, string ns)
		{
			writer.WriteQualifiedName (localName, ns);
		}

		public override void WriteRaw (string data)
		{
			writer.WriteRaw (data);
		}

		public override void WriteRaw (char [] buffer, int index, int count)
		{
			writer.WriteRaw (buffer, index, count);
		}

		public override void WriteStartAttribute (string prefix, string localName, string ns)
		{
			writer.WriteStartAttribute (prefix, localName, ns);
		}

		public override void WriteStartDocument (bool standalone)
		{
			writer.WriteStartDocument (standalone);
		}

		public override void WriteStartDocument ()
		{
			writer.WriteStartDocument ();
		}

		public override void WriteStartElement (string prefix, string localName, string ns)
		{
			Depth++;
			NSIndex = 0;
			writer.WriteStartElement (prefix, localName, ns);
		}

		public override void WriteString (string text)
		{
			writer.WriteString (text);
		}

		public override void WriteSurrogateCharEntity (char lowChar, char highChar)
		{
			writer.WriteSurrogateCharEntity (lowChar, highChar);
		}

		public override void WriteWhitespace (string ws)
		{
			writer.WriteWhitespace (ws);
		}

		public override WriteState WriteState {
			get {
				return writer.WriteState;
			}
		}

		public override string XmlLang {
			get {
				return writer.XmlLang;
			}
		}

		public override XmlSpace XmlSpace {
			get {
				return writer.XmlSpace;
			}
		}

	}
}
#endif
