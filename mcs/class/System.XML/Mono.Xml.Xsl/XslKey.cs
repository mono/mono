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
using System.Collections.Specialized;
using System.Xml;
using System.Xml.Schema;
using System.Xml.XPath;
using System.Xml.Xsl;
using Mono.Xml.XPath;

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

		public override XPathResultType ReturnType {
			get { return expr.ReturnType; }
		}
	}

	internal class XslKey
	{
		QName name;
		CompiledExpression useExpr;
		Pattern matchPattern;

		public XslKey (Compiler c)
		{
			this.name = c.ParseQNameAttribute ("name");

			c.KeyCompilationMode = true;
			useExpr = c.CompileExpression (c.GetAttribute ("use"));
			if (useExpr == null)
				useExpr = c.CompileExpression (".");

			c.AssertAttribute ("match");
			string matchString = c.GetAttribute ("match");
			this.matchPattern = c.CompilePattern (matchString, c.Input);
			c.KeyCompilationMode = false;
		}

		public QName Name { get { return name; }}
		internal CompiledExpression Use { get { return useExpr; }}
		internal Pattern Match { get { return matchPattern; }}
	}

	// represents part of dynamic context that holds index table for a key
	internal class KeyIndexTable
	{
		XsltCompiledContext ctx;
		XslKey key;
		Hashtable map;
		Hashtable mappedDocuments;

		public KeyIndexTable (XsltCompiledContext ctx, XslKey key)
		{
			this.ctx = ctx;
			this.key = key;
		}

		public XslKey Key {
			get { return key; }
		}

		private void CollectTable (XPathNavigator doc, XsltContext ctx)
		{
			XPathNavigator nav = doc.Clone ();
			nav.MoveToRoot ();
			XPathNavigator tmp = doc.Clone ();

			do {
				if (key.Match.Matches (nav, ctx)) {
					tmp.MoveTo (nav);
					CollectIndex (nav, tmp);
				}
			} while (MoveNavigatorToNext (nav));
			if (map != null)
				foreach (ArrayList list in map.Values)
					list.Sort (XPathNavigatorComparer.Instance);
		}

		private bool MoveNavigatorToNext (XPathNavigator nav)
		{
			if (nav.MoveToFirstChild ())
				return true;
			do {
				if (nav.MoveToNext ())
					return true;
			} while (nav.MoveToParent ());
			return false;
		}

		private void CollectIndex (XPathNavigator nav, XPathNavigator target)
		{
			XPathNodeIterator iter;
			switch (key.Use.ReturnType) {
			case XPathResultType.NodeSet:
				iter = nav.Select (key.Use);
				while (iter.MoveNext ())
					AddIndex (iter.Current.Value, target);
				break;
			case XPathResultType.Any:
				object o = nav.Evaluate (key.Use);
				iter = o as XPathNodeIterator;
				if (iter != null) {
					while (iter.MoveNext ())
						AddIndex (iter.Current.Value, target);
				}
				else
					AddIndex (XPathFunctions.ToString (o), target);
				break;
			default:
				string keyValue = nav.EvaluateString (key.Use, null, null);
				AddIndex (keyValue, target);
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
			for (int i = 0; i < al.Count; i++)
				if (((XPathNavigator) al [i]).IsSamePosition (target))
					return;
			al.Add (target.Clone ());
		}

		private ArrayList GetNodesByValue (XPathNavigator nav, string value, XsltContext ctx)
		{
			if (map == null) {
				mappedDocuments = new Hashtable ();
				map = new Hashtable ();
			}
			if (!mappedDocuments.ContainsKey (nav.BaseURI)) {
				mappedDocuments.Add (nav.BaseURI, nav.BaseURI);
				CollectTable (nav, ctx);
			}
			
			return map [value] as ArrayList;
		}

		public bool Matches (XPathNavigator nav, string value, XsltContext ctx)
		{
			ArrayList al = GetNodesByValue (nav, value, ctx);
			if (al == null)
				return false;
			for (int i = 0; i < al.Count; i++)
				if (((XPathNavigator) al [i]).IsSamePosition (nav))
					return true;
			return false;
		}

		// Invoked from XsltKey (XPathFunction)
		public BaseIterator Evaluate (BaseIterator iter,
			Expression valueExpr)
		{
			XPathNodeIterator i = iter;
			if (iter.CurrentPosition == 0) {
				i = iter.Clone ();
				i.MoveNext ();
			}
			XPathNavigator nav = i.Current;

			object o = valueExpr.Evaluate (iter);
			XPathNodeIterator it = o as XPathNodeIterator;
			XsltContext ctx = iter.NamespaceManager as XsltContext;

			BaseIterator result = null;

			if (it != null) {
				while (it.MoveNext()) {
					ArrayList nodes = GetNodesByValue (
						it.Current, it.Current.Value, ctx);
					if (nodes == null)
						continue;
					ListIterator tmp =
						new ListIterator (nodes, ctx);
					if (result == null)
						result = tmp;
					else
						result = new UnionIterator (
							iter, result, tmp);
				}
			}
			else {
				ArrayList nodes = GetNodesByValue (
					nav, XPathFunctions.ToString (o), ctx);
				if (nodes != null)
					result = new ListIterator (nodes, ctx);
			}

			return result != null ? result : new NullIterator (iter);
		}
	}
}