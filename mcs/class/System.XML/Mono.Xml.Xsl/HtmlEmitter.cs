//
// HtmlEmitter.cs
//
// Author:
//	Atsushi Enomoto <ginga@kit.hi-ho.ne.jp>
//	
// (C)2003 Atsushi Enomoto
//
// TODO:
//	indent, uri escape, allowed entity char such as &nbsp;,
//	encoding to meta tag, doctype-public/doctype-system.
//

using System;
using System.Collections;
using System.IO;
using System.Text;
using System.Xml;

namespace Mono.Xml.Xsl
{
	internal class HtmlEmitter : Emitter
	{
		TextWriter writer;
		Stack elementNameStack;
		bool openElement;
		bool openAttribute;
		int nonHtmlDepth;
		bool indent;
		Encoding outputEncoding;

		public HtmlEmitter (TextWriter writer, XslOutput output)
		{
			this.writer = writer;
			indent = !(output.Indent == "no");
			elementNameStack = new Stack ();
			nonHtmlDepth = -1;
			outputEncoding = writer.Encoding == null ? output.Encoding : writer.Encoding;
		}

		public override void WriteStartDocument (Encoding encoding, StandaloneType standalone)
		{
			// do nothing
		}
		
		public override void WriteEndDocument ()
		{
			// do nothing
		}

		public override void WriteDocType (string name, string publicId, string systemId)
		{
			writer.Write ("<!DOCTYPE html ");
			if (publicId != null && publicId != String.Empty) {
				writer.Write ("PUBLIC \"");
				writer.Write (publicId);
				writer.Write ("\" ");
				if (systemId != null && systemId != String.Empty) {
					writer.Write ("\"");
 					writer.Write (systemId);
					writer.Write ("\"");
				}
			} else if (systemId != null && systemId != String.Empty) {
				writer.Write ("SYSTEM \"");
				writer.Write (systemId);
				writer.Write ('\"');
			}
			writer.Write ('>');
			if (indent)
				writer.WriteLine ();
		}

		private void CloseAttribute ()
		{
			writer.Write ('\"');
			openAttribute = false;
		}

		private void CloseStartElement ()
		{
			if (openAttribute)
				CloseAttribute ();
			writer.Write ('>');
			openElement = false;

			if (outputEncoding != null && elementNameStack.Count > 0) {
				string name = ((string) elementNameStack.Peek ()).ToUpper ();
				switch (name) {
				case "HEAD":
					WriteStartElement (String.Empty, "META", String.Empty);
					WriteAttributeString (String.Empty, "http-equiv", String.Empty, "Content-Type");
					WriteAttributeString (String.Empty, "content", String.Empty, "text/html; charset=" + outputEncoding.WebName);
					WriteEndElement ();
					break;
				case "STYLE":
				case "SCRIPT":
					writer.WriteLine ();
					for (int i = 0; i <= elementNameStack.Count; i++)
						writer.Write ("  ");
					break;
				}
			}
		}

		// FIXME: check all HTML elements' indentation.
		private void Indent (string elementName, bool endIndent)
		{
			if (!indent)
				return;
			switch (elementName.ToUpper ()) {
			case "ADDRESS":
			case "APPLET":
			case "BDO":
			case "BLOCKQUOTE":
			case "BODY":
			case "BUTTON":
			case "CAPTION":
			case "CENTER":
			case "DD":
			case "DEL":
			case "DIR":
			case "DIV":
			case "DL":
			case "DT":
			case "FIELDSET":
			case "HEAD":
			case "HTML":
			case "IFRAME":
			case "INS":
			case "LI":
			case "MAP":
			case "MENU":
			case "NOFRAMES":
			case "NOSCRIPT":
			case "OBJECT":
			case "OPTION":
			case "PRE":
			case "TABLE":
			case "TD":
			case "TH":
			case "TR":
				writer.Write (Environment.NewLine);
				int count = elementNameStack.Count;
				for (int i = 0; i < count; i++)
					writer.Write ("  ");
				break;
			default:
				if (elementName.Length > 0 && nonHtmlDepth > 0)
					goto case "HTML";
				break;
			}
		}

		public override void WriteStartElement (string prefix, string localName, string nsURI)
		{
			if (openElement)
				CloseStartElement ();
			Indent (elementNameStack.Count > 0 ? elementNameStack.Peek () as String : String.Empty, false);
			string formatName = localName;

			writer.Write ('<');
			if (nsURI != String.Empty || !IsHtmlElement (localName)) {
				// XML output
				if (prefix != String.Empty) {
					writer.Write (prefix);
					writer.Write (':');
					formatName = String.Concat (prefix, ":", localName);
				}
				
				if (nonHtmlDepth < 0)
					nonHtmlDepth = elementNameStack.Count + 1;
			}
			writer.Write (formatName);
			elementNameStack.Push (formatName);
			openElement = true;
		}

