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

		TextWriter w;
		bool nullEncoding = false;
		bool openWriter = true;
		bool openStartElement = false;
		bool openStartAttribute = false;
		bool documentStarted = false;
		bool namespaces = true;
		bool openAttribute = false;
		bool attributeWrittenForElement = false;
		Stack openElements = new Stack ();
		Formatting formatting = Formatting.None;
		int indentation = 2;
		char indentChar = ' ';
		string indentChars = "  ";
		char quoteChar = '\"';
		int indentLevel = 0;
		string indentFormatting;
		Stream baseStream = null;
		string xmlLang = null;
		XmlSpace xmlSpace = XmlSpace.None;
		bool openXmlLang = false;
		bool openXmlSpace = false;
		string openElementPrefix;
		string openElementNS;

		#endregion

		#region Constructors

		public XmlTextWriter (TextWriter w) : base ()
		{
			this.w = w;
			nullEncoding = (w.Encoding == null);
			
			try {
				baseStream = ((StreamWriter)w).BaseStream;
			}
			catch (Exception) { }
		}

		public XmlTextWriter (Stream w,	Encoding encoding) : base ()
		{
			if (encoding == null) {
				nullEncoding = true;
				encoding = new UTF8Encoding ();
			}

			this.w = new StreamWriter(w, encoding);
			baseStream = w;
		}

		public XmlTextWriter (string filename, Encoding encoding) :
			this (new FileStream (filename, FileMode.Create, FileAccess.Write, FileShare.None), encoding)
		{
		}

		#endregion

		#region Properties

		public Stream BaseStream {
			get { return baseStream; }
		}


		public Formatting Formatting {
			get { return formatting; }
			set { formatting = value; }
		}

		private bool IndentingOverriden 
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

		public bool Namespaces {
			get { return namespaces; }
			set {
				if (ws != WriteState.Start)
					throw new InvalidOperationException ("NotInWriteState.");
				
				namespaces = value;
			}
		}

		public char QuoteChar {
			get { return quoteChar; }
			set {
				if ((value != '\'') && (value != '\"'))
					throw new ArgumentException ("This is an invalid XML attribute quote character. Valid attribute quote characters are ' and \".");
				
				quoteChar = value;
			}
		}

		public override WriteState WriteState {
			get { return ws; }
		}
		
		public override string XmlLang {
			get {
				string xmlLang = null;
				int i;

				for (i = 0; i < openElements.Count; i++) 
				{
					xmlLang = ((XmlTextWriterOpenElement)openElements.ToArray().GetValue(i)).XmlLang;
					if (xmlLang != null)
						break;
				}

				return xmlLang;
			}
		}

		public override XmlSpace XmlSpace {
			get {
				XmlSpace xmlSpace = XmlSpace.None;
				int i;

				for (i = 0; i < openElements.Count; i++) 
				{
					xmlSpace = ((XmlTextWriterOpenElement)openElements.ToArray().GetValue(i)).XmlSpace;
					if (xmlSpace != XmlSpace.None)
						break;
				}

				return xmlSpace;
			}
		}

		#endregion

		#region Methods
		private void AddMissingElementXmlns ()
		{
			// output namespace declaration if not exist.
			string prefix = openElementPrefix;
			string ns = openElementNS;
			if (ns != null/* && LookupPrefix (ns) != prefix*/) 
			{
				string formatXmlns = String.Empty;
				if (ns != String.Empty)
				{
					string existingPrefix = namespaceManager.LookupPrefix (ns);
					bool addDefaultNamespace = false;

					if (existingPrefix == null) 
					{
						namespaceManager.AddNamespace (prefix, ns);
						addDefaultNamespace = true;
					}

					if (prefix == String.Empty)
						prefix = existingPrefix;

					if (prefix != existingPrefix)
						formatXmlns = String.Format (" xmlns:{0}={1}{2}{1}", prefix, quoteChar, ns);
					else if (addDefaultNamespace)
						formatXmlns = String.Format (" xmlns={0}{1}{0}", quoteChar, ns);
				} 
				else if ((prefix == String.Empty) && (namespaceManager.LookupNamespace (prefix) != String.Empty)) 
				{
					namespaceManager.AddNamespace (prefix, ns);
					formatXmlns = String.Format (" xmlns={0}{0}", quoteChar);
				}
				if(formatXmlns != String.Empty)
					w.Write(formatXmlns);
				openElementPrefix = null;
				openElementNS = null;
			}
		}

		private void CheckState ()
		{
			if (!openWriter) {
				throw new InvalidOperationException ("The Writer is closed.");
			}
			if ((documentStarted == true) && (formatting == Formatting.Indented) && (!IndentingOverriden)) {
				indentFormatting = w.NewLine;
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
			CloseOpenAttributeAndElements ();

			w.Close();
			ws = WriteState.Closed;
			openWriter = false;
		}

		private void CloseOpenAttributeAndElements ()
		{
			if (openAttribute)
				WriteEndAttribute ();

			while (openElements.Count > 0) {
				WriteEndElement();
			}
		}

		private void CloseStartElement ()
		{
			if (!openStartElement)
				return;

			AddMissingElementXmlns ();

			w.Write (">");
			ws = WriteState.Content;
			openStartElement = false;
			attributeWrittenForElement = false;
		}

		public override void Flush ()
		{
			w.Flush ();
		}

		public override string LookupPrefix (string ns)
		{
			string prefix = namespaceManager.LookupPrefix (ns);

			// XmlNamespaceManager has changed to return null when NSURI not found.
			// (Contradiction to the documentation.)
			//if (prefix == String.Empty)
			//	prefix = null;
			return prefix;
		}

		private void UpdateIndentChars ()
		{
			indentChars = "";
			for (int i = 0; i < indentation; i++)
				indentChars += indentChar;
		}

		public override void WriteBase64 (byte[] buffer, int index, int count)
		{
			w.Write (Convert.ToBase64String (buffer, index, count));
		}

		[MonoTODO]
		public override void WriteBinHex (byte[] buffer, int index, int count)
		{
			throw new NotImplementedException ();
		}

		public override void WriteCData (string text)
		{
			if (text.IndexOf("]]>") > 0)
				throw new ArgumentException ();

			CheckState ();
			CloseStartElement ();

			w.Write("<![CDATA[{0}]]>", text);
		}

		public override void WriteCharEntity (char ch)
		{
			Int16	intCh = (Int16)ch;

			// Make sure the character is not in the surrogate pair
			// character range, 0xd800- 0xdfff
			if ((intCh >= -10240) && (intCh <= -8193))
				throw new ArgumentException ("Surrogate Pair is invalid.");

			w.Write("&#x{0:X};", intCh);
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

		public override void WriteDocType (string name, string pubid, string sysid, string subset)
		{
			if (name == null || name.Trim ().Length == 0)
				throw new ArgumentException ("Invalid DOCTYPE name", "name");

			w.Write ("<!DOCTYPE ");
			w.Write (name);
			if (pubid != null) {
				w.Write (String.Format (" PUBLIC {0}{1}{0} {0}{2}{0}", quoteChar, pubid, sysid));
			} else if (sysid != null) {
				w.Write (String.Format (" SYSTEM {0}{1}{0}", quoteChar, sysid));
			}

			if (subset != null)
				w.Write ("[" + subset + "]");

			w.Write('>');
		}

		public override void WriteEndAttribute ()
		{
			if (!openAttribute)
				throw new InvalidOperationException("Token EndAttribute in state Start would result in an invalid XML document.");

			CheckState ();

			if (openXmlLang) {
				w.Write (xmlLang);
				openXmlLang = false;
				((XmlTextWriterOpenElement)openElements.Peek()).XmlLang = xmlLang;
			}

			if (openXmlSpace) 
			{
				w.Write (xmlSpace.ToString ().ToLower ());
				openXmlSpace = false;
				((XmlTextWriterOpenElement)openElements.Peek()).XmlSpace = xmlSpace;
			}

			w.Write ("{0}", quoteChar);

			openAttribute = false;
		}

		public override void WriteEndDocument ()
		{
			CloseOpenAttributeAndElements ();

			if ((ws == WriteState.Start) || (ws == WriteState.Prolog))
				throw new ArgumentException ("This document does not have a root element.");

			ws = WriteState.Start;
		}

		public override void WriteEndElement ()
		{
			WriteEndElementInternal (false);
		}

		private void WriteEndElementInternal (bool fullEndElement)
		{
			if (openElements.Count == 0)
				throw new InvalidOperationException("There was no XML start tag open.");

			indentLevel--;
			CheckState ();
			AddMissingElementXmlns ();

			if (openStartElement) {
				if (openAttribute)
					WriteEndAttribute ();
				if (fullEndElement)
					w.Write ("></{0}>", ((XmlTextWriterOpenElement)openElements.Peek ()).Name);
				else
					w.Write (" />");

				openElements.Pop ();
				openStartElement = false;
			} else {
				w.Write ("{0}</{1}>", indentFormatting, openElements.Pop ());
			}

			namespaceManager.PopScope();
		}

		[MonoTODO]
		public override void WriteEntityRef (string name)
		{
			throw new NotImplementedException ();
		}

		public override void WriteFullEndElement ()
		{
			WriteEndElementInternal (true);
		}

		private void CheckValidChars (string name, bool firstOnlyLetter)
		{
			foreach (char c in name) {
				if (XmlConvert.IsInvalid (c, firstOnlyLetter))
					throw new ArgumentException ("There is an invalid character: '" + c +
								     "'", "name");
			}
		}

		public override void WriteName (string name)
		{
			CheckValidChars (name, true);
			w.Write (name);
		}

		public override void WriteNmToken (string name)
		{
			CheckValidChars (name, false);
			w.Write (name);
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
			if (localName == null || localName == String.Empty)
				throw new ArgumentException ();

			CheckState ();
			w.Write ("{0}:{1}", ns, localName);
		}

		public override void WriteRaw (string data)
		{
			WriteStringInternal (data, false);
		}

		[MonoTODO]
		public override void WriteRaw (char[] buffer, int index, int count)
		{
			throw new NotImplementedException ();
		}

		public override void WriteStartAttribute (string prefix, string localName, string ns)
		{
			if ((prefix == "xml") && (localName == "lang"))
				openXmlLang = true;

			if ((prefix == "xml") && (localName == "space"))
				openXmlSpace = true;

			if ((prefix == "xmlns") && (localName == "xmlns"))
				throw new ArgumentException ("Prefixes beginning with \"xml\" (regardless of whether the characters are uppercase, lowercase, or some combination thereof) are reserved for use by XML.");

			CheckState ();

			if (ws == WriteState.Content)
				throw new InvalidOperationException ("Token StartAttribute in state " + WriteState + " would result in an invalid XML document.");

			if (prefix == null)
				prefix = String.Empty;

			if (ns == null)
				ns = String.Empty;

			string formatPrefix = "";
			string formatSpace = "";

			if (ns != String.Empty) 
			{
				string existingPrefix = namespaceManager.LookupPrefix (ns);

				if (prefix == String.Empty)
					prefix = (existingPrefix == null) ?
						String.Empty : existingPrefix;
			}

			if (prefix != String.Empty) 
			{
				formatPrefix = prefix + ":";
			}

			if (openStartElement || attributeWrittenForElement)
				formatSpace = " ";

			w.Write ("{0}{1}{2}={3}", formatSpace, formatPrefix, localName, quoteChar);

			openAttribute = true;
			attributeWrittenForElement = true;
			ws = WriteState.Attribute;
		}

		public override void WriteStartDocument ()
		{
			WriteStartDocument ("");
		}

		public override void WriteStartDocument (bool standalone)
		{
			string standaloneFormatting;

			if (standalone == true)
				standaloneFormatting = String.Format (" standalone={0}yes{0}", quoteChar);
			else
				standaloneFormatting = String.Format (" standalone={0}no{0}", quoteChar);

			WriteStartDocument (standaloneFormatting);
		}

		private void WriteStartDocument (string standaloneFormatting)
		{
			if (documentStarted == true)
				throw new InvalidOperationException("WriteStartDocument should be the first call.");

			CheckState ();

			string encodingFormatting = "";

			if (!nullEncoding) 
				encodingFormatting = String.Format (" encoding={0}{1}{0}", quoteChar, w.Encoding.HeaderName);

			w.Write("<?xml version={0}1.0{0}{1}{2}?>", quoteChar, encodingFormatting, standaloneFormatting);
			ws = WriteState.Prolog;
		}

		public override void WriteStartElement (string prefix, string localName, string ns)
		{
			if (!Namespaces && (((prefix != null) && (prefix != String.Empty))
				|| ((ns != null) && (ns != String.Empty))))
				throw new ArgumentException ("Cannot set the namespace if Namespaces is 'false'.");

			WriteStartElementInternal (prefix, localName, ns);
		}

		private void WriteStartElementInternal (string prefix, string localName, string ns)
		{
			if ((prefix != null && prefix != String.Empty) && ((ns == null) || (ns == String.Empty)))
				throw new ArgumentException ("Cannot use a prefix with an empty namespace.");

			CheckState ();
			CloseStartElement ();
			
			if (prefix == null)
				prefix = namespaceManager.LookupPrefix (ns);
			if (prefix == null)
				prefix = String.Empty;

			string formatXmlns = "";
			string formatPrefix = "";

			if(ns != null) {
				if (prefix != String.Empty)
					formatPrefix = prefix + ":";
			}

			w.Write ("{0}<{1}{2}{3}", indentFormatting, formatPrefix, localName, formatXmlns);
	

			openElements.Push (new XmlTextWriterOpenElement (formatPrefix + localName));
			ws = WriteState.Element;
			openStartElement = true;
			openElementNS = ns;
			openElementPrefix = prefix;

			namespaceManager.PushScope ();
//			if(ns != null)
//				namespaceManager.AddNamespace (prefix, ns);
			indentLevel++;
		}

		public override void WriteString (string text)
		{
			if (ws == WriteState.Prolog)
				throw new InvalidOperationException ("Token content in state Prolog would result in an invalid XML document.");

			WriteStringInternal (text, true);
		}

		private void WriteStringInternal (string text, bool entitize)
		{
			if (text == null)
				text = String.Empty;

			if (text != String.Empty) 
			{
				CheckState ();

				if (entitize)
				{
					text = text.Replace ("&", "&amp;");
					text = text.Replace ("<", "&lt;");
					text = text.Replace (">", "&gt;");
					
					if (openAttribute) 
					{
						if (quoteChar == '"')
							text = text.Replace ("\"", "&quot;");
						else
							text = text.Replace ("'", "&apos;");
					}
				}

				if (!openAttribute)
				{
					IndentingOverriden = true;
					CloseStartElement ();
				}
				if (!openXmlLang && !openXmlSpace)
					w.Write (text);
				else 
				{
					if (openXmlLang)
						xmlLang = text;
					else 
					{
						switch (text) 
						{
							case "default":
								xmlSpace = XmlSpace.Default;
								break;
							case "preserve":
								xmlSpace = XmlSpace.Preserve;
								break;
							default:
								throw new ArgumentException ("'{0}' is an invalid xml:space value.");
						}
					}
				}
			}
		}

		[MonoTODO]
		public override void WriteSurrogateCharEntity (char lowChar, char highChar)
		{
			throw new NotImplementedException ();
		}

		public override void WriteWhitespace (string ws)
		{
			foreach (char c in ws) {
				if ((c != ' ') && (c != '\t') && (c != '\r') && (c != '\n'))
					throw new ArgumentException ();
			}

			w.Write (ws);
		}

		#endregion
	}
}
