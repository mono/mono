//
// System.Xml.XmlTextWriter
//
// Author:
//   Kral Ferch <kral_ferch@hotmail.com>
//   Atsushi Enomoto <ginga@kit.hi-ho.ne.jp>
//
// (C) 2002 Kral Ferch
// (C) 2003 Atsushi Enomoto
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
		const string XmlnsNamespace = "http://www.w3.org/2000/xmlns/";

		WriteState ws = WriteState.Start;
		TextWriter w;
		bool nullEncoding = false;
		bool openWriter = true;
		bool openStartElement = false;
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
		bool hasRoot = false;
		Hashtable newAttributeNamespaces = new Hashtable ();

		XmlNamespaceManager namespaceManager = new XmlNamespaceManager (new NameTable ());
		string savingAttributeValue = String.Empty;
		bool saveAttributeValue = false;
		string savedAttributePrefix;

		#endregion

		#region Constructors

		public XmlTextWriter (TextWriter w) : base ()
		{
			this.w = w;
			nullEncoding = (w.Encoding == null);
			StreamWriter sw = w as StreamWriter;
			if (sw != null)
				baseStream = ((StreamWriter)w).BaseStream;
		}

		public XmlTextWriter (Stream w,	Encoding encoding) : base ()
		{
			if (encoding == null) {
				nullEncoding = true;
				this.w = new StreamWriter (w);
			} else 
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
			openElementPrefix = null;
			openElementNS = null;

			// LAMESPEC: If prefix was already assigned another nsuri, then this element's nsuri goes away!

			if (ns != null) 
			{
				string formatXmlns = String.Empty;
				if (ns != String.Empty)
				{
					string existingPrefix = namespaceManager.LookupPrefix (ns);

					if (existingPrefix != prefix)
					{
						if (prefix != string.Empty)
							formatXmlns = String.Format ("xmlns:{0}={1}{2}{1}", prefix, quoteChar, ns);
						else
							formatXmlns = String.Format ("xmlns={0}{1}{0}", quoteChar, ns);
						namespaceManager.AddNamespace (prefix, ns);
					}
				} 
				else if ((prefix == String.Empty) && (namespaceManager.LookupNamespace (prefix) != ns)) 
				{
					namespaceManager.AddNamespace (prefix, ns);
					formatXmlns = String.Format ("xmlns={0}{0}", quoteChar);
				}
				if(formatXmlns != String.Empty) {
					w.Write (' ');
					w.Write(formatXmlns);
				}
			}

			if (newAttributeNamespaces.Count > 0)
			{
				foreach (DictionaryEntry ent in newAttributeNamespaces)
				{
					string ans = (string) ent.Value;
					string aprefix = (string) ent.Key;

					if (namespaceManager.LookupNamespace (aprefix) == ans)
						continue;
	
					string formatXmlns = String.Format (" xmlns:{0}={1}{2}{1}", aprefix, quoteChar, ans);
					w.Write(formatXmlns);
				}
			}
			newAttributeNamespaces.Clear ();
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
			newAttributeNamespaces.Clear ();
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
			CheckState ();

			if (!openAttribute) {
				IndentingOverriden = true;
				CloseStartElement ();
			}

			w.Write (Convert.ToBase64String (buffer, index, count));
		}

		// BinHex??
		public override void WriteBinHex (byte[] buffer, int index, int count)
		{
			CheckState ();

			if (!openAttribute) {
				IndentingOverriden = true;
				CloseStartElement ();
			}

			if (index < 0)
				throw new ArgumentOutOfRangeException ("index", index, "index must be non negative integer.");
			if (count < 0)
				throw new ArgumentOutOfRangeException ("count", count, "count must be non negative integer.");
			if (buffer.Length < index + count)
				throw new ArgumentOutOfRangeException ("index and count must be smaller than the length of the buffer.");

			for (int i = index; i < count; i++) {
				int val = buffer [i];
				int high = val >> 4;
				int low = val & 15;
				if (high > 9)
					w.Write ((char) (high + 55));
				else
					w.Write ((char) (high + 0x30));
				if (low > 9)
					w.Write ((char) (low + 55));
				else
					w.Write ((char) (low + 0x30));
			}
		}

		public override void WriteCData (string text)
		{
			if (text.IndexOf("]]>") > 0)
				throw new ArgumentException ();

			CheckState ();
			CloseStartElement ();
			
			w.Write ("<![CDATA[");
			w.Write (text);
			w.Write ("]]>");
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

		public override void WriteChars (char[] buffer, int index, int count)
		{
			CheckState ();

			if (!openAttribute) {
				IndentingOverriden = true;
				CloseStartElement ();
			}

			w.Write (buffer, index, count);
		}

		public override void WriteComment (string text)
		{
			if ((text.EndsWith("-")) || (text.IndexOf("-->") > 0)) {
				throw new ArgumentException ();
			}

			CheckState ();
			CloseStartElement ();

			w.Write ("<!--");
			w.Write (text);
			w.Write ("-->");
		}

		public override void WriteDocType (string name, string pubid, string sysid, string subset)
		{
			if (name == null || name.Trim (XmlChar.WhitespaceChars).Length == 0)
				throw new ArgumentException ("Invalid DOCTYPE name", "name");

			w.Write ("<!DOCTYPE ");
			w.Write (name);
			if (pubid != null) {
				w.Write (" PUBLIC ");
				w.Write (quoteChar);
				w.Write (pubid);
				w.Write (quoteChar);
				w.Write (' ');
				w.Write (quoteChar);
				w.Write (sysid);
				w.Write (quoteChar);
			} else if (sysid != null) {
				w.Write (" SYSTEM ");
				w.Write (quoteChar);
				w.Write (sysid);
				w.Write (quoteChar);
			}

			if (subset != null) {
				w.Write ('[');
				w.Write (subset);
				w.Write (']');
			}
			
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

			w.Write (quoteChar);

			openAttribute = false;

			if (saveAttributeValue) {
				// add namespace
				namespaceManager.AddNamespace (
					savedAttributePrefix, savingAttributeValue);
				saveAttributeValue = false;
				savedAttributePrefix = String.Empty;
				savingAttributeValue = String.Empty;
			}
		}

		public override void WriteEndDocument ()
		{
			CloseOpenAttributeAndElements ();

			if (!hasRoot)
				throw new ArgumentException ("This document does not have a root element.");

			ws = WriteState.Start;
			hasRoot = false;
		}

		public override void WriteEndElement ()
		{
			WriteEndElementInternal (false);
		}

		private void WriteEndElementInternal (bool fullEndElement)
		{
			if (openElements.Count == 0)
				throw new InvalidOperationException("There was no XML start tag open.");

			if (openAttribute)
				WriteEndAttribute ();

			indentLevel--;
			CheckState ();
			AddMissingElementXmlns ();

			if (openStartElement) {
				if (openAttribute)
					WriteEndAttribute ();
				if (fullEndElement) {
					w.Write ('>');
					w.Write (indentFormatting);
					w.Write ("</");
					XmlTextWriterOpenElement el = (XmlTextWriterOpenElement) openElements.Peek ();
					if (el.Prefix != String.Empty) {
						w.Write (el.Prefix);
						w.Write (':');
					}
					w.Write (el.LocalName);
					w.Write ('>');
				} else
					w.Write (" />");

				openElements.Pop ();
				openStartElement = false;
			} else {
				w.Write (indentFormatting);
				w.Write ("</");
				XmlTextWriterOpenElement el = (XmlTextWriterOpenElement) openElements.Pop ();
				if (el.Prefix != String.Empty) {
					w.Write (el.Prefix);
					w.Write (':');
				}
				w.Write (el.LocalName);
				w.Write ('>');
			}

			namespaceManager.PopScope();
		}

		public override void WriteEntityRef (string name)
		{
			WriteRaw ("&");
			WriteStringInternal (name, true);
			WriteRaw (";");
		}

		public override void WriteFullEndElement ()
		{
			WriteEndElementInternal (true);
		}

		public override void WriteName (string name)
		{
			if (!XmlChar.IsName (name))
				throw new ArgumentException ("There is an invalid character: '" + name [0] +
							     "'", "name");
			w.Write (name);
		}

		public override void WriteNmToken (string name)
		{
			if (!XmlChar.IsNmToken (name))
				throw new ArgumentException ("There is an invalid character: '" + name [0] +
							     "'", "name");
			w.Write (name);
		}

		// LAMESPEC: It should reject such name that starts with "x" "m" "l" by XML specification, but
		// in fact it is used to write XmlDeclaration in WriteNode() (and it is inevitable since
		// WriteStartDocument() cannot specify encoding, while WriteNode() can write it).
		public override void WriteProcessingInstruction (string name, string text)
		{
			if ((name == null) || (name == string.Empty))
				throw new ArgumentException ();
			if (!XmlChar.IsName (name))
				throw new ArgumentException ("Invalid processing instruction name.");
			if ((text.IndexOf("?>") > 0))
				throw new ArgumentException ("Processing instruction cannot contain \"?>\" as its value.");

			CheckState ();
			CloseStartElement ();

			w.Write (indentFormatting);
			w.Write ("<?");
			w.Write (name);
			w.Write (' ');
			w.Write (text);
			w.Write ("?>");
		}

		public override void WriteQualifiedName (string localName, string ns)
		{
			if (localName == null || localName == String.Empty)
				throw new ArgumentException ();

			CheckState ();
			if (!openAttribute)
				CloseStartElement ();

			w.Write (namespaceManager.LookupPrefix (ns));
			w.Write (':');
			w.Write (localName);
		}

		public override void WriteRaw (string data)
		{
			WriteStringInternal (data, false);
		}

		public override void WriteRaw (char[] buffer, int index, int count)
		{
//			WriteRawInternal (new string (buffer, index, count));
			WriteStringInternal (new string (buffer, index, count), false);
		}

		public override void WriteStartAttribute (string prefix, string localName, string ns)
		{
			if ((prefix == "xml") && (localName == "lang")) {
				ns = XmlNamespaceManager.XmlnsXml;
				openXmlLang = true;
			}

			if ((prefix == "xml") && (localName == "space")) {
				ns = XmlNamespaceManager.XmlnsXml;
				openXmlSpace = true;
			}

			if ((prefix == "xmlns") && (localName.ToLower ().StartsWith ("xml")))
				throw new ArgumentException ("Prefixes beginning with \"xml\" (regardless of whether the characters are uppercase, lowercase, or some combination thereof) are reserved for use by XML: " + prefix + ":" + localName);

			if ((prefix == "xmlns") && (ns != XmlnsNamespace))
				throw new ArgumentException (String.Format ("The 'xmlns' attribute is bound to the reserved namespace '{0}'", XmlnsNamespace));

			CheckState ();

			if (ws == WriteState.Content)
				throw new InvalidOperationException ("Token StartAttribute in state " + WriteState + " would result in an invalid XML document.");

			if (prefix == null)
				prefix = String.Empty;

			if (ns == null)
				ns = String.Empty;

			string formatPrefix = "";
			string formatSpace = "";

			if (ns != String.Empty && prefix != "xmlns") 
			{
				string existingPrefix = namespaceManager.LookupPrefix (ns);

				if (existingPrefix == null || existingPrefix == "") 
				{
					bool createPrefix = false;
					if (prefix == "")
						createPrefix = true;
					else {
						string existingNs = namespaceManager.LookupNamespace (prefix);
						if (existingNs != null) {
							namespaceManager.RemoveNamespace (prefix, existingNs);
							if (namespaceManager.LookupNamespace (prefix) != existingNs) {
								createPrefix = true;
								namespaceManager.AddNamespace (prefix, existingNs);
							}
						}
					}
					if (createPrefix)
						prefix = "d" + indentLevel + "p" + (newAttributeNamespaces.Count + 1);
					
					// check if prefix exists. If yes - check if namespace is the same.
					if (newAttributeNamespaces [prefix] == null)
						newAttributeNamespaces.Add (prefix, ns);
					else if (!newAttributeNamespaces [prefix].Equals (ns))
						throw new ArgumentException ("Duplicate prefix with different namespace");
				}

				if (prefix == String.Empty && ns != XmlnsNamespace)
					prefix = (existingPrefix == null) ?
						String.Empty : existingPrefix;
			}

			if (prefix != String.Empty) 
			{
				formatPrefix = prefix + ":";
			}

			if (openStartElement || attributeWrittenForElement)
				formatSpace = " ";

			w.Write (formatSpace);
			w.Write (formatPrefix);
			w.Write (localName);
			w.Write ('=');
			w.Write (quoteChar);

			openAttribute = true;
			attributeWrittenForElement = true;
			ws = WriteState.Attribute;

			if (prefix == "xmlns" || prefix == String.Empty && localName == "xmlns") {
				saveAttributeValue = true;
				savedAttributePrefix = (prefix == "xmlns") ? localName : String.Empty;
				savingAttributeValue = String.Empty;
			}
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

			if (hasRoot)
				throw new XmlException ("WriteStartDocument called twice.");

			CheckState ();

			string encodingFormatting = "";

			if (!nullEncoding) 
				encodingFormatting = String.Format (" encoding={0}{1}{0}", quoteChar, w.Encoding.WebName);

			w.Write("<?xml version={0}1.0{0}{1}{2}?>", quoteChar, encodingFormatting, standaloneFormatting);
			ws = WriteState.Prolog;
		}

		public override void WriteStartElement (string prefix, string localName, string ns)
		{
			if (!Namespaces && (((prefix != null) && (prefix != String.Empty))
				|| ((ns != null) && (ns != String.Empty))))
				throw new ArgumentException ("Cannot set the namespace if Namespaces is 'false'.");
			if ((prefix != null && prefix != String.Empty) && ((ns == null) || (ns == String.Empty)))
				throw new ArgumentException ("Cannot use a prefix with an empty namespace.");

			// ignore non-namespaced node's prefix.
			if (ns == null)
				ns = String.Empty;
			if (ns == String.Empty)
				prefix = String.Empty;


			WriteStartElementInternal (prefix, localName, ns);
		}

		private void WriteStartElementInternal (string prefix, string localName, string ns)
		{
			hasRoot = true;
			CheckState ();
			CloseStartElement ();
			newAttributeNamespaces.Clear ();

			if (prefix == null)
				prefix = namespaceManager.LookupPrefix (ns);
			if (prefix == null)
				prefix = String.Empty;

			w.Write (indentFormatting);
			w.Write ('<');
			if (prefix != String.Empty) {
				w.Write (prefix);
				w.Write (':');
			}
			w.Write (localName);

			openElements.Push (new XmlTextWriterOpenElement (prefix, localName));
			ws = WriteState.Element;
			openStartElement = true;
			openElementNS = ns;
			openElementPrefix = prefix;

			namespaceManager.PushScope ();
			indentLevel++;
		}

		public override void WriteString (string text)
		{
			if (ws == WriteState.Prolog)
				throw new InvalidOperationException ("Token content in state Prolog would result in an invalid XML document.");

			WriteStringInternal (text, true);

			// MS.NET (1.0) saves attribute value only at WriteString.
			if (saveAttributeValue)
				// In most cases it will be called one time, so simply use string + string.
				savingAttributeValue += text;
		}

		private string NormalizeAttributeString (string value)
		{
			return value.Replace ("\r", "&#xD;").Replace ("\n", "&#xA;");
		}

		private void WriteStringInternal (string text, bool entitize)
		{
			if (text == null)
				text = String.Empty;

			if (text != String.Empty) {
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
						text = NormalizeAttributeString (text);
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

		public override void WriteSurrogateCharEntity (char lowChar, char highChar)
		{
			if (lowChar < '\uDC00' || lowChar > '\uDFFF' ||
				highChar < '\uD800' || highChar > '\uDBFF')
				throw new ArgumentException ("Invalid (low, high) pair of characters was specified.");

			CheckState ();

			if (!openAttribute) {
				IndentingOverriden = true;
				CloseStartElement ();
			}

			w.Write ("&#x");
			w.Write (((int) ((highChar - 0xD800) * 0x400 + (lowChar - 0xDC00) + 0x10000)).ToString ("x"));
			w.Write (';');
		}

		public override void WriteWhitespace (string ws)
		{
			if (!XmlChar.IsWhitespace (ws))
				throw new ArgumentException ("Invalid Whitespace");

			CheckState ();

			if (!openAttribute) {
				IndentingOverriden = true;
				CloseStartElement ();
			}

			w.Write (ws);
		}

		#endregion
	}
}
