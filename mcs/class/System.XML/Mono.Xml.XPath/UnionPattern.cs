//
// Mono.Xml.XPath.UnionPattern
//
// Author:
//	Ben Maurer (bmaurer@users.sourceforge.net)
//
// (C) 2003 Ben Maurer
//

using System;
using System.Collections;
using System.IO;
using System.Xml;
using System.Xml.Schema;
using System.Xml.XPath;
using System.Xml.Xsl;

namespace Mono.Xml.XPath {
	internal class UnionPattern : Pattern {
		
		public readonly Pattern p0, p1;
		
		public UnionPattern (Pattern p0, Pattern p1)
		{
			this.p0 = p0;
			this.p1 = p1;
		}
		
		public override bool Matches (XPathNavigator node, XsltContext ctx)
		{
			return p0.Matches (node, ctx) || p1.Matches (node, ctx);
		}
		
		public override string ToString () {
			return p0.ToString () + " | " + p1.ToString ();
		}
	}
}