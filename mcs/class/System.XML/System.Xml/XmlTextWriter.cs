//
// System.Xml.XmlTextWriter
//
// Author:
//   Kral Ferch <kral_ferch@hotmail.com>
//
// (C) 2002 Kral Ferch
//

using System;
using System.Collections;
using System.IO;
using System.Text;

namespace System.Xml
{
	public class XmlTextWriter : XmlWriter
	{
		#region Fields

		protected TextWriter w;
		protected bool openWriter = true;
		protected bool openStartElement;
		protected Stack openElements = new Stack ();
		protected XmlNamespaceManager namespaceManager = new XmlNamespaceManager (new NameTable ());

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

		private void CheckOpenWriter ()
		{
			if (!openWriter) {
				throw new InvalidOperationException ("The Writer is closed.");
			}
		}

		public override void Close ()
		{
			while (openElements.Count > 0) {
				WriteEndElement();
			}

			w.Close();

			openWriter = false;
		}

		private void CloseStartElement ()
		{
			if (openStartElement) 
			{
				w.Write(">");
				openStartElement = false;
			}
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
			if (text.IndexOf("]]>") > 0) 
			{
				throw new ArgumentException ();
			}

			CheckOpenWriter ();
			CloseStartElement ();

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
			if ((text.EndsWith("-")) || (text.IndexOf("-->") > 0)) {
				throw new ArgumentException ();
			}

			CheckOpenWriter ();
			CloseStartElement ();

			w.Write ("<!--{0}-->", text);
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

		public override void WriteEndElement ()
		{
			if (openStartElement) {
				w.Write (" />");
				openElements.Pop ();
				openStartElement = false;
			}
			else {
				w.Write ("</{0}>", openElements.Pop ());
				namespaceManager.PopScope();
			}
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

		public override void WriteProcessingInstruction (string name, string text)
		{
			if ((name == null) || (name == string.Empty) || (name.IndexOf("?>") > 0) || (text.IndexOf("?>") > 0)) {
				throw new ArgumentException ();
			}

			CheckOpenWriter ();
			CloseStartElement ();

			w.Write ("<?{0} {1}?>", name, text);
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

		public override void WriteStartElement (string prefix, string localName, string ns)
		{
			string formatXmlns = "";
			string formatPrefix = "";

			if ((prefix != String.Empty) && (ns == String.Empty))
				throw new ArgumentException ("Cannot use a prefix with an empty namespace.");

			CheckOpenWriter ();
			CloseStartElement ();

			if (ns != String.Empty) {
				string existingPrefix = namespaceManager.LookupPrefix (ns);

				if (prefix == String.Empty)
					prefix = existingPrefix;

				if (prefix != existingPrefix)
					formatXmlns = " xmlns:" + prefix + "=\"" + ns + "\"";
				else if (existingPrefix == String.Empty)
					formatXmlns = " xmlns=\"" + ns + "\"";
			}
			else if ((prefix == String.Empty) && (namespaceManager.LookupNamespace(prefix) != String.Empty)) {
				formatXmlns = " xmlns=\"\"";
			}

			if (prefix != String.Empty) {
				formatPrefix = prefix + ":";
			}

			w.Write ("<{0}{1}{2}", formatPrefix, localName, formatXmlns);

			openElements.Push (formatPrefix + localName);
			openStartElement = true;

			namespaceManager.PushScope ();
			namespaceManager.AddNamespace (prefix, ns);
		}

		[MonoTODO("Haven't done any entity replacements yet.")]
		public override void WriteString (string text)
		{
			if (text != String.Empty) {
				CheckOpenWriter ();
				CloseStartElement ();
				w.Write (text);
			}
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
