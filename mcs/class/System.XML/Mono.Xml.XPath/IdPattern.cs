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

		string [] ids;
		
		public IdPattern (string arg0)
			: base ((NodeTest) null)
		{
			ids = arg0.Split (XmlChar.WhitespaceChars);
		}
		
		public override bool Matches (XPathNavigator node, XsltContext ctx)
		{
			XPathNavigator tmp = node.Clone ();
			for (int i = 0; i < ids.Length; i++)
				if (tmp.MoveToId (ids [i]) && tmp.IsSamePosition (node))
					return true;
			return false;
		}

		public override double DefaultPriority { get { return 0.5; } }
	}
}