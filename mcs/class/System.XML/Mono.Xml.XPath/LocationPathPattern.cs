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
		
		LocationPathPattern patternPrevious;
		bool isAncestor;
		NodeTest nodeTest;
		ExprFilter filter;
		XPathNavigator previousNavigator;
		
		public LocationPathPattern (NodeTest nodeTest)
		{
			this.nodeTest = nodeTest;
		}
		
		public LocationPathPattern (ExprFilter filter)
		{
			this.filter = filter;
			
			while (! (filter.expr is NodeTest))
				filter = (ExprFilter)filter.expr;
			
			this.nodeTest = (NodeTest)filter.expr;
		}

		internal void SetPreviousPattern (Pattern prev, bool isAncestor)
		{
			LocationPathPattern toSet = LastPathPattern;
			toSet.patternPrevious = (LocationPathPattern)prev;
			toSet.isAncestor = isAncestor;
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
			
			if (nodeTest is NodeTypeTest) {
				// node () is different in xslt patterns
				if (((NodeTypeTest)nodeTest).type == XPathNodeType.All && 
					(node.NodeType == XPathNodeType.Root ||
					node.NodeType == XPathNodeType.Attribute)
				)
				return false;
			}
			
			if (filter == null && patternPrevious == null)
				return true;
			
			if (patternPrevious != null) {
				if (!isAncestor) {
					XPathNavigator parent = node.Clone ();
					parent.MoveToParent ();
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
			}

						
			if (filter == null)
				return true;

			// Optimization for non-positional predicate
			if (!filter.IsPositional && !(filter.expr is ExprFilter)) {
				return filter.pred.EvaluateBoolean (new NullIterator (node, ctx));
			}

			XPathNavigator p = null;
			if (previousNavigator == node) {
				p = previousNavigator;
				p.MoveTo (node);
			} else {
				p = node.Clone ();
				previousNavigator = p;
			}
			p.MoveToParent ();

			BaseIterator matches = filter.EvaluateNodeSet (new NullIterator (p, ctx));
			
			while (matches.MoveNext ()) {
				if (node.IsSamePosition (matches.Current))
					return true;
			}
			
			return false;
		}
		
		public override string ToString ()
		{
			string ret = "";
			if (patternPrevious != null) ret = patternPrevious.ToString () + (isAncestor ? "//" : "/");
			if (filter != null) ret += filter.ToString ();
			else ret += nodeTest.ToString ();
			
			return ret;
		}
		
		public LocationPathPattern LastPathPattern {
			get {
				LocationPathPattern ret = this;
				
				while (ret.patternPrevious != null)
					ret = ret.patternPrevious;
				
				return ret;
			}
		}
	}
}