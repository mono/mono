//
// Mono.Xml.XPath.LocationPathPattern
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
	internal class LocationPathPattern : Pattern {
		
		internal Pattern patternPrevious;
		internal bool isAncestor;
		internal NodeTest nodeTest;
		ExprFilter filter;
		
		public LocationPathPattern (NodeTest nodeTest)
		{
			this.nodeTest = nodeTest;
		}
		
		public LocationPathPattern (ExprFilter filter) : this ((NodeTest)filter.expr)
		{
			this.filter = filter;
		}

		internal void SetPreviousPattern (Pattern prev, bool isAncestor)
		{
			this.patternPrevious = prev;
		}
		
		public override double DefaultPriority { 
			get { 
				if (patternPrevious == null && filter == null) {
					NodeNameTest t = nodeTest as NodeNameTest;
					if (t != null) {
						if (t.Name.Name == "*")
							return -.25;
						return 0;
					}

					return -.5;
				}
				return .5;
			}
		}
		
		public override bool Matches (XPathNavigator node, XsltContext ctx)
		{
			if (! nodeTest.Match (ctx, node))
				return false;
			
			if (filter == null && patternPrevious == null)
				return true;
			
			if (isAncestor) {
				XPathNavigator parent = node.Clone ();
				if (!patternPrevious.Matches (parent, ctx))
					return false;
			} else {
				XPathNavigator anc = node.Clone ();
				while (true) {
					if (!anc.MoveToParent ())
						return false;
					
					if (patternPrevious.Matches (anc, ctx))
						break;
				}
			}

						
			if (filter == null)
				return true;
			
			BaseIterator parentItr = new ParentIterator (node, ctx);
			BaseIterator matches = filter.EvaluateNodeSet (parentItr);
			
			while (matches.MoveNext ()) {
				if (node.IsSamePosition (matches.Current))
					return true;
			}
			
			return false;
		}
	}
}