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
using System.Collections;
using System.Globalization;
using System.IO;
using System.Text;

namespace System.Xml
{
	[MonoTODO ("CheckCharacters; Conformance; NormalizeNewLines; OmitXmlDeclaration")]
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
		ArrayList openElements = new ArrayList ();
		int openElementCount;
		Formatting formatting = Formatting.None;
		int indentation = 2;
		char indentChar = ' ';
		string indentChars = "  ";
		char quoteChar = '\"';
		int indentLevel = 0;
		bool indentLocal;
		Stream baseStream = null;
		string xmlLang = null;
		XmlSpace xmlSpace = XmlSpace.None;
		bool openXmlLang = false;
		bool openXmlSpace = false;
		string openElementPrefix;
		string openElementNS;
		bool hasRoot = false;
		Hashtable newAttributeNamespaces = new Hashtable ();
		Hashtable userWrittenNamespaces = new Hashtable ();
		StringBuilder cachedStringBuilder;

		XmlNamespaceManager namespaceManager = new XmlNamespaceManager (new NameTable ());
		string savingAttributeValue = String.Empty;
		bool saveAttributeValue;
		string savedAttributePrefix;
		bool shouldAddSavedNsToManager;
		bool shouldCheckElementXmlns;

		// XmlWriterSettings support
		bool closeOutput;
		bool newLineOnAttributes;
		string newLineChars;

		#endregion

		#region Constructors

		public XmlTextWriter (TextWriter w) : base ()
		{
			this.w = w;
			nullEncoding = (w.Encoding == null);
			StreamWriter sw = w as StreamWriter;
			if (sw != null)
				baseStream = sw.BaseStream;
			newLineChars = w.NewLine;
		}

