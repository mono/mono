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
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual object Evaluate (string xpath)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual object Evaluate (XPathExpression expr)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual object Evaluate (XPathExpression expr, XPathNodeIterator context)
		{
			throw new NotImplementedException ();
		}

		public abstract string GetAttribute (string localName, string namespaceURI);

		public abstract string GetNamespace (string name);
		
		[MonoTODO]
		object ICloneable.Clone ()
		{
			throw new NotImplementedException ();
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
			throw new NotImplementedException ();
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

		[MonoTODO]
		public bool MoveToFirstNamespace ()
		{
			throw new NotImplementedException ();
		}

		public abstract bool MoveToFirstNamespace (XPathNamespaceScope namespaceScope);

		public abstract bool MoveToId (string id);

		public abstract bool MoveToNamespace (string name);

		public abstract bool MoveToNext ();

		public abstract bool MoveToNextAttribute ();

		[MonoTODO]
		public bool MoveToNextNamespace ()
		{
			throw new NotImplementedException ();
		}

		public abstract bool MoveToNextNamespace (XPathNamespaceScope namespaceScope);

		public abstract bool MoveToParent ();

		public abstract bool MoveToPrevious ();

		public abstract void MoveToRoot ();

		[MonoTODO]
		public virtual XPathNodeIterator Select (string xpath)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual XPathNodeIterator Select (XPathExpression expr)
		{
			throw new NotImplementedException ();
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

		[MonoTODO]
		public override string ToString ()
		{
			throw new NotImplementedException ();
		}

		#endregion
	}
}
