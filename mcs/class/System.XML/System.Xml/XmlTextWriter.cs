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
		protected bool nullEncoding = false;
		protected bool openWriter = true;
		protected bool openStartElement;
		protected bool documentStarted = false;
		protected Stack openElements = new Stack ();
		protected XmlNamespaceManager namespaceManager = new XmlNamespaceManager (new NameTable ());
		protected Formatting formatting = Formatting.None;
		protected int indentation = 2;
		protected char indentChar = ' ';
		protected string indentChars = "  ";
		protected char quoteChar = '\"';
		protected int indentLevel = 0;
		protected string indentFormatting;

		#endregion

		#region Constructors

		public XmlTextWriter (TextWriter w) : base ()
		{
			this.w = w;
		}

		public XmlTextWriter (Stream w,	Encoding encoding) : base ()
		{
			if (encoding == null) {
				nullEncoding = true;
				encoding = new UTF8Encoding ();
			}

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


		public Formatting Formatting {
			get { return formatting; }
			set { formatting = value; }
		}

		public bool IndentingOverriden 
		{
			get {
				if (openElements.Count == 0)
					return false;
				else
					return (((XmlTextWriterOpenElement)openElements.Peek()).IndentingOverriden);
			}
			set {
				if (openElements.Count > 0)
					((XmlTextWriterOpenElement)openElements.Peek()).IndentingOverriden = value;
			}
		}

		public int Indentation {
			get { return indentation; }
			set {
				indentation = value;
				UpdateIndentChars ();
			}
		}

		public char IndentChar {
			get { return indentChar; }
			set {
				indentChar = value;
				UpdateIndentChars ();
			}
		}

		[MonoTODO]
		public bool Namespaces {
			get { throw new NotImplementedException(); }
			set { throw new NotImplementedException(); }
		}

		[MonoTODO]
		public char QuoteChar {
			get { return quoteChar; }
			set {
				if ((value != '\'') && (value != '\"'))
					throw new ArgumentException ("This is an invalid XML attribute quote character. Valid attribute quote characters are ' and \".");
				
				quoteChar = value;
			}
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

		private void CheckState ()
		{
			if (!openWriter) {
				throw new InvalidOperationException ("The Writer is closed.");
			}

			if ((documentStarted == true) && (formatting == Formatting.Indented) && (!IndentingOverriden)) {
				indentFormatting = "\r\n";
				if (indentLevel > 0) {
					for (int i = 0; i < indentLevel; i++)
						indentFormatting += indentChars;
				}
			}
			else
				indentFormatting = "";

			documentStarted = true;
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

		public override void Flush ()
		{
			w.Flush ();
		}

		[MonoTODO]
		public override string LookupPrefix (string ns)
		{
			throw new NotImplementedException ();
		}

		private void UpdateIndentChars ()
		{
			indentChars = "";
			for (int i = 0; i < indentation; i++)
				indentChars += indentChar;
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

			CheckState ();
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

			CheckState ();
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
			if (openElements.Count == 0)
				throw new InvalidOperationException("There was no XML start tag open.");

			indentLevel--;

			CheckState ();

			if (openStartElement) {
				w.Write (" />");
				openElements.Pop ();
				openStartElement = false;
			}
			else {
				w.Write ("{0}</{1}>", indentFormatting, openElements.Pop ());
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

			CheckState ();
			CloseStartElement ();

			w.Write ("{0}<?{1} {2}?>", indentFormatting, name, text);
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

		public override void WriteStartDocument ()
		{
			WriteStartDocument ("");
		}

		public override void WriteStartDocument (bool standalone)
		{
			string standaloneFormatting;

			if (standalone == true)
				standaloneFormatting = " standalone=\"yes\"";
			else
				standaloneFormatting = " standalone=\"no\"";

			WriteStartDocument (standaloneFormatting);
		}

		private void WriteStartDocument (string standaloneFormatting)
		{
			if (documentStarted == true)
				throw new InvalidOperationException("WriteStartDocument should be the first call.");

			CheckState ();

			string encodingFormatting = "";

			if (!nullEncoding)
				encodingFormatting = " encoding=\"" + w.Encoding.HeaderName + "\"";

			w.Write("<?xml version=\"1.0\"{0}{1}?>", encodingFormatting, standaloneFormatting);
		}

		public override void WriteStartElement (string prefix, string localName, string ns)
		{
			if ((prefix != String.Empty) && (ns == String.Empty))
				throw new ArgumentException ("Cannot use a prefix with an empty namespace.");

			CheckState ();
			CloseStartElement ();

			string formatXmlns = "";
			string formatPrefix = "";

			if (ns != String.Empty) 
			{
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

			w.Write ("{0}<{1}{2}{3}", indentFormatting, formatPrefix, localName, formatXmlns);

			openElements.Push (new XmlTextWriterOpenElement (formatPrefix + localName));
			openStartElement = true;

			namespaceManager.PushScope ();
			namespaceManager.AddNamespace (prefix, ns);

			indentLevel++;
		}

		[MonoTODO("Haven't done any entity replacements yet.")]
		public override void WriteString (string text)
		{
			if (text != String.Empty) {
				CheckState ();
				CloseStartElement ();
				w.Write (text);
			}

			IndentingOverriden = true;
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
