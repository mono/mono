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
		QName name;
		XPathExpression usePattern;
		XPathExpression matchPattern;

		public XslKey (Compiler c)
		{
			this.name = c.ParseQNameAttribute ("name");
			
			usePattern = c.CompileExpression (c.GetAttribute ("use"));
			if (usePattern == null)
				usePattern = c.CompileExpression (".");

			c.AssertAttribute ("match");
			this.matchPattern = c.CompileExpression (c.GetAttribute ("match"));
		}

		public QName Name { get { return name; }}
		public XPathExpression UsePattern { get { return usePattern; }}
		public XPathExpression MatchPattern { get { return matchPattern; }}
		
		public bool Matches (XPathNavigator nav, string value)
		{
			
			if (!nav.Matches (MatchPattern)) 
				return false;
			Debug.WriteLine ("? " + nav.Name);
			switch (UsePattern.ReturnType)
			{
			case XPathResultType.NodeSet:
				XPathNodeIterator matches = nav.Select (UsePattern);
				while (matches.MoveNext ()) {
					if (matches.Current.Value == value)
						return true;
				}
				
				return false;
			case XPathResultType.Any:
				
				object o = nav.Evaluate (UsePattern);
				if (o is XPathNodeIterator) {
					XPathNodeIterator it = (XPathNodeIterator)o;
					while (it.MoveNext ())
						if (it.Current.Value == value)
							return true;
					return false;
				} else {
					return value == XPathFunctions.ToString (o);
				}
			default:
				return value == nav.EvaluateString (UsePattern, null);
			}
		}
	}
}