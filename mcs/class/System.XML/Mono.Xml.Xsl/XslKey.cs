//
// XslKey.cs
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
using System.Collections.Specialized;
using System.Xml;
using System.Xml.Schema;
using System.Xml.XPath;
using System.Xml.Xsl;

using QName = System.Xml.XmlQualifiedName;

namespace Mono.Xml.Xsl {
	public class XslKey {
		XPathNavigator style;	// for late compilation
		QName name;
		XPathExpression usePattern;
		XPathExpression matchPattern;
		string use;
		string match;

		public XslKey (QName name, string use, string match, XPathNavigator style)
		{
			this.name = name;
			this.use = use;
			this.match = match;
			this.style = style;
		}

		public QName Name {
			get { return name; }
		}


		public XPathExpression UsePattern {
			get {
				if (usePattern == null)
					usePattern = style.Compile (use);
				return usePattern;
			}
		}


		public XPathExpression MatchPattern {
			get {
				if (matchPattern == null)
					matchPattern = style.Compile (match);
				return matchPattern;
			}
		}
	}
}