		private bool IsHtmlElement (string localName)
		{
			// see http://www.w3.org/TR/html401/index/elements.html
			switch (localName.ToUpper ()) {
			case "A": case "ABBR": case "ACRONYM":
			case "ADDRESS": case "APPLET": case "AREA":
			case "B": case "BASE": case "BASEFONT": case "BDO": case "BIG": 
			case "BLOCKQUOTE": case "BODY": case "BR": case "BUTTON":
			case "CAPTION": case "CENTER": case "CITE":
			case "CODE": case "COL": case "COLGROUP":
			case "DD": case "DEL": case "DFN": case "DIR":
			case "DIV": case "DL": case "DT":
			case "EM":
			case "FIELDSET": case "FONT": case "FORM": case "FRAME": case "FRAMESET":
			case "H1": case "H2": case "H3": case "H4": case "H5": case "H6":
			case "HEAD": case "HR": case "HTML":
			case "I": case "IFRAME": case "IMG":
			case "INPUT": case "INS": case "ISINDEX":
			case "KBD":
			case "LABEL": case "LEGEND": case "LI": case "LINK":
			case "MAP": case "MENU": case "META":
			case "NOFRAMES": case "NOSCRIPT":
			case "OBJECT": case "OL": case "OPTGROUP": case "OPTION":
			case "P": case "PARAM": case "PRE":
			case "Q":
			case "S": case "SAMP": case "SCRIPT": case "SELECT":
			case "SMALL": case "SPAN": case "STRIKE": case "STRONG":
			case "STYLE": case "SUB": case "SUP":
			case "TABLE": case "TBODY": case "TD": case "TEXTAREA":
			case "TFOOT": case "THEAD": case "TITLE": case "TR": case "TT":
			case "U": case "UL":
			case "VAR":
				return true;
			}
			return false;
		}

		public override void WriteEndElement ()
		{
			WriteFullEndElement ();
		}

		public override void WriteFullEndElement ()
		{
			string element = elementNameStack.Peek () as string;
			switch (element.ToUpper ()) {
			case "AREA":
			case "BASE":
			case "BASEFONT":
			case "BR":
			case "COL":
			case "FRAME":
			case "HR":
			case "IMG":
			case "INPUT":
			case "ISINDEX":
			case "LINK":
			case "META":
			case "PARAM":
				if (openAttribute)
					CloseAttribute ();
				writer.Write ('>');
				elementNameStack.Pop ();
				break;
			default:
				if (openElement)
					CloseStartElement ();
				elementNameStack.Pop ();
				if (IsHtmlElement (element))
					Indent (element as string, true);
				writer.Write ("</");
				writer.Write (element);
				writer.Write (">");
				break;
			}
			if (nonHtmlDepth > elementNameStack.Count)
				nonHtmlDepth = -1;
			openElement = false;
		}

		public override void WriteAttributeString (string prefix, string localName, string nsURI, string value)
		{
			if (nonHtmlDepth >= 0) {
				writer.Write (' ');
				writer.Write (localName);
				writer.Write ("=\"");
				openAttribute = true;
				WriteFormattedString (value);
				openAttribute = false;
				writer.Write ('\"');

				return;
			}

			string attribute = localName.ToUpper ();
			writer.Write (' ');
			writer.Write (localName);

			switch (attribute) {
			case "OPTION":
			case "CHECKED":
			case "SELECTED":
				return;
			}

			writer.Write ("=\"");
			openAttribute = true;
			WriteFormattedString (value);
			openAttribute = false;
			writer.Write ('\"');
		}

		public override void WriteComment (string text) {
			if (openElement)
				CloseStartElement ();
			writer.Write ("<!--");
			writer.Write (text);
			writer.Write ("-->");
		}

		public override void WriteProcessingInstruction (string name, string text)
		{
			if ((text.IndexOf("?>") > 0))
				throw new ArgumentException ("Processing instruction cannot contain \"?>\" as its value.");
			if (openElement)
				CloseStartElement ();

			if (elementNameStack.Count > 0)
				Indent (elementNameStack.Peek () as string, false);

			writer.Write ("<?");
			writer.Write (name);
			if (text != null && text != String.Empty) {
				writer.Write (' ');
				writer.Write (text);
			}

			if (nonHtmlDepth >= 0)
				writer.Write ("?>");
			else
				writer.Write (">"); // HTML PI ends with '>'
		}

		public override void WriteString (string text)
		{
			if (openElement)
				CloseStartElement ();
			WriteFormattedString (text);
		}

		private void WriteFormattedString (string text)
		{
			// style and script should not be escaped.
			if (!openAttribute) {
				string element = ((string) elementNameStack.Peek ()).ToUpper ();
				switch (element) {
				case "SCRIPT":
				case "STYLE":
					writer.Write (text);
					return;
				}
			}

			int start = 0;
			for (int i = 0; i < text.Length; i++) {
				switch (text [i]) {
				case '&':
					// '&' '{' should be "&{", not "&amp;{"
					if (nonHtmlDepth < 0 && i + 1 < text.Length && text [i + 1] == '{')
						continue;
					writer.Write (text.ToCharArray (), start, i - start);
					writer.Write ("&amp;");
					start = i + 1;
					break;
				case '<':
					if (openAttribute)
						continue;
					writer.Write (text.ToCharArray (), start, i - start);
					writer.Write ("&lt;");
					start = i + 1;
					break;
				case '>':
					if (openAttribute)
						continue;
					writer.Write (text.ToCharArray (), start, i - start);
					writer.Write ("&gt;");
					start = i + 1;
					break;
				case '\"':
					if (!openAttribute)
						continue;
					writer.Write (text.ToCharArray (), start, i - start);
					writer.Write ("&quot;");
					start = i + 1;
					break;
				}
			}
			if (text.Length > start)
				writer.Write (text.ToCharArray (), start, text.Length - start);
		}

		public override void WriteRaw (string data)
		{
			if (openElement)
				CloseStartElement ();
			writer.Write (data);
		}

		public override void WriteCDataSection (string text) {
			if (openElement)
				CloseStartElement ();
			writer.Write ("<![CDATA[");
			writer.Write (text);
			writer.Write ("]]>");
		}

		public override void WriteWhitespace (string value)
		{
			if (openElement)
				CloseStartElement ();
			writer.Write (value);
		}

		public override void Done ()
		{
			writer.Flush ();
		}
	}
}
