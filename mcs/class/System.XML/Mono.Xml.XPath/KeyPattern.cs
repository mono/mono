//
// Mono.Xml.XPath.KeyPattern
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
	internal class KeyPattern : Pattern {

		string arg0, arg1;
		
		public KeyPattern (string arg0, string arg1)
		{
			this.arg0 = arg0;
			this.arg1 = arg1;
		}
		
		public override bool Matches (XPathNavigator node, XsltContext ctx)
		{
			throw new NotImplementedException ();
		}
	}
}