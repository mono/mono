//
// System.Xml.XPath.XPathNavigator
//
// Author:
//   Jason Diamond (jason@injektilo.org)
//   Atsushi Enomoto (atsushi@ximian.com)
//
// (C) 2002 Jason Diamond  http://injektilo.org/
// (C) 2004 Novell Inc.
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
#if NET_2_0
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
#endif
using System.IO;
using System.Xml;
using System.Xml.Schema;
using Mono.Xml.XPath;

#if NET_2_0
using NSResolver = System.Xml.IXmlNamespaceResolver;
#else
using NSResolver = System.Xml.XmlNamespaceManager;
#endif

namespace System.Xml.XPath
{
#if NET_2_0
	public abstract class XPathNavigator : XPathItem,
		ICloneable, IXPathNavigable, IXmlNamespaceResolver
#else
	public abstract class XPathNavigator : ICloneable
#endif
	{
		class EnumerableIterator : XPathNodeIterator
		{
			IEnumerable source;
			IEnumerator e;
			int pos;

			public EnumerableIterator (IEnumerable source, int pos)
			{
				this.source = source;
				for (int i = 0; i < pos; i++)
					MoveNext ();
			}

			public override XPathNodeIterator Clone ()
			{
				return new EnumerableIterator (source, pos);
			}

			public override bool MoveNext ()
			{
				if (e == null)
					e = source.GetEnumerator ();
				if (!e.MoveNext ())
					return false;
				pos++;
				return true;
			}

			public override int CurrentPosition {
				get { return pos; }
			}

			public override XPathNavigator Current {
				get { return pos == 0 ? null : (XPathNavigator) e.Current; }
			}
		}

		#region Static members
#if NET_2_0
		public static IEqualityComparer NavigatorComparer {
			get { return XPathNavigatorComparer.Instance; }
		}
#endif
		#endregion

		#region Constructor

		protected XPathNavigator ()
		{
		}

		#endregion

		#region Properties

		public abstract string BaseURI { get; }

#if NET_2_0
		public virtual bool CanEdit {
			get { return false; }
		}

		public virtual bool HasAttributes {
			get {
				if (!MoveToFirstAttribute ())
					return false;
				MoveToParent ();
				return true;
			}
		}

		public virtual bool HasChildren {
			get {
				if (!MoveToFirstChild ())
					return false;
				MoveToParent ();
				return true;
			}
		}
#else
		public abstract bool HasAttributes { get; }

		public abstract bool HasChildren { get; }
#endif

		public abstract bool IsEmptyElement { get; }

		public abstract string LocalName { get; }

		public abstract string Name { get; }

		public abstract string NamespaceURI { get; }

		public abstract XmlNameTable NameTable { get; }

		public abstract XPathNodeType NodeType { get; }

		public abstract string Prefix { get; }

#if NET_2_0
		public virtual string XmlLang {
			get {
				XPathNavigator nav = Clone ();
				switch (nav.NodeType) {
				case XPathNodeType.Attribute:
				case XPathNodeType.Namespace:
					nav.MoveToParent ();
					break;
				}
				do {
					if (nav.MoveToAttribute ("lang", "http://www.w3.org/XML/1998/namespace"))
						return nav.Value;
				} while (nav.MoveToParent ());
				return String.Empty;
			}
		}
#else
		public abstract string Value { get; }

		public abstract string XmlLang { get; }
#endif

		#endregion

		#region Methods

		public abstract XPathNavigator Clone ();

		public virtual XmlNodeOrder ComparePosition (XPathNavigator nav)
		{
			if (IsSamePosition (nav))
				return XmlNodeOrder.Same;

			// quick check for direct descendant
			if (IsDescendant (nav))
				return XmlNodeOrder.Before;

			// quick check for direct ancestor
			if (nav.IsDescendant (this))
				return XmlNodeOrder.After;

			XPathNavigator nav1 = Clone ();
			XPathNavigator nav2 = nav.Clone ();

			// check if document instance is the same.
			nav1.MoveToRoot ();
			nav2.MoveToRoot ();
			if (!nav1.IsSamePosition (nav2))
				return XmlNodeOrder.Unknown;
			nav1.MoveTo (this);
			nav2.MoveTo (nav);

			int depth1 = 0;
			while (nav1.MoveToParent ())
				depth1++;
			nav1.MoveTo (this);
			int depth2 = 0;
			while (nav2.MoveToParent ())
				depth2++;
			nav2.MoveTo (nav);

			// find common parent depth
			int common = depth1;
			for (;common > depth2; common--)
				nav1.MoveToParent ();
			for (int i = depth2; i > common; i--)
				nav2.MoveToParent ();
			while (!nav1.IsSamePosition (nav2)) {
				nav1.MoveToParent ();
				nav2.MoveToParent ();
				common--;
			}

			// For each this and target, move to the node that is 
			// ancestor of the node and child of the common parent.
			nav1.MoveTo (this);
			for (int i = depth1; i > common + 1; i--)
				nav1.MoveToParent ();
			nav2.MoveTo (nav);
			for (int i = depth2; i > common + 1; i--)
				nav2.MoveToParent ();

			// Those children of common parent are comparable.
			// namespace nodes precede to attributes, and they
			// precede to other nodes.
			if (nav1.NodeType == XPathNodeType.Namespace) {
				if (nav2.NodeType != XPathNodeType.Namespace)
					return XmlNodeOrder.Before;
				while (nav1.MoveToNextNamespace ())
					if (nav1.IsSamePosition (nav2))
						return XmlNodeOrder.Before;
				return XmlNodeOrder.After;
			}
			if (nav2.NodeType == XPathNodeType.Namespace)
				return XmlNodeOrder.After;
			if (nav1.NodeType == XPathNodeType.Attribute) {
				if (nav2.NodeType != XPathNodeType.Attribute)
					return XmlNodeOrder.Before;
				while (nav1.MoveToNextAttribute ())
					if (nav1.IsSamePosition (nav2))
						return XmlNodeOrder.Before;
				return XmlNodeOrder.After;
			}
			while (nav1.MoveToNext ())
				if (nav1.IsSamePosition (nav2))
					return XmlNodeOrder.Before;
			return XmlNodeOrder.After;
		}

		public virtual XPathExpression Compile (string xpath)
		{
			return XPathExpression.Compile (xpath);
		}
		
		internal virtual XPathExpression Compile (string xpath, System.Xml.Xsl.IStaticXsltContext ctx)
		{
			return XPathExpression.Compile (xpath, null, ctx);
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
			return Evaluate (expr, context, null);
		}

		BaseIterator ToBaseIterator (XPathNodeIterator iter, NSResolver ctx)
		{
			BaseIterator i = iter as BaseIterator;
			if (i == null)
				i = new WrapperIterator (iter, ctx);
			return i;
		}

		object Evaluate (XPathExpression expr, XPathNodeIterator context, NSResolver ctx)
		{
			CompiledExpression cexpr = (CompiledExpression) expr;
			if (ctx == null)
				ctx = cexpr.NamespaceManager;
			
			if (context == null)
				context = new NullIterator (this, ctx);
			BaseIterator iterContext = ToBaseIterator (context, ctx);
			iterContext.NamespaceManager = ctx;
			return cexpr.Evaluate (iterContext);
		}

		internal XPathNodeIterator EvaluateNodeSet (XPathExpression expr, XPathNodeIterator context, NSResolver ctx)
		{
			CompiledExpression cexpr = (CompiledExpression) expr;
			if (ctx == null)
				ctx = cexpr.NamespaceManager;
			
			if (context == null)
				context = new NullIterator (this, cexpr.NamespaceManager);
			BaseIterator iterContext = ToBaseIterator (context, ctx);
			iterContext.NamespaceManager = ctx;
			return cexpr.EvaluateNodeSet (iterContext);
		}

		internal string EvaluateString (XPathExpression expr, XPathNodeIterator context, NSResolver ctx)
		{
			CompiledExpression cexpr = (CompiledExpression) expr;
			if (ctx == null)
				ctx = cexpr.NamespaceManager;
			
			if (context == null)
				context = new NullIterator (this, cexpr.NamespaceManager);
			BaseIterator iterContext = ToBaseIterator (context, ctx);
			return cexpr.EvaluateString (iterContext);
		}

		internal double EvaluateNumber (XPathExpression expr, XPathNodeIterator context, NSResolver ctx)
		{
			CompiledExpression cexpr = (CompiledExpression) expr;
			if (ctx == null)
				ctx = cexpr.NamespaceManager;
			
			if (context == null)
				context = new NullIterator (this, cexpr.NamespaceManager);
			BaseIterator iterContext = ToBaseIterator (context, ctx);
			iterContext.NamespaceManager = ctx;
			return cexpr.EvaluateNumber (iterContext);
		}

		internal bool EvaluateBoolean (XPathExpression expr, XPathNodeIterator context, NSResolver ctx)
		{
			CompiledExpression cexpr = (CompiledExpression) expr;
			if (ctx == null)
				ctx = cexpr.NamespaceManager;
			
			if (context == null)
				context = new NullIterator (this, cexpr.NamespaceManager);
			BaseIterator iterContext = ToBaseIterator (context, ctx);
			iterContext.NamespaceManager = ctx;
			return cexpr.EvaluateBoolean (iterContext);
		}

#if NET_2_0
		public virtual string GetAttribute (string localName, string namespaceURI)
		{
			if (!MoveToAttribute (localName, namespaceURI))
				return String.Empty;
			string value = Value;
			MoveToParent ();
			return value;
		}

		public virtual string GetNamespace (string name)
		{
			if (!MoveToNamespace (name))
				return String.Empty;
			string value = Value;
			MoveToParent ();
			return value;
		}

#else
		public abstract string GetAttribute (string localName, string namespaceURI);

		public abstract string GetNamespace (string name);
#endif
		
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
			Expression e = ((CompiledExpression) expr).ExpressionNode;
			if (e is ExprRoot)
				return NodeType == XPathNodeType.Root;
			
			NodeTest nt = e as NodeTest;
			if (nt != null) {
				switch (nt.Axis.Axis) {
				case Axes.Child:
				case Axes.Attribute:
					break;
				default:
					throw new XPathException ("Only child and attribute pattern are allowed for a pattern.");
				}
				return nt.Match (((CompiledExpression)expr).NamespaceManager, this);
			}
			if (e is ExprFilter) {
				do {
					e = ((ExprFilter) e).LeftHandSide;
				} while (e is ExprFilter);
				
				if (e is NodeTest && !((NodeTest) e).Match (((CompiledExpression) expr).NamespaceManager, this))
					return false;
			}

			XPathResultType resultType = e.ReturnType;
			switch (resultType) {
			case XPathResultType.Any:
			case XPathResultType.NodeSet:
				break;
			default:
				return false;
			}

			switch (e.EvaluatedNodeType) {
			case XPathNodeType.Attribute:
			case XPathNodeType.Namespace:
				if (NodeType != e.EvaluatedNodeType)
					return false;
				break;
			}

			XPathNodeIterator nodes;
			nodes = this.Select (expr);
			while (nodes.MoveNext ()) {
				if (IsSamePosition (nodes.Current))
					return true;
			}

			// ancestors might select this node.

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

#if NET_2_0
		public virtual bool MoveToAttribute (string localName, string namespaceURI)
		{
			if (MoveToFirstAttribute ()) {
				do {
					if (LocalName == localName && NamespaceURI == namespaceURI)
						return true;
				} while (MoveToNextAttribute ());
				MoveToParent ();
			}
			return false;
		}

		public virtual bool MoveToNamespace (string name)
		{
			if (MoveToFirstNamespace ()) {
				do {
					if (LocalName == name)
						return true;
				} while (MoveToNextNamespace ());
				MoveToParent ();
			}
			return false;
		}

		/*
		public virtual bool MoveToFirst ()
		{
			if (MoveToPrevious ()) {
				// It would be able to invoke MoveToPrevious() until the end, but this way would be much faster
				MoveToParent ();
				MoveToFirstChild ();
				return true;
			}
			return false;
		}
		*/

		public virtual bool MoveToFirst ()
		{
			return MoveToFirstImpl ();
		}

		public virtual void MoveToRoot ()
		{
			while (MoveToParent ())
				;
		}
#else
		public abstract bool MoveToAttribute (string localName, string namespaceURI);

		public abstract bool MoveToNamespace (string name);

		public abstract bool MoveToFirst ();

		public abstract void MoveToRoot ();
#endif

		internal bool MoveToFirstImpl ()
		{
			switch (NodeType) {
			case XPathNodeType.Attribute:
			case XPathNodeType.Namespace:
				return false;
			default:
				if (!MoveToParent ())
					return false;
				// Follow these 2 steps so that we can skip 
				// some types of nodes .
				MoveToFirstChild ();
				return true;
			}
		}

		public abstract bool MoveToFirstAttribute ();

		public abstract bool MoveToFirstChild ();

		public bool MoveToFirstNamespace ()
		{
			return MoveToFirstNamespace (XPathNamespaceScope.All);
		}

		public abstract bool MoveToFirstNamespace (XPathNamespaceScope namespaceScope);

		public abstract bool MoveToId (string id);

		public abstract bool MoveToNext ();

		public abstract bool MoveToNextAttribute ();

		public bool MoveToNextNamespace ()
		{
			return MoveToNextNamespace (XPathNamespaceScope.All);
		}

		public abstract bool MoveToNextNamespace (XPathNamespaceScope namespaceScope);

		public abstract bool MoveToParent ();

		public abstract bool MoveToPrevious ();

		public virtual XPathNodeIterator Select (string xpath)
		{
			return Select (Compile (xpath));
		}

		public virtual XPathNodeIterator Select (XPathExpression expr)
		{
			return Select (expr, null);
		}
		
		internal XPathNodeIterator Select (XPathExpression expr, NSResolver ctx)
		{
			CompiledExpression cexpr = (CompiledExpression) expr;
			if (ctx == null)
				ctx = cexpr.NamespaceManager;
			
			BaseIterator iter = new NullIterator (this, ctx);
			return cexpr.EvaluateNodeSet (iter);
		}

		public virtual XPathNodeIterator SelectAncestors (XPathNodeType type, bool matchSelf)
		{
			Axes axis = (matchSelf) ? Axes.AncestorOrSelf : Axes.Ancestor;
			return SelectTest (new NodeTypeTest (axis, type));
		}

		public virtual XPathNodeIterator SelectAncestors (string name, string namespaceURI, bool matchSelf)
		{
			if (name == null)
				throw new ArgumentNullException ("name");
			if (namespaceURI == null)
				throw new ArgumentNullException ("namespaceURI");

			Axes axis = (matchSelf) ? Axes.AncestorOrSelf : Axes.Ancestor;
			XmlQualifiedName qname = new XmlQualifiedName (name, namespaceURI);
			return SelectTest (new NodeNameTest (axis, qname, true));
		}

		static IEnumerable EnumerateChildren (XPathNavigator n, XPathNodeType type)
		{
			if (!n.MoveToFirstChild ())
				yield break;
			n.MoveToParent ();
			XPathNavigator nav = n.Clone ();
			nav.MoveToFirstChild ();
			XPathNavigator nav2 = null;
			do {
				if (type == XPathNodeType.All || nav.NodeType == type) {
					if (nav2 == null)
						nav2 = nav.Clone ();
					else
						nav2.MoveTo (nav);
					yield return nav2;
				}
			} while (nav.MoveToNext ());
		}

		public virtual XPathNodeIterator SelectChildren (XPathNodeType type)
		{
#if false
			return SelectTest (new NodeTypeTest (Axes.Child, type));
#else
			return new WrapperIterator (new EnumerableIterator (EnumerateChildren (this, type), 0), null);
			// FIXME: make it work i.e. remove dependency on BaseIterator
//			return new EnumerableIterator (EnumerateChildren (this, type), 0);
#endif
		}

		static IEnumerable EnumerateChildren (XPathNavigator n, string name, string ns)
		{
			if (!n.MoveToFirstChild ())
				yield break;
			n.MoveToParent ();
			XPathNavigator nav = n.Clone ();
			nav.MoveToFirstChild ();
			XPathNavigator nav2 = nav.Clone ();
			do {
				if ((name == String.Empty || nav.LocalName == name) && (ns == String.Empty || nav.NamespaceURI == ns)) {
					nav2.MoveTo (nav);
					yield return nav2;
				}
			} while (nav.MoveToNext ());
		}

		public virtual XPathNodeIterator SelectChildren (string name, string namespaceURI)
		{
			if (name == null)
				throw new ArgumentNullException ("name");
			if (namespaceURI == null)
				throw new ArgumentNullException ("namespaceURI");

#if false
			Axes axis = Axes.Child;
			XmlQualifiedName qname = new XmlQualifiedName (name, namespaceURI);
			return SelectTest (new NodeNameTest (axis, qname, true));
#else
			return new WrapperIterator (new EnumerableIterator (EnumerateChildren (this, name, namespaceURI), 0), null);
#endif
		}

		public virtual XPathNodeIterator SelectDescendants (XPathNodeType type, bool matchSelf)
		{
			Axes axis = (matchSelf) ? Axes.DescendantOrSelf : Axes.Descendant;
			return SelectTest (new NodeTypeTest (axis, type));
		}

		public virtual XPathNodeIterator SelectDescendants (string name, string namespaceURI, bool matchSelf)
		{
			if (name == null)
				throw new ArgumentNullException ("name");
			if (namespaceURI == null)
				throw new ArgumentNullException ("namespaceURI");


			Axes axis = (matchSelf) ? Axes.DescendantOrSelf : Axes.Descendant;
			XmlQualifiedName qname = new XmlQualifiedName (name, namespaceURI);
			return SelectTest (new NodeNameTest (axis, qname, true));
		}

		internal XPathNodeIterator SelectTest (NodeTest test)
		{
			return test.EvaluateNodeSet (new NullIterator (this));
		}

		public override string ToString ()
		{
			return Value;
		}

		#endregion

#if NET_2_0

		public virtual bool CheckValidity (XmlSchemaSet schemas, ValidationEventHandler validationEventHandler)
		{
			XmlReaderSettings settings = new XmlReaderSettings ();
			settings.NameTable = NameTable;
			settings.SetSchemas (schemas);
			settings.ValidationEventHandler += validationEventHandler;
			settings.ValidationType = ValidationType.Schema;
			try {
				XmlReader r = XmlReader.Create (
					ReadSubtree (), settings);
				while (!r.EOF)
					r.Read ();
			} catch (XmlSchemaValidationException) {
				return false;
			}
			return true;
		}

		public virtual XPathNavigator CreateNavigator ()
		{
			return Clone ();
		}

		public virtual object Evaluate (string xpath, IXmlNamespaceResolver resolver)
		{
			return Evaluate (Compile (xpath), null, resolver);
		}

		public virtual IDictionary<string, string> GetNamespacesInScope (XmlNamespaceScope scope)
		{
			IDictionary<string, string> table = new Dictionary<string, string> ();
			XPathNamespaceScope xpscope =
				scope == XmlNamespaceScope.Local ?
					XPathNamespaceScope.Local :
				scope == XmlNamespaceScope.ExcludeXml ?
					XPathNamespaceScope.ExcludeXml :
				XPathNamespaceScope.All;
			XPathNavigator nav = Clone ();
			if (nav.NodeType != XPathNodeType.Element)
				nav.MoveToParent ();
			if (!nav.MoveToFirstNamespace (xpscope))
				return table;
			do {
				table.Add (nav.Name, nav.Value);
			} while (nav.MoveToNextNamespace (xpscope));
			return table;
		}
#endif

#if NET_2_0
		public
#else
		internal
#endif
		virtual string LookupNamespace (string prefix)
		{
			XPathNavigator nav = Clone ();
			if (nav.NodeType != XPathNodeType.Element)
				nav.MoveToParent ();
			if (nav.MoveToNamespace (prefix))
				return nav.Value;
			return null;
		}

#if NET_2_0
		public
#else
		internal
#endif
		virtual string LookupPrefix (string namespaceURI)
		{
			XPathNavigator nav = Clone ();
			if (nav.NodeType != XPathNodeType.Element)
				nav.MoveToParent ();
			if (!nav.MoveToFirstNamespace ())
				return null;
			do {
				if (nav.Value == namespaceURI)
					return nav.Name;
			} while (nav.MoveToNextNamespace ());
			return null;
		}

		private bool MoveTo (XPathNodeIterator iter)
		{
			if (iter.MoveNext ()) {
				MoveTo (iter.Current);
				return true;
			}
			else
				return false;
		}

#if NET_2_0
		public
#else
		internal
#endif
		virtual bool MoveToChild (XPathNodeType type)
		{
			return MoveTo (SelectChildren (type));
		}

#if NET_2_0
		public
#else
		internal
#endif
		virtual bool MoveToChild (string localName, string namespaceURI)
		{
			return MoveTo (SelectChildren (localName, namespaceURI));
		}

#if NET_2_0
		public
#else
		internal
#endif
		virtual bool MoveToNext (string localName, string namespaceURI)
		{
			XPathNavigator nav = Clone ();
			while (nav.MoveToNext ()) {
				if (nav.LocalName == localName &&
					nav.NamespaceURI == namespaceURI) {
					MoveTo (nav);
					return true;
				}
			}
			return false;
		}

#if NET_2_0
		public
#else
		internal
#endif
		virtual bool MoveToNext (XPathNodeType type)
		{
			XPathNavigator nav = Clone ();
			while (nav.MoveToNext ()) {
				if (type == XPathNodeType.All || nav.NodeType == type) {
					MoveTo (nav);
					return true;
				}
			}
			return false;
		}

#if NET_2_0
		public
#else
		internal
#endif
		virtual bool MoveToFollowing (string localName,
			string namespaceURI)
		{
			return MoveToFollowing (localName, namespaceURI, null);
		}

#if NET_2_0
		public
#else
		internal
#endif
		virtual bool MoveToFollowing (string localName,
			string namespaceURI, XPathNavigator end)
		{
			if (localName == null)
				throw new ArgumentNullException ("localName");
			if (namespaceURI == null)
				throw new ArgumentNullException ("namespaceURI");
			localName = NameTable.Get (localName);
			if (localName == null)
				return false;
			namespaceURI = NameTable.Get (namespaceURI);
			if (namespaceURI == null)
				return false;

			XPathNavigator nav = Clone ();
			switch (nav.NodeType) {
			case XPathNodeType.Attribute:
			case XPathNodeType.Namespace:
				nav.MoveToParent ();
				break;
			}
			do {
				if (!nav.MoveToFirstChild ()) {
					do {
						if (!nav.MoveToNext ()) {
							if (!nav.MoveToParent ())
								return false;
						}
						else
							break;
					} while (true);
				}
				if (end != null && end.IsSamePosition (nav))
					return false;
				if (object.ReferenceEquals (localName, nav.LocalName) &&
					object.ReferenceEquals (namespaceURI, nav.NamespaceURI)) {
					MoveTo (nav);
					return true;
				}
			} while (true);
		}

#if NET_2_0
		public
#else
		internal
#endif
		virtual bool MoveToFollowing (XPathNodeType type)
		{
			return MoveToFollowing (type, null);
		}

#if NET_2_0
		public
#else
		internal
#endif
		virtual bool MoveToFollowing (XPathNodeType type,
			XPathNavigator end)
		{
			if (type == XPathNodeType.Root)
				return false; // will never match
			XPathNavigator nav = Clone ();
			switch (nav.NodeType) {
			case XPathNodeType.Attribute:
			case XPathNodeType.Namespace:
				nav.MoveToParent ();
				break;
			}
			do {
				if (!nav.MoveToFirstChild ()) {
					do {
						if (!nav.MoveToNext ()) {
							if (!nav.MoveToParent ())
								return false;
						}
						else
							break;
					} while (true);
				}
				if (end != null && end.IsSamePosition (nav))
					return false;
				if (type == XPathNodeType.All || nav.NodeType == type) {
					MoveTo (nav);
					return true;
				}
			} while (true);
		}

#if NET_2_0
		public virtual XmlReader ReadSubtree ()
		{
			switch (NodeType) {
			case XPathNodeType.Element:
			case XPathNodeType.Root:
				return new XPathNavigatorReader (this);
			default:
				throw new InvalidOperationException (String.Format ("NodeType {0} is not supported to read as a subtree of an XPathNavigator.", NodeType));
			}
		}

		public virtual XPathNodeIterator Select (string xpath, IXmlNamespaceResolver resolver)
		{
			return Select (Compile (xpath), resolver);
		}

		public virtual XPathNavigator SelectSingleNode (string xpath)
		{
			return SelectSingleNode (xpath, null);
		}

		public virtual XPathNavigator SelectSingleNode (string xpath, IXmlNamespaceResolver resolver)
		{
			XPathExpression expr = Compile (xpath);
			expr.SetContext (resolver);
			return SelectSingleNode (expr);
		}

		public virtual XPathNavigator SelectSingleNode (XPathExpression expression)
		{
			XPathNodeIterator iter = Select (expression);
			if (iter.MoveNext ())
				return iter.Current;
			else
				return null;
		}

		// it is not very effective code but should just work
		public override object ValueAs (Type returnType, IXmlNamespaceResolver nsResolver)
		{
			return new XmlAtomicValue (Value, XmlSchemaSimpleType.XsString).ValueAs (returnType, nsResolver);
		}

		public virtual void WriteSubtree (XmlWriter writer)
		{
			writer.WriteNode (this, false);
		}

		static readonly char [] escape_text_chars =
				new char [] {'&', '<', '>'};
		static readonly char [] escape_attr_chars =
				new char [] {'"', '&', '<', '>', '\r', '\n'};

		static string EscapeString (string value, bool attr)
		{
			StringBuilder sb = null;
			char [] escape = attr ? escape_attr_chars : escape_text_chars;
			if (value.IndexOfAny (escape) < 0)
				return value;
			sb = new StringBuilder (value, value.Length + 10);
			if (attr)
				sb.Replace ("\"", "&quot;");
			sb.Replace ("<", "&lt;");
			sb.Replace (">", "&gt;");
			if (attr) {
				sb.Replace ("\r\n", "&#10;");
				sb.Replace ("\r", "&#10;");
				sb.Replace ("\n", "&#10;");
			}
			return sb.ToString ();
		}

		public virtual string InnerXml {
			get {
				switch (NodeType) {
				case XPathNodeType.Element:
				case XPathNodeType.Root:
					break;
				case XPathNodeType.Attribute:
				case XPathNodeType.Namespace:
					return EscapeString (Value, true);
				case XPathNodeType.Text:
				case XPathNodeType.Whitespace:
				case XPathNodeType.SignificantWhitespace:
					return String.Empty;
				case XPathNodeType.ProcessingInstruction:
				case XPathNodeType.Comment:
					return Value;
				}

				XmlReader r = ReadSubtree ();
				r.Read (); // start
				// skip the element itself (or will reach to 
				// EOF if other than element) unless writing
				// doc itself
				int depth = r.Depth;
				if (NodeType != XPathNodeType.Root)
					r.Read ();
				else
					depth = -1; // for Root, it should consume the entire tree, so no depth check is done.
				StringWriter sw = new StringWriter ();
				XmlWriterSettings s = new XmlWriterSettings ();
				s.Indent = true;
				s.ConformanceLevel = ConformanceLevel.Fragment;
				s.OmitXmlDeclaration = true;
				XmlWriter xtw = XmlWriter.Create (sw, s);
				while (!r.EOF && r.Depth > depth)
					xtw.WriteNode (r, false);
				return sw.ToString ();
			}
			set {
				DeleteChildren ();
				if (NodeType == XPathNodeType.Attribute) {
					SetValue (value);
					return;
				}
				AppendChild (value);
			}
		}

		public override sealed bool IsNode {
			get { return true; }
		}

		public virtual string OuterXml {
			get {
				switch (NodeType) {
				case XPathNodeType.Attribute:
					return String.Concat (
						Prefix,
						Prefix.Length > 0 ? ":" : String.Empty,
						LocalName,
						"=\"",
						EscapeString (Value, true),
						"\"");
				case XPathNodeType.Namespace:
					return String.Concat (
						"xmlns",
						LocalName.Length > 0 ? ":" : String.Empty,
						LocalName,
						"=\"",
						EscapeString (Value, true),
						"\"");
				case XPathNodeType.Text:
					return EscapeString (Value, false);
				case XPathNodeType.Whitespace:
				case XPathNodeType.SignificantWhitespace:
					return Value;
				}

				XmlWriterSettings s = new XmlWriterSettings ();
				s.Indent = true;
				s.OmitXmlDeclaration = true;
				s.ConformanceLevel = ConformanceLevel.Fragment;
				StringBuilder sb = new StringBuilder ();
				using (XmlWriter w = XmlWriter.Create (sb, s)) {
					WriteSubtree (w);
				}
				return sb.ToString ();
			}
			set {
				switch (NodeType) {
				case XPathNodeType.Root:
				case XPathNodeType.Attribute:
				case XPathNodeType.Namespace:
					throw new XmlException ("Setting OuterXml Root, Attribute and Namespace is not supported.");
				}

				DeleteSelf ();
				AppendChild (value);
				MoveToFirstChild ();
			}
		}

		public virtual IXmlSchemaInfo SchemaInfo {
			get {
				return null;
			}
		}

		public override object TypedValue {
			get {
				switch (NodeType) {
				case XPathNodeType.Element:
				case XPathNodeType.Attribute:
					if (XmlType == null)
						break;
					XmlSchemaDatatype dt = XmlType.Datatype;
					if (dt == null)
						break;
					return dt.ParseValue (Value, NameTable, this as IXmlNamespaceResolver);
				}
				return Value;
			}
		}

		public virtual object UnderlyingObject {
			get { return null; }
		}

		public override bool ValueAsBoolean {
			get { return XQueryConvert.StringToBoolean (Value); }
		}

		public override DateTime ValueAsDateTime {
			get { return XmlConvert.ToDateTime (Value); }
		}

		public override double ValueAsDouble {
			get { return XQueryConvert.StringToDouble (Value); }
		}

		public override int ValueAsInt {
			get { return XQueryConvert.StringToInt (Value); }
		}

		public override long ValueAsLong {
			get { return XQueryConvert.StringToInteger (Value); }
		}

		public override Type ValueType {
			get {
				return SchemaInfo != null &&
					SchemaInfo.SchemaType != null &&
					SchemaInfo.SchemaType.Datatype != null ?
					SchemaInfo.SchemaType.Datatype.ValueType
					: null;
			}
		}

		public override XmlSchemaType XmlType {
			get {
				if (SchemaInfo != null)
					return SchemaInfo.SchemaType;
				return null;
			}
		}

		private XmlReader CreateFragmentReader (string fragment)
		{
			XmlReaderSettings settings = new XmlReaderSettings ();
			settings.ConformanceLevel = ConformanceLevel.Fragment;
			XmlNamespaceManager nsmgr = new XmlNamespaceManager (NameTable);
			foreach (KeyValuePair<string,string> nss in GetNamespacesInScope (XmlNamespaceScope.All))
				nsmgr.AddNamespace (nss.Key, nss.Value);
			return XmlReader.Create (
				new StringReader (fragment),
				settings,
				new XmlParserContext (NameTable, nsmgr, null, XmlSpace.None));
		}

		// must override it.
		public virtual XmlWriter AppendChild ()
		{
			throw new NotSupportedException ();
		}

		public virtual void AppendChild (
			string newChild)
		{
			AppendChild (CreateFragmentReader (newChild));
		}

		public virtual void AppendChild (
			XmlReader newChild)
		{
			XmlWriter w = AppendChild ();
			while (!newChild.EOF)
				w.WriteNode (newChild, false);
			w.Close ();
		}

		public virtual void AppendChild (
			XPathNavigator newChild)
		{
			AppendChild (new XPathNavigatorReader (newChild));
		}

		public virtual void AppendChildElement (string prefix, string localName, string namespaceURI, string value)
		{
			XmlWriter xw = AppendChild ();
			xw.WriteStartElement (prefix, localName, namespaceURI);
			xw.WriteString (value);
			xw.WriteEndElement ();
			xw.Close ();
		}

		public virtual void CreateAttribute (string prefix, string localName, string namespaceURI, string value)
		{
			using (XmlWriter w = CreateAttributes ()) {
				w.WriteAttributeString (prefix, localName, namespaceURI, value);
			}
		}

		// must override it.
		public virtual XmlWriter CreateAttributes ()
		{
			throw new NotSupportedException ();
		}

		// must override it.
		public virtual void DeleteSelf ()
		{
			throw new NotSupportedException ();
		}

		// must override it.
		public virtual void DeleteRange (XPathNavigator lastSiblingToDelete)
		{
			throw new NotSupportedException ();
		}

		public virtual XmlWriter ReplaceRange (XPathNavigator lastSiblingToReplace)
		{
			throw new NotSupportedException ();
		}
	
		public virtual XmlWriter InsertAfter ()
		{
			switch (NodeType) {
			case XPathNodeType.Root:
			case XPathNodeType.Attribute:
			case XPathNodeType.Namespace:
				throw new InvalidOperationException (String.Format ("Insertion after {0} is not allowed.", NodeType));
			}
			XPathNavigator nav = Clone ();
			if (nav.MoveToNext ())
				return nav.InsertBefore ();
			else if (nav.MoveToParent ())
				return nav.AppendChild ();
			else
				throw new InvalidOperationException ("Could not move to parent to insert sibling node");
		}

		public virtual void InsertAfter (string newSibling)
		{
			InsertAfter (CreateFragmentReader (newSibling));
		}

		public virtual void InsertAfter (XmlReader newSibling)
		{
			using (XmlWriter w = InsertAfter ()) {
				w.WriteNode (newSibling, false);
			}
		}

		public virtual void InsertAfter (XPathNavigator newSibling)
		{
			InsertAfter (new XPathNavigatorReader (newSibling));
		}

		public virtual XmlWriter InsertBefore ()
		{
			throw new NotSupportedException ();
		}

		public virtual void InsertBefore (string newSibling)
		{
			InsertBefore (CreateFragmentReader (newSibling));
		}

		public virtual void InsertBefore (XmlReader newSibling)
		{
			using (XmlWriter w = InsertBefore ()) {
				w.WriteNode (newSibling, false);
			}
		}

		public virtual void InsertBefore (XPathNavigator newSibling)
		{
			InsertBefore (new XPathNavigatorReader (newSibling));
		}

		public virtual void InsertElementAfter (string prefix, 
			string localName, string namespaceURI, string value)
		{
			using (XmlWriter w = InsertAfter ()) {
				w.WriteElementString (prefix, localName, namespaceURI, value);
			}
		}

		public virtual void InsertElementBefore (string prefix, 
			string localName, string namespaceURI, string value)
		{
			using (XmlWriter w = InsertBefore ()) {
				w.WriteElementString (prefix, localName, namespaceURI, value);
			}
		}

		public virtual XmlWriter PrependChild ()
		{
			XPathNavigator nav = Clone ();
			if (nav.MoveToFirstChild ())
				return nav.InsertBefore ();
			else
				return AppendChild ();
		}

		public virtual void PrependChild (string newChild)
		{
			PrependChild (CreateFragmentReader (newChild));
		}

		public virtual void PrependChild (XmlReader newChild)
		{
			using (XmlWriter w = PrependChild ()) {
				w.WriteNode (newChild, false);
			}
		}

		public virtual void PrependChild (XPathNavigator newChild)
		{
			PrependChild (new XPathNavigatorReader (newChild));
		}

		public virtual void PrependChildElement (string prefix, 
			string localName, string namespaceURI, string value)
		{
			using (XmlWriter w = PrependChild ()) {
				w.WriteElementString (prefix, localName, namespaceURI, value);
			}
		}

		public virtual void ReplaceSelf (string newNode)
		{
			ReplaceSelf (CreateFragmentReader (newNode));
		}

		// must override it.
		public virtual void ReplaceSelf (XmlReader newNode)
		{
			throw new NotSupportedException ();
		}

		public virtual void ReplaceSelf (XPathNavigator newNode)
		{
			ReplaceSelf (new XPathNavigatorReader (newNode));
		}

		// Dunno the exact purpose, but maybe internal editor use
		[MonoTODO]
		public virtual void SetTypedValue (object typedValue)
		{
			throw new NotSupportedException ();
		}

		public virtual void SetValue (string value)
		{
			throw new NotSupportedException ();
		}

		private void DeleteChildren ()
		{
			switch (NodeType) {
			case XPathNodeType.Namespace:
				throw new InvalidOperationException ("Removing namespace node content is not supported.");
			case XPathNodeType.Attribute:
				return;
			case XPathNodeType.Text:
			case XPathNodeType.SignificantWhitespace:
			case XPathNodeType.Whitespace:
			case XPathNodeType.ProcessingInstruction:
			case XPathNodeType.Comment:
				DeleteSelf ();
				return;
			}
			if (!HasChildren)
				return;
			XPathNavigator nav = Clone ();
			nav.MoveToFirstChild ();
			while (!nav.IsSamePosition (this))
				nav.DeleteSelf ();
		}
#endif
	}
}
