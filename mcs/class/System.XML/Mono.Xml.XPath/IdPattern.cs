//
// Mono.Xml.XPath.IdPattern
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
	internal class IdPattern : LocationPathPattern {

		string arg0;
		
		public IdPattern (string arg0)
			: base ((NodeTest) null)
		{
			this.arg0 = arg0;
		}
		
		public override bool Matches (XPathNavigator node, XsltContext ctx)
		{
			XPathNavigator tmp = node.Clone ();
			tmp.MoveToId (arg0);
			return tmp.IsSamePosition (node);
		}

		public override double DefaultPriority { get { return 0.5; } }
	}
}