//
// System.Xml.XmlDeclaration
//
// Author:
//	Duncan Mak  (duncan@ximian.com)
//
// (C) Ximian, Inc.

using System;
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
				if (value.ToUpper() == "YES")
					standalone = "yes";
				if (value.ToUpper() == "NO")
					standalone = "no";
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

		public override void WriteContentTo (XmlWriter w)
		{
			// Nothing to do - no children.
		}

		[MonoTODO]
		public override void WriteTo (XmlWriter w)
		{
			if ((Standalone == String.Empty) || (Encoding == String.Empty))
				return;
		}

		void ParseInput (string input)
		{			
			Encoding = input.Split (new char [] { ' ' }) [1].Split (new char [] { '=' }) [1];
			Standalone = input.Split (new char [] { ' ' }) [2].Split (new char [] { '=' }) [1];
		}
	}
}
