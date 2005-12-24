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
		string mediaType;

		public HtmlEmitter (TextWriter writer, XslOutput output)
		{
			this.writer = writer;
			indent = output.Indent == "yes" || output.Indent == null;
			elementNameStack = new Stack ();
			nonHtmlDepth = -1;
			outputEncoding = writer.Encoding == null ? output.Encoding : writer.Encoding;
			mediaType = output.MediaType;
			if (mediaType == null || mediaType.Length == 0)
				mediaType = "text/html";
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
			if (publicId != null) {
				writer.Write ("PUBLIC \"");
				writer.Write (publicId);
				writer.Write ("\" ");
				if (systemId != null) {
					writer.Write ("\"");
 					writer.Write (systemId);
					writer.Write ("\"");
				}
			} else if (systemId != null) {
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
			//FIXME: consider sanity check if (openElement) return;
			if (openAttribute)
				CloseAttribute ();
			writer.Write ('>');
			openElement = false;

			if (outputEncoding != null && elementNameStack.Count > 0) {
				string name = ((string) elementNameStack.Peek ()).ToUpper (CultureInfo.InvariantCulture);
				switch (name) {
				case "HEAD":
					WriteStartElement (String.Empty, "META", String.Empty);
					WriteAttributeString (String.Empty, "http-equiv", String.Empty, "Content-Type");
					WriteAttributeString (String.Empty, "content", String.Empty, String.Concat (mediaType, "; charset=", outputEncoding.WebName));
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
			switch (elementName.ToUpper (CultureInfo.InvariantCulture)) {
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
				writer.Write (writer.NewLine);
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
			if (nsURI != String.Empty) {// && !IsHtmlElement (localName)) {
				// XML output
				if (prefix != String.Empty) {
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
			switch (localName.ToUpper (CultureInfo.InvariantCulture)) {
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
			case "TFOOT": case "TH": case "THEAD": case "TITLE":
			case "TR": case "TT": case "U": case "UL": case "VAR":
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
			switch (element.ToUpper (CultureInfo.InvariantCulture)) {
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
				if (openElement)
					writer.Write ('>'); //FIXME: consider using CloseStartElement() to write '>'
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
			writer.Write (' ');
			if (prefix != null && prefix.Length!=0)
			{
				writer.Write (prefix);
				writer.Write (":");
			}
			writer.Write (localName);

			if (nonHtmlDepth >= 0) 
			{
				writer.Write ("=\"");
				openAttribute = true;
				WriteFormattedString (value);
				openAttribute = false;
				writer.Write ('\"');

				return;
			}

			string attribute = localName.ToUpper (CultureInfo.InvariantCulture);
			string element = ((string) elementNameStack.Peek ()).ToLower (CultureInfo.InvariantCulture);

			if (attribute == "SELECTED" && element == "option"
				|| attribute == "CHECKED" && element == "input")
				return;

			writer.Write ("=\"");
			openAttribute = true;

			// URI attribute should be escaped.
			string attrName = null;
			string [] attrNames = null;
			switch (element) {
			case "q":
			case "blockquote":
			case "ins":
			case "del":
				attrName = "cite";
				break;
			case "form":
				attrName = "action";
				break;
			case "a":
			case "area":
			case "link":
			case "base":
				attrName = "href";
				break;
			case "head":
				attrName = "profile";
				break;
			case "input":
				attrNames = new string [] {"src", "usemap"};
				break;
			case "img":
				attrNames = new string [] {"src", "usemap", "longdesc"};
				break;
			case "object":
				attrNames = new string [] {"classid", "codebase", "data", "archive", "usemap"};
				break;
			case "script":
				attrNames = new string [] {"src", "for"};
				break;
			}
			if (attrNames != null) {
				string attr = localName.ToLower (CultureInfo.InvariantCulture);
				foreach (string a in attrNames) {
					if (a == attr) {
						value = HtmlUriEscape.EscapeUri (value);
						break;
					}
				}
			}
			else if (attrName != null && attrName == localName.ToLower (CultureInfo.InvariantCulture))
				value = HtmlUriEscape.EscapeUri (value);
			WriteFormattedString (value);
			openAttribute = false;
			writer.Write ('\"');
		}

		class HtmlUriEscape : Uri
		{
			private HtmlUriEscape () : base ("urn:foo") {}

			public static string EscapeUri (string input)
			{
				StringBuilder sb = new StringBuilder ();
				int start = 0;
				for (int i = 0; i < input.Length; i++) {
					char c = input [i];
					if (c < 32 || c > 127)
						continue;
					bool preserve = false;
					switch (c) {
					case '&':
					case '<':
					case '>':
					case '"':
					case '\'':
						preserve = true;
						break;
					default:
						preserve = HtmlUriEscape.IsExcludedCharacter (c);
						break;
					}
					if (preserve) {
						sb.Append (EscapeString (input.Substring (start, i - start)));
						sb.Append (c);
						start = i + 1;
					}
				}
				if (start < input.Length)
					sb.Append (EscapeString (input.Substring (start)));
				return sb.ToString ();
			}
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
			if (!openAttribute && elementNameStack.Count > 0) {
				string element = ((string) elementNameStack.Peek ()).ToUpper (CultureInfo.InvariantCulture);
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
//			writer.Write ("<![CDATA[");
			writer.Write (text);
//			writer.Write ("]]>");
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
