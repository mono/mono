//
// DefaultXmlWriter.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2006 Novell, Inc. http://www.novell.com
//
using System;
using System.Globalization;
using System.Collections;

namespace System.Xml
{
	internal class DefaultXmlWriter : XmlWriter
	{
		XmlWriter writer;
		WriteState state = WriteState.Start;
		bool delegate_write_state;

		public DefaultXmlWriter (XmlWriter writer)
			: this (writer, true)
		{
		}

		public DefaultXmlWriter (XmlWriter writer, bool delegateWriteState)
		{
			this.writer = writer;
			delegate_write_state = delegateWriteState;
		}
	
		protected XmlWriter Writer {
			get { return writer; }
		}

		private void CloseStartElement ()
		{
			state = WriteState.Content;
		}

		public override void Close ()
		{
			writer.Close ();
			state = WriteState.Closed;
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
			state = WriteState.Content;
		}
	
		public override void WriteBinHex (byte [] buffer, int index, int count)
		{
			writer.WriteBinHex (buffer, index, count);
			state = WriteState.Content;
		}
	
		public override void WriteCData (string text)
		{
			writer.WriteCData (text);
			state = WriteState.Content;
		}
	
		public override void WriteCharEntity (char ch)
		{
			writer.WriteCharEntity (ch);
			state = WriteState.Content;
		}
	
		public override void WriteChars (char [] buffer, int index, int count)
		{
			writer.WriteChars (buffer, index, count);
			state = WriteState.Content;
		}
	
		public override void WriteComment (string text)
		{
			writer.WriteComment (text);
			if (state == WriteState.Start)
				state = WriteState.Prolog;
			else
				state = WriteState.Content;
		}
	
		public override void WriteDocType (string name, string pubid, string sysid, string subset)
		{
			writer.WriteDocType (name, pubid, sysid, subset);
			state = WriteState.Prolog;
		}
	
		public override void WriteEndAttribute ()
		{
			writer.WriteEndAttribute ();
			state = WriteState.Element;
		}
	
		public override void WriteEndDocument ()
		{
			writer.WriteEndDocument ();
			state = WriteState.Start;
		}
	
		public override void WriteEndElement ()
		{
			writer.WriteEndElement ();
			state = WriteState.Content;
		}
	
		public override void WriteEntityRef (string name)
		{
			writer.WriteEntityRef (name);
			state = WriteState.Content;
		}
	
		public override void WriteFullEndElement ()
		{
			writer.WriteFullEndElement ();
			state = WriteState.Content;
		}
	
		public override void WriteName (string name)
		{
			writer.WriteName (name);
			state = WriteState.Content;
		}
	
		public override void WriteNmToken (string name)
		{
			writer.WriteNmToken (name);
			state = WriteState.Content;
		}
	
		public override void WriteNode (XmlReader reader, bool defattr)
		{
			writer.WriteNode (reader, defattr);
		}
	
		public override void WriteProcessingInstruction (string name, string text)
		{
			writer.WriteProcessingInstruction (name, text);
			if (state == WriteState.Start)
				state = WriteState.Prolog;
			else
				state = WriteState.Content;
		}
	
		public override void WriteQualifiedName (string localName, string ns)
		{
			writer.WriteQualifiedName (localName, ns);
			state = WriteState.Content;
		}
	
		public override void WriteRaw (string data)
		{
			writer.WriteRaw (data);
			if (state == WriteState.Start)
				state = WriteState.Prolog;
			else
				state = WriteState.Content;
		}
	
		public override void WriteRaw (char [] buffer, int index, int count)
		{
			writer.WriteRaw (buffer, index, count);
			if (state == WriteState.Start)
				state = WriteState.Prolog;
			else
				state = WriteState.Content;
		}
	
		public override void WriteStartAttribute (string prefix, string localName, string ns)
		{
			writer.WriteStartAttribute (prefix, localName, ns);
			state = WriteState.Attribute;
		}
	
		public override void WriteStartDocument (bool standalone)
		{
			writer.WriteStartDocument (standalone);
			state = WriteState.Prolog;
		}
	
		public override void WriteStartDocument ()
		{
			writer.WriteStartDocument ();
			state = WriteState.Prolog;
		}
	
		public override void WriteStartElement (string prefix, string localName, string ns)
		{
			writer.WriteStartElement (prefix, localName, ns);
			state = WriteState.Element;
		}
	
		public override void WriteString (string text)
		{
			writer.WriteString (text);
			state = WriteState.Content;
		}
	
		public override void WriteSurrogateCharEntity (char lowChar, char highChar)
		{
			writer.WriteSurrogateCharEntity (lowChar, highChar);
			state = WriteState.Content;
		}
	
		public override void WriteWhitespace (string ws)
		{
			writer.WriteWhitespace (ws);
			if (state == WriteState.Start)
				state = WriteState.Prolog;
			else
				state = WriteState.Content;
		}
	
		public override WriteState WriteState {
			get {
				if (delegate_write_state)
					return writer.WriteState;
				else
					return state;
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
