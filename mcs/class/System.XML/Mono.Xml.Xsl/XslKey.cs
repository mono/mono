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

namespace Mono.Xml.Xsl
{
	internal class ExprKeyContainer : Expression
	{
		Expression expr;
		public ExprKeyContainer (Expression expr)
		{
			this.expr = expr;
		}

		public Expression BodyExpression {
			get { return expr; }
		}

		public override object Evaluate (BaseIterator iter)
		{
			return expr.Evaluate (iter);
		}

		internal override XPathNodeType EvaluatedNodeType {
			get { return expr.EvaluatedNodeType; }
		}

		internal override bool NeedAbsoluteMatching {
			// This must be evaluated at any point.
			get { return true; }
		}

		public override XPathResultType ReturnType {
			get { return expr.ReturnType; }
		}
	}

	public class XslKey
	{
		QName name;
		CompiledExpression usePattern;
		CompiledExpression matchPattern;
		Hashtable map;
		Hashtable mappedDocuments;

		public XslKey (Compiler c)
		{
			this.name = c.ParseQNameAttribute ("name");

			c.KeyCompilationMode = true;
			usePattern = c.CompileExpression (c.GetAttribute ("use"));
			if (usePattern == null)
				usePattern = c.CompileExpression (".");

			c.AssertAttribute ("match");
			string matchString = c.GetAttribute ("match");
			this.matchPattern = c.CompileExpression (matchString, true);
			c.KeyCompilationMode = false;
		}

		public QName Name { get { return name; }}
		internal CompiledExpression UsePattern { get { return usePattern; }}
		internal CompiledExpression MatchPattern { get { return matchPattern; }}

		internal void ClearKeyTable ()
		{
			if (map != null) {
				map.Clear ();
				map = null;
			}
			if (mappedDocuments != null) {
				mappedDocuments.Clear ();
				mappedDocuments = null;
			}
		}

		internal void CollectTable (XPathNavigator doc)
		{
			XPathNavigator nav = doc.Clone ();
			nav.MoveToRoot ();
			Expression expr = ((ExprKeyContainer) MatchPattern.ExpressionNode).BodyExpression;
			if (expr.NeedAbsoluteMatching)
				CollectAbsoluteMatchNodes (nav);
			else
				CollectRelativeMatchNodes (nav);
		}

		private void CollectAbsoluteMatchNodes (XPathNavigator nav)
		{
			XPathNodeIterator iter = nav.Select (MatchPattern);
			while (iter.MoveNext ())
				CollectIndex (iter.Current);
		}

		private void CollectRelativeMatchNodes (XPathNavigator nav)
		{
			do {
				if (nav.NodeType != XPathNodeType.Root)
					while (!nav.MoveToNext ())
						if (!nav.MoveToParent ())
							// finished
							return;
				do {
					do {
						if (nav.Matches (MatchPattern))
							CollectIndex (nav);
					} while (nav.MoveToFirstChild ());
				} while (nav.MoveToNext ());
			} while (nav.MoveToParent ());
		}

		private void CollectIndex (XPathNavigator nav)
		{
			XPathNavigator target = nav.Clone ();
			XPathNodeIterator iter;
			switch (UsePattern.ReturnType) {
			case XPathResultType.NodeSet:
				iter = nav.Select (UsePattern);
				while (iter.MoveNext ())
					AddIndex (iter.Current.Value, target);
				break;
			case XPathResultType.Any:
				object o = nav.Evaluate (UsePattern);
				iter = o as XPathNodeIterator;
				if (iter != null) {
					while (iter.MoveNext ())
						AddIndex (iter.Current.Value, target);
				}
				else
					AddIndex (nav.EvaluateString (UsePattern, null, null), target);
				break;
			default:
				string key = nav.EvaluateString (UsePattern, null, null);
				AddIndex (key, target);
				break;
			}
		}

		private void AddIndex (string key, XPathNavigator target)
		{
			ArrayList al = map [key] as ArrayList;
			if (al == null) {
				al = new ArrayList ();
				map [key] = al;
			}
			al.Add (target);
		}

		public bool Matches (XPathNavigator nav, XmlNamespaceManager nsmgr, string value)
		{
			if (map == null) {
				mappedDocuments = new Hashtable ();
				map = new Hashtable ();
			}
			if (mappedDocuments [nav.BaseURI] == null) {
				mappedDocuments.Add (nav.BaseURI, nav.BaseURI);
				MatchPattern.SetContext (nsmgr);
				UsePattern.SetContext (nsmgr);
				CollectTable (nav);
				MatchPattern.SetContext (null);
				UsePattern.SetContext (null);
			}
			
			ArrayList al = map [value] as ArrayList;
			if (al == null)
				return false;
			for (int i = 0; i < al.Count; i++)
				if (((XPathNavigator) al [i]).IsSamePosition (nav))
					return true;
			return false;
		}
	}
}