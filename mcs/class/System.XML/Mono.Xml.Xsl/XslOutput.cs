//
// XslOutput.cs
//
// Authors:
//	Ben Maurer (bmaurer@users.sourceforge.net)
//	Atsushi Enomoto (ginga@kit.hi-ho.ne.jp)
//	
// (C) 2003 Ben Maurer
// (C) 2003 Atsushi Enomoto
//

using System;
using System.CodeDom;
using System.Collections;
using System.Xml;
using System.Xml.Schema;
using System.Xml.XPath;
using System.Xml.Xsl;

namespace Mono.Xml.Xsl
{
	using QName = System.Xml.XmlQualifiedName;

	public class XslOutput	// also usable for xsl:result-document
	{
		string uri;
		QName method;
		string version;
		string encoding;
		bool omitXmlDeclaration;
		string standalone;
		string doctypePublic;
		string doctypeSystem;
		QName [] cdataSectionElements;
		bool indent;
		string mediaType;
		bool escapeUriAttributes;
		bool includeContentType;
		bool normalizeUnicode;
		bool undeclareNamespaces;
		QName [] useCharacterMaps;

		// for compilation only.
		ArrayList cdSectsList = new ArrayList ();

		public XslOutput (string uri)
		{
			this.uri = uri;
		}

		public QName Method {
			get { return method; }
		}

		public string Version {
			get { return version; }
		}

		public string Encoding {
			get { return encoding; }
		}

		public string Uri {
			get { return uri; }
		}

		public bool OmitXmlDeclaration {
			get { return omitXmlDeclaration; }
		}

		public string Standalone {
			get { return standalone; }
		}

		public string DoctypePublic {
			get { return doctypePublic; }
		}

		public string DoctypeSystem {
			get { return doctypeSystem; }
		}

		public QName [] CdataSectionElements {
			get {
				if (cdataSectionElements == null)
					cdataSectionElements = cdSectsList.ToArray (typeof (QName)) as QName [];
				return cdataSectionElements;
			}
		}

		public bool Indent {
			get { return indent; }
		}

		public string MediaType {
			get { return mediaType; }
		}

		// Below are introduced in XSLT 2.0 (WD-20030502)
		public bool EscapeUriAttributes {
			get { return escapeUriAttributes; }
		}

		public bool IncludeContentType {
			get { return includeContentType; }
		}

		public bool NormalizeUnicode {
			get { return normalizeUnicode; }
		}

		public bool UndeclareNamespaces {
			get { return undeclareNamespaces; }
		}

		public QName [] UseCharacterMaps {
			get { return useCharacterMaps; }
		}

		public void Fill (XPathNavigator nav)
		{
			string att;
			
			// cdata-section-elements
		// FILL IN
		//	att = nav.GetAttribute ("cdata-section-elements", "");
		//	if (att != null)
		//		cdSectsList.AddRange (XslNameUtil.ParseQNames (att, nav));

			att = nav.GetAttribute ("method", "");
			if (att != null)
				this.method = XslNameUtil.FromString (att, nav);

			att = nav.GetAttribute ("version", "");
			if (att != null)
				this.version = att;

			att = nav.GetAttribute ("encoding", "");
			if (att != null)
				this.encoding = att;

			att = nav.GetAttribute ("standalone", "");
			if (att != null)
				this.standalone = att;

			att = nav.GetAttribute ("doctype-public", "");
			if (att != null)
				this.doctypePublic = att;

			att = nav.GetAttribute ("doctype-system", "");
			if (att != null)
				this.doctypeSystem = att;

			att = nav.GetAttribute ("media-type", "");
			if (att != null)
				this.mediaType = att;

			att = nav.GetAttribute ("omit-xml-declaration", "");
			if (att != null)
				this.omitXmlDeclaration = att == "yes";

			att = nav.GetAttribute ("indent", "");
			if (att != null)
				this.indent = att == "yes";
		}
	}

}
