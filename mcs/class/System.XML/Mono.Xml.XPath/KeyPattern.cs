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
using Mono.Xml.Xsl;

namespace Mono.Xml.XPath 
{
	internal class KeyPattern : LocationPathPattern 
	{
		XmlQualifiedName keyName;
		string arg0, arg1;
		XsltKey key;
		
		public KeyPattern (XsltKey key)
			: base ((NodeTest) null)
		{
			this.key = key;
			ExprLiteral keyName = key.KeyName as ExprLiteral;
			ExprLiteral field = key.Field as ExprLiteral;
			this.arg0 = keyName.Value;
			this.arg1 = field.Value;
			this.keyName = XslNameUtil.FromString (arg0, key.NamespaceManager);
		}
		
		public override bool Matches (XPathNavigator node, XsltContext ctx)
		{
			XsltCompiledContext xctx = ctx as XsltCompiledContext;
			XslKey xslkey = xctx.Processor.CompiledStyle.Keys [keyName] as XslKey;
			XPathNodeIterator iter = key.EvaluateNodeSet (new SelfIterator (node, ctx));
			while (iter.MoveNext ())
				if (iter.Current.IsSamePosition (node))
					return true;
			return false;
		}

		public override double DefaultPriority { get { return 0.5; } }
	}
}