//
// System.Xml.XPath.XPathNavigator
//
// Author:
//   Jason Diamond (jason@injektilo.org)
//
// (C) 2002 Jason Diamond  http://injektilo.org/
//

using System;
using Mono.Xml.XPath;

namespace System.Xml.XPath
{
	public abstract class XPathNavigator : ICloneable
	{
		#region Constructor

		protected XPathNavigator ()
		{
		}

		#endregion

		#region Properties

		public abstract string BaseURI { get; }

		public abstract bool HasAttributes { get; }

		public abstract bool HasChildren { get; }

		public abstract bool IsEmptyElement { get; }

		public abstract string LocalName { get; }

		public abstract string Name { get; }

		public abstract string NamespaceURI { get; }

		public abstract XmlNameTable NameTable { get; }

		public abstract XPathNodeType NodeType { get; }

		public abstract string Prefix { get; }

		public abstract string Value { get; }

		public abstract string XmlLang { get; }

		#endregion

		#region Methods

		public abstract XPathNavigator Clone ();

		[MonoTODO]
		public virtual XmlNodeOrder ComparePosition (XPathNavigator nav)
		{
			throw new NotImplementedException ();
		}

		public virtual XPathExpression Compile (string xpath)
		{
			Tokenizer tokenizer = new Tokenizer (xpath);
			XPathParser parser = new XPathParser ();
			Expression expr = (Expression) parser.yyparseSafe (tokenizer);
//			Expression expr = (Expression) parser.yyparseDebug (tokenizer);
			return new CompiledExpression (expr);
		}

		public virtual object Evaluate (string xpath)
		{
			return Evaluate (Compile (xpath));
		}

		public virtual object Evaluate (XPathExpression expr)
		{
			return Evaluate (expr, null);
		}

		public virtual object Evaluate (XPathExpression expr, XPathNodeIterator context)
		{
			CompiledExpression cexpr = (CompiledExpression) expr;
			if (context == null)
				context = new NullIterator (this, cexpr.NamespaceManager);
			BaseIterator iterContext = (BaseIterator) context;
			iterContext.NamespaceManager = cexpr.NamespaceManager;
			return cexpr.Evaluate (iterContext);
		}

		public abstract string GetAttribute (string localName, string namespaceURI);

		public abstract string GetNamespace (string name);
		
		object ICloneable.Clone ()
		{
			return Clone ();
		}

		public virtual bool IsDescendant (XPathNavigator nav)
		{
			if (nav != null)
			{
				nav = nav.Clone ();
				while (nav.MoveToParent ())
				{
					if (IsSamePosition (nav))
						return true;
				}
			}
			return false;
		}

		public abstract bool IsSamePosition (XPathNavigator other);

		public virtual bool Matches (string xpath)
		{
			return Matches (Compile (xpath));
		}

		public virtual bool Matches (XPathExpression expr)
		{
			XPathNodeIterator nodes = Select (expr);

			while (nodes.MoveNext ()) {
				if (IsSamePosition (nodes.Current))
					return true;
			}

			XPathNavigator navigator = Clone ();

			while (navigator.MoveToParent ()) {
				nodes = navigator.Select (expr);

				while (nodes.MoveNext ()) {
					if (IsSamePosition (nodes.Current))
						return true;
				}
			}

			return false;
		}

		public abstract bool MoveTo (XPathNavigator other);

		public abstract bool MoveToAttribute (string localName, string namespaceURI);

		public abstract bool MoveToFirst ();

		public abstract bool MoveToFirstAttribute ();

		public abstract bool MoveToFirstChild ();

		public bool MoveToFirstNamespace ()
		{
			return MoveToFirstNamespace (XPathNamespaceScope.All);
		}

		public abstract bool MoveToFirstNamespace (XPathNamespaceScope namespaceScope);

		public abstract bool MoveToId (string id);

		public abstract bool MoveToNamespace (string name);

		public abstract bool MoveToNext ();

		public abstract bool MoveToNextAttribute ();

		public bool MoveToNextNamespace ()
		{
			return MoveToNextNamespace (XPathNamespaceScope.All);
		}

		public abstract bool MoveToNextNamespace (XPathNamespaceScope namespaceScope);

		public abstract bool MoveToParent ();

		public abstract bool MoveToPrevious ();

		public abstract void MoveToRoot ();

		public virtual XPathNodeIterator Select (string xpath)
		{
			return Select (Compile (xpath));
		}

		public virtual XPathNodeIterator Select (XPathExpression expr)
		{
			CompiledExpression cexpr = (CompiledExpression) expr;
			BaseIterator iter = new NullIterator (this, cexpr.NamespaceManager);
			return cexpr.EvaluateNodeSet (iter);
		}

		public virtual XPathNodeIterator SelectAncestors (XPathNodeType type, bool matchSelf)
		{
			Axes axis = (matchSelf) ? Axes.AncestorOrSelf : Axes.Ancestor;
			NodeTest test = new NodeTypeTest (axis, type);
			return SelectTest (test);
		}

		[MonoTODO]
		public virtual XPathNodeIterator SelectAncestors (string name, string namespaceURI, bool matchSelf)
		{
			if (namespaceURI != null && namespaceURI != "")
				throw new NotImplementedException ();

			Axes axis = (matchSelf) ? Axes.AncestorOrSelf : Axes.Ancestor;
			XmlQualifiedName qname = new XmlQualifiedName (name);
			NodeTest test = new NodeNameTest (axis, qname);
			return SelectTest (test);
		}

		public virtual XPathNodeIterator SelectChildren (XPathNodeType type)
		{
			NodeTest test = new NodeTypeTest (Axes.Child, type);
			return SelectTest (test);
		}

		[MonoTODO]
		public virtual XPathNodeIterator SelectChildren (string name, string namespaceURI)
		{
			if (namespaceURI != null && namespaceURI != "")
				throw new NotImplementedException ();

			Axes axis = Axes.Child;
			XmlQualifiedName qname = new XmlQualifiedName (name);
			NodeTest test = new NodeNameTest (axis, qname);
			return SelectTest (test);
		}

		public virtual XPathNodeIterator SelectDescendants (XPathNodeType type, bool matchSelf)
		{
			Axes axis = (matchSelf) ? Axes.DescendantOrSelf : Axes.Descendant;
			NodeTest test = new NodeTypeTest (axis, type);
			return SelectTest (test);
		}

		[MonoTODO]
		public virtual XPathNodeIterator SelectDescendants (string name, string namespaceURI, bool matchSelf)
		{
			if (namespaceURI != null && namespaceURI != "")
				throw new NotImplementedException ();

			Axes axis = (matchSelf) ? Axes.DescendantOrSelf : Axes.Descendant;
			XmlQualifiedName qname = new XmlQualifiedName (name);
			NodeTest test = new NodeNameTest (axis, qname);
			return SelectTest (test);
		}

		internal XPathNodeIterator SelectTest (NodeTest test)
		{
			Expression expr = new ExprStep (test, null);
			BaseIterator iter = new NullIterator (this, null);
			return expr.EvaluateNodeSet (iter);
		}

		public override string ToString ()
		{
			return Value;
		}

		#endregion
	}
}
