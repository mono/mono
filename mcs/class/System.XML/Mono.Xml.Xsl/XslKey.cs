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

		public override bool RequireSorting {
			get { return true; }
		}
	}

	internal class XslKey
	{
		QName name;
		CompiledExpression usePattern;
		CompiledExpression matchPattern;
//		Hashtable map;
//		Hashtable mappedDocuments;

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

		private void CollectTable (XPathNavigator doc)
		{
			XPathNavigator nav = doc.Clone ();
			nav.MoveToRoot ();
			XPathNavigator tmp = doc.Clone ();

			do {
				if (nav.Matches (key.MatchPattern)) {
					tmp.MoveTo (nav);
					CollectIndex (nav, tmp);
				}
			} while (MoveNavigatorToNext (nav));
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
			switch (key.UsePattern.ReturnType) {
			case XPathResultType.NodeSet:
				iter = nav.Select (key.UsePattern);
				while (iter.MoveNext ())
					AddIndex (iter.Current.Value, target);
				break;
			case XPathResultType.Any:
				object o = nav.Evaluate (key.UsePattern);
				iter = o as XPathNodeIterator;
				if (iter != null) {
					while (iter.MoveNext ())
						AddIndex (iter.Current.Value, target);
				}
				else
					AddIndex (nav.EvaluateString (key.UsePattern, null, null), target);
				break;
			default:
				string keyValue = nav.EvaluateString (key.UsePattern, null, null);
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

		public bool Matches (XPathNavigator nav, string value)
		{
			if (map == null) {
				mappedDocuments = new Hashtable ();
				map = new Hashtable ();
			}
			if (!mappedDocuments.ContainsKey (nav.BaseURI)) {
				mappedDocuments.Add (nav.BaseURI, nav.BaseURI);
				key.MatchPattern.SetContext (ctx);
				key.UsePattern.SetContext (ctx);
				CollectTable (nav);
				key.MatchPattern.SetContext (null);
				key.UsePattern.SetContext (null);
			}
			
			ArrayList al = map [value] as ArrayList;
			if (al == null)
				return false;
			for (int i = 0; i < al.Count; i++)
				if (((XPathNavigator) al [i]).IsSamePosition (nav))
					return true;
			return false;
		}

		// Invoked from XsltKey (XPathFunction)
		public object Evaluate (BaseIterator iter,
			Expression valueExpr)
		{
			ArrayList result = new ArrayList ();
			object o = valueExpr.Evaluate (iter);
			XPathNodeIterator it = o as XPathNodeIterator;
			
			if (it != null) {
				while (it.MoveNext())
					FindKeyMatch (it.Current.Value, result, iter.Current);
			} else {
				FindKeyMatch (XPathFunctions.ToString (o), result, iter.Current);
			}
			result.Sort (XPathNavigatorComparer.Instance);
			return new ListIterator (result, (ctx));
		}
		
		void FindKeyMatch (string value, ArrayList result, XPathNavigator context)
		{
			XPathNavigator searchDoc = context.Clone ();
			searchDoc.MoveToRoot ();
			if (key != null) {
				XPathNodeIterator desc = searchDoc.SelectDescendants (XPathNodeType.All, true);

				while (desc.MoveNext ()) {
					if (Matches (desc.Current, value))
						AddResult (result, desc.Current);
					
					if (!desc.Current.MoveToFirstAttribute ())
						continue;
					do {
						if (Matches (desc.Current, value))
							AddResult (result, desc.Current);	
					} while (desc.Current.MoveToNextAttribute ());
					
					desc.Current.MoveToParent ();
				}
			}
		}

		void AddResult (ArrayList result, XPathNavigator nav)
		{
			for (int i = 0; i < result.Count; i++) {
				XmlNodeOrder docOrder = nav.ComparePosition (((XPathNavigator)result [i]));
				if (docOrder == XmlNodeOrder.Same)
					return;
				
				if (docOrder == XmlNodeOrder.Before) {
					result.Insert(i, nav.Clone ());
					return;
				}
			}
			result.Add (nav.Clone ());
		}
	}
}