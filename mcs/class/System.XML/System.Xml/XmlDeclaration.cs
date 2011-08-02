//
// System.Xml.XmlDeclaration
//
// Author:
//	Duncan Mak  (duncan@ximian.com)
//	Atsushi Enomotot  (atsushi@ximian.com)
//
// (C) Ximian, Inc.

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
using System.Globalization;
using System.Text;
using System.Xml;

namespace System.Xml
{
	public class XmlDeclaration : XmlLinkedNode
	{
		string encoding = "UTF-8"; // defaults to UTF-8
		string standalone;
		string version;

		protected internal XmlDeclaration (string version, string encoding,
						   string standalone, XmlDocument doc)
			: base (doc)
		{
			if (encoding == null)
				encoding = "";

			if (standalone == null)
				standalone = "";

			this.version = version;
			this.encoding = encoding;
			this.standalone = standalone;
		}

		public string Encoding  {
			get { return encoding; } 
			set { encoding = (value == null) ? String.Empty : value; }
		}

		public override string InnerText {
			get { return Value; }
			set { ParseInput (value); }
		}
		
		public override string LocalName {
			get { return "xml"; }
		}

		public override string Name {
			get { return "xml"; }
		}

		public override XmlNodeType NodeType {
			get { return XmlNodeType.XmlDeclaration; }
		}

		public string Standalone {
			get { return standalone; }
			set {
				if(value != null)
				{
					if (String.Compare (value, "YES", true, CultureInfo.InvariantCulture) == 0)
						standalone = "yes";
					if (String.Compare (value, "NO", true, CultureInfo.InvariantCulture) == 0)
						standalone = "no";
				}
				else
					standalone = String.Empty;
			}
		}

		public override string Value {
			get {
				string formatEncoding = "";
				string formatStandalone = "";

				if (encoding != String.Empty)
					formatEncoding = String.Format (" encoding=\"{0}\"", encoding);

				if (standalone != String.Empty)
					formatStandalone = String.Format (" standalone=\"{0}\"", standalone);

				return String.Format ("version=\"{0}\"{1}{2}", Version, formatEncoding, formatStandalone);
			}
			set { ParseInput (value); }
		}

		public string Version {
			get { return version; }
		}

		public override XmlNode CloneNode (bool deep)
		{
			return new XmlDeclaration (Version, Encoding, standalone, OwnerDocument);
		}

		public override void WriteContentTo (XmlWriter w) {}

		public override void WriteTo (XmlWriter w)
		{
			// This doesn't seem to match up very well with w.WriteStartDocument()
			// so writing out PI here. (it used to be WriteRaw)
			w.WriteProcessingInstruction ("xml", Value);
		}

		private int SkipWhitespace (string input, int index)
		{
			while (index < input.Length) {
				if (XmlChar.IsWhitespace (input [index]))
					index++;
				else
					break;
			}
			return index;
		}

		void ParseInput (string input)
		{
			int index = SkipWhitespace (input, 0);
			if (index + 7 > input.Length || input.IndexOf ("version", index, 7) != index)
				throw new XmlException ("Missing 'version' specification.");
			index = SkipWhitespace (input, index + 7);

			char c = input [index];
			if (c != '=')
				throw new XmlException ("Invalid 'version' specification.");
			index++;
			index = SkipWhitespace (input, index);
			c = input [index];
			if (c != '"' && c != '\'')
				throw new XmlException ("Invalid 'version' specification.");
			index++;
			int end = input.IndexOf (c, index);
			if (end < 0 || input.IndexOf ("1.0", index, 3) != index)
				throw new XmlException ("Invalid 'version' specification.");
			index += 4;
			if (index == input.Length)
				return;
			if (!XmlChar.IsWhitespace (input [index]))
				throw new XmlException ("Invalid XML declaration.");
			index = SkipWhitespace (input, index + 1);
			if (index == input.Length)
				return;

			if (input.Length > index + 8 && input.IndexOf ("encoding", index, 8) > 0) {
				index = SkipWhitespace (input, index + 8);
				c = input [index];
				if (c != '=')
					throw new XmlException ("Invalid 'version' specification.");
				index++;
				index = SkipWhitespace (input, index);
				c = input [index];
				if (c != '"' && c != '\'')
					throw new XmlException ("Invalid 'encoding' specification.");
				end = input.IndexOf (c, index + 1);
				if (end < 0)
					throw new XmlException ("Invalid 'encoding' specification.");
				Encoding = input.Substring (index + 1, end - index - 1);
				index = end + 1;
				if (index == input.Length)
					return;
				if (!XmlChar.IsWhitespace (input [index]))
					throw new XmlException ("Invalid XML declaration.");
				index = SkipWhitespace (input, index + 1);
			}

			if (input.Length > index + 10 && input.IndexOf ("standalone", index, 10) > 0) {
				index = SkipWhitespace (input, index + 10);
				c = input [index];
				if (c != '=')
					throw new XmlException ("Invalid 'version' specification.");
				index++;
				index = SkipWhitespace (input, index);
				c = input [index];
				if (c != '"' && c != '\'')
					throw new XmlException ("Invalid 'standalone' specification.");
				end = input.IndexOf (c, index + 1);
				if (end < 0)
					throw new XmlException ("Invalid 'standalone' specification.");
				string tmp = input.Substring (index + 1, end - index - 1);
				switch (tmp) {
				case "yes":
				case "no":
					break;
				default:
					throw new XmlException ("Invalid standalone specification.");
				}
				Standalone = tmp;
				index = end + 1;
				index = SkipWhitespace (input, index);
			}
			if (index != input.Length)
				throw new XmlException ("Invalid XML declaration.");
		}
	}
}
