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
	internal class IdPattern : Pattern {

		string arg0;
		
		public IdPattern (string arg0)
		{
			this.arg0 = arg0;
		}
		
		public override bool Matches (XPathNavigator node, XsltContext ctx)
		{
			throw new NotImplementedException ();
		}
	}
}