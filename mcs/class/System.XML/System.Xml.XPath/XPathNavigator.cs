//
// System.Xml.XPath.XPathNavigator
//
// Author:
//   Jason Diamond (jason@injektilo.org)
//
// (C) 2002 Jason Diamond  http://injektilo.org/
//

using System;

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

		[MonoTODO]
		public virtual XPathExpression Compile (string xpath)
		{
			Tokenizer tokenizer = new Tokenizer (xpath);
			XPathParser parser = new XPathParser ();
			Expression expr = (Expression) parser.yyparse (tokenizer);
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

		[MonoTODO]
		public virtual object Evaluate (XPathExpression expr, XPathNodeIterator context)
		{
			// TODO: check casts
			if (context == null)
				context = new SelfIterator (this, new DefaultContext ());
			return ((CompiledExpression) expr).Evaluate ((BaseIterator) context);
		}

		public abstract string GetAttribute (string localName, string namespaceURI);

		public abstract string GetNamespace (string name);
		
		object ICloneable.Clone ()
		{
			return Clone ();
		}

		[MonoTODO]
		public virtual bool IsDescendant (XPathNavigator nav)
		{
			throw new NotImplementedException ();
		}

		public abstract bool IsSamePosition (XPathNavigator other);

		[MonoTODO]
		public virtual bool Matches (string xpath)
		{
			return Matches (Compile (xpath));
		}

		[MonoTODO]
		public virtual bool Matches (XPathExpression expr)
		{
			throw new NotImplementedException ();
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

		[MonoTODO]
		public virtual XPathNodeIterator Select (XPathExpression expr)
		{
			BaseIterator iter = new SelfIterator (this, new DefaultContext ());
			return ((CompiledExpression) expr).EvaluateNodeSet (iter);
		}

		[MonoTODO]
		public virtual XPathNodeIterator SelectAncestors (XPathNodeType type, bool matchSelf)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual XPathNodeIterator SelectAncestors (string name, string namespaceURI, bool matchSelf)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual XPathNodeIterator SelectChildren (XPathNodeType type)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual XPathNodeIterator SelectChildren (string name, string namespaceURI)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual XPathNodeIterator SelectDescendants (XPathNodeType type, bool matchSelf)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual XPathNodeIterator SelectDescendants (string name, string namespaceURI, bool matchSelf)
		{
			throw new NotImplementedException ();
		}

		public override string ToString ()
		{
			return Value;
		}

		#endregion
	}
}
