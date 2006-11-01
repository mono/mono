//
// Mono.Xml.XPath.Pattern
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

namespace Mono.Xml.XPath 
{
	internal abstract class Pattern 
	{
		internal static Pattern Compile (string s, Compiler comp)
		{		
			return Compile (comp.patternParser.Compile (s));
		}
		
		internal static Pattern Compile (Expression e)
		{		
			if (e is ExprUNION)
				return new UnionPattern (
					Compile (((ExprUNION)e).left),
					Compile (((ExprUNION)e).right)
				);
			
			if (e is ExprRoot)
				return new LocationPathPattern (
					new NodeTypeTest (Axes.Self, XPathNodeType.Root)
				);
			
			if (e is NodeTest)
				return new LocationPathPattern (
					(NodeTest)e
				);
			
			if (e is ExprFilter)
				return new LocationPathPattern (
					(ExprFilter)e
				);
			
			if (e is ExprSLASH)
			{
				Pattern p0 = Compile (((ExprSLASH)e).left);
				LocationPathPattern p1
					= (LocationPathPattern)Compile (((ExprSLASH)e).right);
				
				p1.SetPreviousPattern (p0, false);
				return p1;
			}
			
			if (e is ExprSLASH2)
			{
				if (((ExprSLASH2)e).left is ExprRoot)
					return Compile (((ExprSLASH2)e).right);
				
				Pattern p0 = Compile (((ExprSLASH2)e).left);
				LocationPathPattern p1
					= (LocationPathPattern)Compile (((ExprSLASH2)e).right);
				
				p1.SetPreviousPattern (p0, true);
				return p1;
			}
			
			if (e is XPathFunctionId)
			{
				ExprLiteral id = ((XPathFunctionId) e).Id as ExprLiteral;
				return new IdPattern (id.Value);
			}

			if (e is XsltKey)
			{
				return new KeyPattern ((XsltKey) e);
			}

			return null; // throw Exception outer this method.
		}
		
		public virtual double DefaultPriority { get { return 0.5; }}

		public virtual XPathNodeType EvaluatedNodeType {
			get { return XPathNodeType.All; }
		}

		public abstract bool Matches (XPathNavigator node, XsltContext ctx);
	}
}