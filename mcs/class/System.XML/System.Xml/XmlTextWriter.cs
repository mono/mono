//
// System.Xml.XmlTextWriter
//
// Author:
//   Kral Ferch <kral_ferch@hotmail.com>
//
// (C) 2002 Kral Ferch
//

using System;
using System.IO;
using System.Text;

namespace System.Xml
{
	public class XmlTextWriter : XmlWriter
	{
		#region Fields

		TextWriter w;

		#endregion

		#region Constructors

		public XmlTextWriter (TextWriter w) : base ()
		{
			this.w = w;
		}

		public XmlTextWriter (Stream w,	Encoding encoding) : base ()
		{
			this.w = new StreamWriter(w, encoding);
		}

		public XmlTextWriter (string filename, Encoding encoding) : base ()
		{
			this.w = new StreamWriter(filename, false, encoding);
		}

		#endregion

		#region Properties

		[MonoTODO]
		public Stream BaseStream {
			get { throw new NotImplementedException(); }
		}


		[MonoTODO]
		public Formatting Formatting {
			get { throw new NotImplementedException(); }
			set { throw new NotImplementedException(); }
		}

		[MonoTODO]
		public int Indentation {
			get { throw new NotImplementedException(); }
			set { throw new NotImplementedException(); }
		}

		[MonoTODO]
		public char IndentChar {
			get { throw new NotImplementedException(); }
			set { throw new NotImplementedException(); }
		}

		[MonoTODO]
		public bool Namespaces {
			get { throw new NotImplementedException(); }
			set { throw new NotImplementedException(); }
		}

		[MonoTODO]
		public char QuoteChar {
			get { throw new NotImplementedException(); }
			set { throw new NotImplementedException(); }
		}

		[MonoTODO]
		public override WriteState WriteState {
			get { throw new NotImplementedException(); }
		}
		
		[MonoTODO]
		public override string XmlLang {
			get { throw new NotImplementedException(); }
		}

		[MonoTODO]
		public override XmlSpace XmlSpace {
			get { throw new NotImplementedException(); }
		}

		#endregion

		#region Methods

		[MonoTODO]
		public override void Close ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override void Flush ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override string LookupPrefix (string ns)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override void WriteBase64 (byte[] buffer, int index, int count)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override void WriteBinHex (byte[] buffer, int index, int count)
		{
			throw new NotImplementedException ();
		}

		public override void WriteCData (string text)
		{
			w.Write("<![CDATA[{0}]]>", text);
		}

		[MonoTODO]
		public override void WriteCharEntity (char ch)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override void WriteChars (char[] buffer, int index, int count)
		{
			throw new NotImplementedException ();
		}

		public override void WriteComment (string text)
		{
			w.Write("<!--{0}-->", text);
		}

		[MonoTODO]
		public override void WriteDocType (string name, string pubid, string sysid, string subset)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override void WriteEndAttribute ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override void WriteEndDocument ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override void WriteEndElement ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override void WriteEntityRef (string name)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override void WriteFullEndElement ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override void WriteName (string name)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override void WriteNmToken (string name)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override void WriteProcessingInstruction (string name, string text)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override void WriteQualifiedName (string localName, string ns)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override void WriteRaw (string data)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override void WriteRaw (char[] buffer, int index, int count)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override void WriteStartAttribute (string prefix, string localName, string ns)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override void WriteStartDocument ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override void WriteStartDocument (bool standalone)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override void WriteStartElement (string prefix, string localName, string ns)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override void WriteString (string text)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override void WriteSurrogateCharEntity (char lowChar, char highChar)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override void WriteWhitespace (string ws)
		{
			throw new NotImplementedException ();
		}

		#endregion
	}
}
