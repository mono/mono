//
// Mono.Xml.XPath.LocationPathPattern
//
// Author:
//	Ben Maurer (bmaurer@users.sourceforge.net)
//
// (C) 2003 Ben Maurer
//

//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Collections;
using System.IO;
using System.Xml;
using System.Xml.Schema;
using System.Xml.XPath;
using System.Xml.Xsl;
using Mono.Xml.Xsl;

namespace Mono.Xml.XPath {
	internal class LocationPathPattern : Pattern {
		
		LocationPathPattern patternPrevious;
		bool isAncestor;
		NodeTest nodeTest;
		ExprFilter filter;
		
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
						if (t.Name.Name == "*" || t.Name.Name.Length == 0)
							return -.25;
						return 0;
					}

					return -.5;
				}
				return .5;
			}
		}

		public override XPathNodeType EvaluatedNodeType {
			get { return nodeTest.EvaluatedNodeType; }
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
			
			XPathNavigator tmpNav;
			if (patternPrevious != null) {
				tmpNav = ((XsltCompiledContext) ctx).GetNavCache (this, node);
				if (!isAncestor) {
					tmpNav.MoveToParent ();
					if (!patternPrevious.Matches (tmpNav, ctx))
						return false;
				} else {
					while (true) {
						if (!tmpNav.MoveToParent ())
							return false;
						
						if (patternPrevious.Matches (tmpNav, ctx))
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

			tmpNav = ((XsltCompiledContext) ctx).GetNavCache (this, node);
			tmpNav.MoveToParent ();

			BaseIterator matches = filter.EvaluateNodeSet (new NullIterator (tmpNav, ctx));
			
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