		public XmlTextWriter (Stream w,	Encoding encoding) : base ()
		{
			if (encoding == null) {
				nullEncoding = true;
				this.w = new StreamWriter (w);
			} else 
				this.w = new StreamWriter (w, encoding);
			baseStream = w;
			newLineChars = this.w.NewLine;
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
				if (openElementCount == 0)
					return false;
				else
					return ((XmlTextWriterOpenElement)
openElements [openElementCount - 1]).IndentingOverriden;
			}
			set {
				if (openElementCount > 0)
					((XmlTextWriterOpenElement) openElements [openElementCount - 1]).IndentingOverriden = value;
			}
		}

		private bool ParentIndentingOverriden {
			get {
				if (openElementCount < 2)
					return false;
				return ((XmlTextWriterOpenElement) openElements [openElementCount - 2]).IndentingOverriden;
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

#if NET_2_0
		internal bool CloseOutput {
//			get { return closeOutput; }
			set { closeOutput = value; }
		}

		internal string IndentChars {
//			get { return indentChars; }
			set { indentChars = value == null ? String.Empty : value; }
		}

		internal string NewLineChars {
//			get { return newLineChars; }
			set { newLineChars = value == null ? String.Empty : value; }
		}

		internal bool NewLineOnAttributes {
//			get { return newLineOnAttributes; }
			set { newLineOnAttributes = value; }
		}

#endif

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

				for (i = openElementCount - 1; i >= 0; i--) {
					xmlLang = ((XmlTextWriterOpenElement) openElements [i]).XmlLang;
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

				for (i = openElementCount - 1; i >= 0; i--) {
					xmlSpace = ((XmlTextWriterOpenElement)openElements [i]).XmlSpace;
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

			if (this.shouldCheckElementXmlns) {
				string formatXmlns = String.Empty;
				if (userWrittenNamespaces [prefix] == null) {
					if (prefix != string.Empty) {
						w.Write (" xmlns:");
						w.Write (prefix);
						w.Write ('=');
						w.Write (quoteChar);
						w.Write (EscapeString (ns, false));
						w.Write (quoteChar);
					}
					else {
						w.Write (" xmlns=");
						w.Write (quoteChar);
						w.Write (EscapeString (ns, false));
						w.Write (quoteChar);
					}
				}

				shouldCheckElementXmlns = false;
			}

			if (newAttributeNamespaces.Count > 0)
			{
				foreach (DictionaryEntry ent in newAttributeNamespaces)
				{
					string ans = (string) ent.Value;
					string aprefix = (string) ent.Key;

					if (namespaceManager.LookupNamespace (aprefix, false) == ans)
						continue;
	
					w.Write (" xmlns:");
					w.Write (aprefix);
					w.Write ('=');
					w.Write (quoteChar);
					w.Write (EscapeString (ans, false));
					w.Write (quoteChar);
				}
				newAttributeNamespaces.Clear ();
			}
		}

		private void CheckState ()
		{
			if (!openWriter) {
				throw new InvalidOperationException ("The Writer is closed.");
			}
			if ((documentStarted == true) && (formatting == Formatting.Indented) && (!IndentingOverriden)) {
				indentLocal = true;
			}
			else
				indentLocal = false;

			documentStarted = true;
		}

		public override void Close ()
		{
			CloseOpenAttributeAndElements ();

			if (closeOutput)
				w.Close();
			ws = WriteState.Closed;
			openWriter = false;
		}

		private void CloseOpenAttributeAndElements ()
		{
			if (openAttribute)
				WriteEndAttribute ();

			while (openElementCount > 0) {
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
			userWrittenNamespaces.Clear ();
		}

		public override void Flush ()
		{
			w.Flush ();
		}

		public override string LookupPrefix (string ns)
		{
			if (ns == null || ns == String.Empty)
				throw new ArgumentException ("The Namespace cannot be empty.");

			string prefix = namespaceManager.LookupPrefix (ns, false);

			// XmlNamespaceManager has changed to return null when NSURI not found.
			// (Contradiction to the ECMA documentation.)
			return prefix;
		}

		private void UpdateIndentChars ()
		{
			indentChars = new string (indentChar, indentation);
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

		public override void WriteBinHex (byte[] buffer, int index, int count)
		{
			CheckState ();

			if (!openAttribute) {
				IndentingOverriden = true;
				CloseStartElement ();
			}

			XmlConvert.WriteBinHex (buffer, index, count, w);
		}

		public override void WriteCData (string text)
		{
			if (text.IndexOf ("]]>") >= 0)
				throw new ArgumentException ();

			CheckState ();
			IndentingOverriden = true;
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
			if ((text.EndsWith("-")) || (text.IndexOf("--") > 0)) {
				throw new ArgumentException ();
			}
			if (ws != WriteState.Content && formatting == Formatting.Indented)
				w.WriteLine ();

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

			if (ws == WriteState.Prolog && formatting == Formatting.Indented)
				w.WriteLine ();

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
				((XmlTextWriterOpenElement) openElements [openElementCount - 1]).XmlLang = xmlLang;
			}

			if (openXmlSpace) 
			{
				if (xmlSpace == XmlSpace.Preserve)
					w.Write ("preserve");
				else if (xmlSpace == XmlSpace.Default)
					w.Write ("default");
				openXmlSpace = false;
				((XmlTextWriterOpenElement) openElements [openElementCount - 1]).XmlSpace = xmlSpace;
			}

			w.Write (quoteChar);

			openAttribute = false;

			if (saveAttributeValue) {
				if (savedAttributePrefix.Length > 0 && savingAttributeValue.Length == 0)
					throw new ArgumentException ("Cannot use prefix with an empty namespace.");

				// add namespace
				if (shouldAddSavedNsToManager) // not OLD one
					namespaceManager.AddNamespace (savedAttributePrefix, savingAttributeValue);
				userWrittenNamespaces [savedAttributePrefix] = savingAttributeValue;
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

		private void WriteIndent ()
		{
			if (!indentLocal)
				return;
			w.Write (newLineChars);
			for (int i = 0; i < indentLevel; i++)
				w.Write (indentChars);
		}

		private void WriteEndElementInternal (bool fullEndElement)
		{
			if (openElementCount == 0)
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
					if (!ParentIndentingOverriden)
						WriteIndent ();
					w.Write ("</");
					XmlTextWriterOpenElement el = (XmlTextWriterOpenElement) openElements [openElementCount - 1];
					if (el.Prefix != String.Empty) {
						w.Write (el.Prefix);
						w.Write (':');
					}
					w.Write (el.LocalName);
					w.Write ('>');
				} else
					w.Write (" />");

				openElementCount--;
				openStartElement = false;
			} else {
				WriteIndent ();
				w.Write ("</");
				XmlTextWriterOpenElement el = (XmlTextWriterOpenElement) openElements [openElementCount - 1];
				openElementCount--;
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
			WriteNameInternal (name);
		}

		public override void WriteNmToken (string name)
		{
			WriteNmTokenInternal (name);
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

			WriteIndent ();
			w.Write ("<?");
			w.Write (name);
			w.Write (' ');
			w.Write (text);
			w.Write ("?>");
		}

		public override void WriteQualifiedName (string localName, string ns)
		{
			CheckState ();
			if (!openAttribute)
				CloseStartElement ();

			WriteQualifiedNameInternal (localName, ns);
		}

		public override void WriteRaw (string data)
		{
			if (ws == WriteState.Start)
				ws = WriteState.Prolog;
			WriteStringInternal (data, false);
		}

		public override void WriteRaw (char[] buffer, int index, int count)
		{
			WriteRaw (new string (buffer, index, count));
		}

		public override void WriteStartAttribute (string prefix, string localName, string ns)
		{
			if (prefix == "xml") {
				// MS.NET looks to allow other names than 
				// lang and space (e.g. xml:link, xml:hack).
				ns = XmlNamespaceManager.XmlnsXml;
				if (localName == "lang")
					openXmlLang = true;
				else if (localName == "space")
					openXmlSpace = true;
			}
			if (prefix == null)
				prefix = String.Empty;

			if (prefix.Length > 0 && (ns == null || ns.Length == 0))
				if (prefix != "xmlns")
					throw new ArgumentException ("Cannot use prefix with an empty namespace.");

			if (prefix == "xmlns") {
				if (localName == null || localName.Length == 0) {
					localName = prefix;
					prefix = String.Empty;
				}
				else if (localName.ToLower (CultureInfo.InvariantCulture).StartsWith ("xml"))
					throw new ArgumentException ("Prefixes beginning with \"xml\" (regardless of whether the characters are uppercase, lowercase, or some combination thereof) are reserved for use by XML: " + prefix + ":" + localName);
			}

			// Note that null namespace with "xmlns" are allowed.
#if NET_1_0
			if ((prefix == "xmlns" || localName == "xmlns" && prefix == String.Empty) && ns != XmlnsNamespace)
#else
			if ((prefix == "xmlns" || localName == "xmlns" && prefix == String.Empty) && ns != null && ns != XmlnsNamespace)
#endif
				throw new ArgumentException (String.Format ("The 'xmlns' attribute is bound to the reserved namespace '{0}'", XmlnsNamespace));

			CheckState ();

			if (ws == WriteState.Content)
				throw new InvalidOperationException ("Token StartAttribute in state " + WriteState + " would result in an invalid XML document.");

			if (prefix == null)
				prefix = String.Empty;

			if (ns == null)
				ns = String.Empty;

			string formatPrefix = "";

			if (ns != String.Empty && prefix != "xmlns") {
				string existingPrefix = namespaceManager.LookupPrefix (ns, false);

				if (existingPrefix == null || existingPrefix == "") {
					bool createPrefix = false;
					if (prefix == "")
						createPrefix = true;
					else {
						string existingNs = namespaceManager.LookupNamespace (prefix, false);
						if (existingNs != null) {
							namespaceManager.RemoveNamespace (prefix, existingNs);
							if (namespaceManager.LookupNamespace (prefix, false) != existingNs) {
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

			if (openStartElement || attributeWrittenForElement) {
				if (newLineOnAttributes)
					WriteIndent ();
				else
					w.Write (" ");
			}

			w.Write (formatPrefix);
			w.Write (localName);
			w.Write ('=');
			w.Write (quoteChar);

			openAttribute = true;
			attributeWrittenForElement = true;
			ws = WriteState.Attribute;

			if (prefix == "xmlns" || prefix == String.Empty && localName == "xmlns") {
				if (prefix != openElementPrefix || openElementNS == null)
					shouldAddSavedNsToManager = true; 
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
			if (ns == null || ns == String.Empty)
				prefix = String.Empty;


			WriteStartElementInternal (prefix, localName, ns);
		}

		private void WriteStartElementInternal (string prefix, string localName, string ns)
		{
			hasRoot = true;
			CheckState ();
			CloseStartElement ();
			newAttributeNamespaces.Clear ();
			userWrittenNamespaces.Clear ();
			shouldCheckElementXmlns = false;

			if (prefix == null && ns != null)
				prefix = namespaceManager.LookupPrefix (ns, false);
			if (prefix == null)
				prefix = String.Empty;

			WriteIndent ();
			w.Write ('<');
			if (prefix != String.Empty) {
				w.Write (prefix);
				w.Write (':');
			}
			w.Write (localName);

			if (openElements.Count == openElementCount)
				openElements.Add (new XmlTextWriterOpenElement (prefix, localName));
			else
				((XmlTextWriterOpenElement) openElements [openElementCount]).Reset (prefix, localName);
			openElementCount++;
			ws = WriteState.Element;
			openStartElement = true;
			openElementNS = ns;
			openElementPrefix = prefix;

			namespaceManager.PushScope ();
			indentLevel++;

			if (ns != null) {
				if (ns.Length > 0) {
					string existing = LookupPrefix (ns);
					if (existing != prefix) {
						shouldCheckElementXmlns = true;
						namespaceManager.AddNamespace (prefix, ns);
					}
				} else {
					if (ns != namespaceManager.DefaultNamespace) {
						shouldCheckElementXmlns = true;
						namespaceManager.AddNamespace ("", ns);
					}
				}
			}
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

		string [] replacements = new string [] {
			"&amp;", "&lt;", "&gt;", "&quot;", "&apos;",
			"&#xD;", "&#xA;"};

		private string EscapeString (string source, bool skipQuotations)
		{
			int start = 0;
			int pos = 0;
			int count = source.Length;
			char invalid = ' ';
			for (int i = 0; i < count; i++) {
				switch (source [i]) {
				case '&':  pos = 0; break;
				case '<':  pos = 1; break;
				case '>':  pos = 2; break;
				case '\"':
					if (skipQuotations) continue;
					if (QuoteChar == '\'') continue;
					pos = 3; break;
				case '\'':
					if (skipQuotations) continue;
					if (QuoteChar == '\"') continue;
					pos = 4; break;
				case '\r':
					if (skipQuotations) continue;
					pos = 5; break;
				case '\n':
					if (skipQuotations) continue;
					pos = 6; break;
				default:
					if (XmlChar.IsInvalid (source [i])) {
						invalid = source [i];
						pos = -1;
						break;
					}
					else
						continue;
				}
				if (cachedStringBuilder == null)
					cachedStringBuilder = new StringBuilder ();
				cachedStringBuilder.Append (source.Substring (start, i - start));
				if (pos < 0) {
					cachedStringBuilder.Append ("&#x");
					if (invalid < (char) 255)
						cachedStringBuilder.Append (((int) invalid).ToString ("X02", CultureInfo.InvariantCulture));
					else
						cachedStringBuilder.Append (((int) invalid).ToString ("X04", CultureInfo.InvariantCulture));
					cachedStringBuilder.Append (";");
				}
				else
					cachedStringBuilder.Append (replacements [pos]);
				start = i + 1;
			}
			if (start == 0)
				return source;
			else if (start < count)
				cachedStringBuilder.Append (source.Substring (start, count - start));
			string s = cachedStringBuilder.ToString ();
			cachedStringBuilder.Length = 0;
			return s;
		}

		private void WriteStringInternal (string text, bool entitize)
		{
			if (text == null)
				text = String.Empty;

			if (text != String.Empty) {
				CheckState ();

				if (entitize)
					text = EscapeString (text, !openAttribute);

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
			w.Write (((int) ((highChar - 0xD800) * 0x400 + (lowChar - 0xDC00) + 0x10000)).ToString ("X", CultureInfo.InvariantCulture));
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
