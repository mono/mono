//
// Mono.Xml.XPath.Pattern
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
using Mono.Xml.Xsl;
using Mono.Xml.Xsl.Functions;

namespace Mono.Xml.XPath {
	public abstract class Pattern {
		
		internal static Pattern Compile (string s, Compiler comp)
		{		
			return Compile (comp.parser.Compile (s));
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
				if (((ExprSLASH2)e).right is ExprRoot)
					return Compile (((ExprSLASH2)e).left);
				
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

			throw new Exception ("Invalid Pattern");
		}
		
		public virtual double DefaultPriority { get { return 0.5; }}
		
		public abstract bool Matches (XPathNavigator node, XsltContext ctx);
	}
}