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
		string standAlone;
		string version;

		protected internal XmlDeclaration (string version, string encoding,
						   string standAlone, XmlDocument doc)
			: base (doc)
		{
			this.version = version;
			this.encoding = encoding;
			this.standAlone = standAlone;
		}

		public string Encoding  {
			get {
				if (encoding == null)
					return String.Empty;
				else
					return encoding;
			} 

			set { encoding = value ; } // Note: MS' doesn't check this string, should we?
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
			get {
				if (standAlone == null)
					return String.Empty;
				else
					return standAlone;
			}

			set {
				if (value.ToUpper() == "YES")
					standAlone = "yes";
				if (value.ToUpper() == "NO")
					standAlone = "no";
			}
		}

		public override string Value {
			get { return String.Format ("version=\"{0}\" encoding=\"{1}\" standalone=\"{2}\"",
						    Version, Encoding, Standalone); }
			set { ParseInput (value); }
		}

		public string Version {
			get { return version; }
		}

		public override XmlNode CloneNode (bool deep)
		{
			return new XmlDeclaration (Encoding, standAlone, OwnerDocument);
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
