//
// System.Xml.XmlDeclaration
//
// Author:
//	Duncan Mak  (duncan@ximian.com)
//
// (C) Ximian, Inc.

using System;
using System.Xml;
using System.Text.RegularExpressions;

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
					if (value.ToUpper() == "YES")
						standalone = "yes";
					if (value.ToUpper() == "NO")
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
			// so writing out custom here.
			w.WriteRaw (String.Format ("<?xml {0}?>", Value));
		}

		void ParseInput (string input)
		{
			char [] sep = new char [] {'\'', '"'};
			if (!input.Trim ().StartsWith ("version"))
				throw new XmlException("missing \"version\".");
			int start = input.IndexOf ("encoding");
			int sstart = -1;
			if (start > 0) {
				int valStart = input.IndexOfAny (sep, start) + 1;
				int valEnd = input.IndexOfAny (sep, valStart);
				Encoding = input.Substring (valStart, valEnd - valStart);
				sstart = input.IndexOf ("standalone");
			}
			else
				sstart = input.IndexOf ("standalone");

			if (sstart > 0) {
				int svalStart = input.IndexOfAny (sep, sstart) + 1;
				int svalEnd = input.IndexOfAny (sep, svalStart);
				Standalone = input.Substring (svalStart, svalEnd - svalStart);
			}	// TODO: some error check
		}
	}
}
