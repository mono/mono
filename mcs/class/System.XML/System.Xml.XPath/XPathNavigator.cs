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
using System.Xml;
using System.Xml.Schema;
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

		int Depth
		{
			get
			{
				int cLevels = 0;
				XPathNavigator nav = Clone ();
				while (nav.MoveToParent ())
					cLevels ++;
				return cLevels;
			}
		}

		#endregion

		#region Methods

		public abstract XPathNavigator Clone ();

		public virtual XmlNodeOrder ComparePosition (XPathNavigator nav)
		{
			if (IsSamePosition (nav))
				return XmlNodeOrder.Same;

			XPathNavigator nav1 = Clone ();
			XPathNavigator nav2 = nav.Clone ();

			int nDepth1 = nav1.Depth;
			int nDepth2 = nav2.Depth;

			if (nDepth1 > nDepth2)
			{
				while (nDepth1 > nDepth2)
				{
					if (!nav1.MoveToParent ())
						break;
					nDepth1 --;
				}
				if (nav1.IsSamePosition (nav2))
					return XmlNodeOrder.After;
			}
			else if (nDepth1 < nDepth2)
			{
				while (nDepth1 < nDepth2)
				{
					if (!nav2.MoveToParent ())
						break;
					nDepth2 --;
				}
				if (nav1.IsSamePosition (nav2))
					return XmlNodeOrder.Before;
			}

			XPathNavigator parent1 = nav1.Clone ();
			XPathNavigator parent2 = nav2.Clone ();
			while (parent1.MoveToParent () && parent2.MoveToParent ())
			{
				if (parent1.IsSamePosition (parent2))
				{
					// the ordering is namespace, attribute, children
					// assume that nav1 is before nav2, find counter-example
					if (nav1.NodeType == XPathNodeType.Namespace)
					{
						if (nav2.NodeType == XPathNodeType.Namespace)
						{
							// match namespaces
							while (nav2.MoveToNextNamespace ())
								if (nav2.IsSamePosition (nav1))
									return XmlNodeOrder.After;
						}
					}
					else if (nav1.NodeType == XPathNodeType.Attribute)
					{
						if (nav2.NodeType == XPathNodeType.Namespace)
							return XmlNodeOrder.After;
						else if (nav2.NodeType == XPathNodeType.Attribute)
						{
							// match attributes
							while (nav2.MoveToNextAttribute ())
								if (nav2.IsSamePosition (nav1))
									return XmlNodeOrder.After;
						}
					}
					else
					{
						switch (nav2.NodeType) {
						case XPathNodeType.Namespace:
						case XPathNodeType.Attribute:
							return XmlNodeOrder.After;
						}
						// match children
						while (nav2.MoveToNext ())
							if (nav2.IsSamePosition (nav1))
								return XmlNodeOrder.After;
					}
					return XmlNodeOrder.Before;
				}
				nav1.MoveToParent ();
				nav2.MoveToParent ();
			}
			return XmlNodeOrder.Unknown;
		}

		public virtual XPathExpression Compile (string xpath)
		{
			XPathParser parser = new XPathParser ();
			return new CompiledExpression (parser.Compile (xpath));
		}
		
		internal virtual XPathExpression Compile (string xpath, System.Xml.Xsl.IStaticXsltContext ctx)
		{
			XPathParser parser = new XPathParser (ctx);
			return new CompiledExpression (parser.Compile (xpath));
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
		
		internal virtual object Evaluate (XPathExpression expr, XPathNodeIterator context, XmlNamespaceManager ctx)
		{
			CompiledExpression cexpr = (CompiledExpression) expr;
			if (ctx == null)
				ctx = cexpr.NamespaceManager;
			
			if (context == null)
				context = new NullIterator (this, ctx);
			BaseIterator iterContext = (BaseIterator) context;
			iterContext.NamespaceManager = ctx;
			return cexpr.Evaluate (iterContext);
		}

		internal XPathNodeIterator EvaluateNodeSet (XPathExpression expr, XPathNodeIterator context, XmlNamespaceManager ctx)
		{
			CompiledExpression cexpr = (CompiledExpression) expr;
			if (ctx == null)
				ctx = cexpr.NamespaceManager;
			
			if (context == null)
				context = new NullIterator (this, cexpr.NamespaceManager);
			BaseIterator iterContext = (BaseIterator) context;
			iterContext.NamespaceManager = ctx;
			return cexpr.EvaluateNodeSet (iterContext);
		}

		internal string EvaluateString (XPathExpression expr, XPathNodeIterator context, XmlNamespaceManager ctx)
		{
			CompiledExpression cexpr = (CompiledExpression) expr;
			if (ctx == null)
				ctx = cexpr.NamespaceManager;
			
			if (context == null)
				context = new NullIterator (this, cexpr.NamespaceManager);
			BaseIterator iterContext = (BaseIterator) context;
			iterContext.NamespaceManager = ctx;
			return cexpr.EvaluateString (iterContext);
		}

		internal double EvaluateNumber (XPathExpression expr, XPathNodeIterator context, XmlNamespaceManager ctx)
		{
			CompiledExpression cexpr = (CompiledExpression) expr;
			if (ctx == null)
				ctx = cexpr.NamespaceManager;
			
			if (context == null)
				context = new NullIterator (this, cexpr.NamespaceManager);
			BaseIterator iterContext = (BaseIterator) context;
			iterContext.NamespaceManager = ctx;
			return cexpr.EvaluateNumber (iterContext);
		}

		internal bool EvaluateBoolean (XPathExpression expr, XPathNodeIterator context, XmlNamespaceManager ctx)
		{
			CompiledExpression cexpr = (CompiledExpression) expr;
			if (ctx == null)
				ctx = cexpr.NamespaceManager;
			
			if (context == null)
				context = new NullIterator (this, cexpr.NamespaceManager);
			BaseIterator iterContext = (BaseIterator) context;
			iterContext.NamespaceManager = ctx;
			return cexpr.EvaluateBoolean (iterContext);
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
			return Select (expr, null);
		}
		
		internal virtual XPathNodeIterator Select (XPathExpression expr, XmlNamespaceManager ctx)
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

		public virtual XPathNodeIterator SelectChildren (XPathNodeType type)
		{
			return SelectTest (new NodeTypeTest (Axes.Child, type));
		}

		public virtual XPathNodeIterator SelectChildren (string name, string namespaceURI)
		{
			if (name == null)
				throw new ArgumentNullException ("name");
			if (namespaceURI == null)
				throw new ArgumentNullException ("namespaceURI");

			Axes axis = Axes.Child;
			XmlQualifiedName qname = new XmlQualifiedName (name, namespaceURI);
			return SelectTest (new NodeNameTest (axis, qname, true));
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

		[MonoTODO]
		public virtual bool CheckValidity (XmlSchemaSet schemas, ValidationEventHandler handler)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual bool CheckValidity (XmlSchemaSet schemas, ValidationEventHandler handler, XmlSchemaAttribute attribute)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual bool CheckValidity (XmlSchemaSet schemas, ValidationEventHandler handler, XmlSchemaElement element)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual bool CheckValidity (XmlSchemaSet schemas, ValidationEventHandler handler, XmlSchemaType schemaType)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual object CopyAsObject (Type targetType)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual XPathNavigator CreateNavigator ()
		{
			return Clone ();
		}

		[MonoTODO]
		public virtual object Evaluate (string xpath, IXmlNamespaceResolver nsResolver)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual IDictionary GetNamespacesInScope (XmlNamespaceScope scope)
		{
			throw new NotImplementedException ();
		}

		public virtual string LookupNamespace (string prefix)
		{
			return LookupNamespace (prefix, false);
		}

		[MonoTODO]
		public virtual string LookupNamespace (string prefix, bool atomizedNames)
		{
			throw new NotImplementedException ();
		}

		public virtual string LookupPrefix (string namespaceUri)
		{
			return LookupPrefix (namespaceUri, false);
		}

		[MonoTODO]
		public virtual string LookupPrefix (string namespaceUri, bool atomizedNames)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual XmlReader ReadSubtree ()
		{
			throw new NotImplementedException ();
		}

		public virtual XPathNavigator SelectSingleNode (string xpath)
		{
			return SelectSingleNode (xpath, null);
		}

		[MonoTODO]
		public virtual XPathNavigator SelectSingleNode (string xpath, IXmlNamespaceResolver nsResolver)
		{
			throw new NotImplementedException ();
		}

		public virtual object ValueAs (Type type)
		{
			return ValueAs (type, null);
		}

		[MonoTODO]
		public virtual object ValueAs (Type type, IXmlNamespaceResolver nsResolver)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual XmlWriter WriteSubtree ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public bool HasNamespaceResolver {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public virtual string InnerXml {
			get { throw new NotImplementedException (); }
		}

		public virtual bool IsNode {
			get { throw new NotImplementedException (); }
		}

/* FIXME: It should be member, but requires new Collection type.
		[MonoTODO]
		public virtual IKeyComparer NavigatorComparer {
			get { throw new NotImplementedException (); }
		}
*/
		[MonoTODO]
		public virtual string OuterXml {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public virtual IXmlSchemaInfo SchemaInfo {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public virtual object TypedValue {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public virtual object UnderlyingObject {
			get { throw new NotImplementedException (); }
		}

		public virtual bool ValueAsBoolean {
			get { throw new NotImplementedException (); }
		}

		public virtual DateTime ValueAsDateTime {
			get { throw new NotImplementedException (); }
		}

		public virtual decimal ValueAsDecimal {
			get { throw new NotImplementedException (); }
		}

		public virtual double ValueAsDouble {
			get { throw new NotImplementedException (); }
		}

		public virtual int ValueAsInt32 {
			get { throw new NotImplementedException (); }
		}

		public virtual long ValueAsInt64 {
			get { throw new NotImplementedException (); }
		}

		public virtual ICollection ValueAsList {
			get { throw new NotImplementedException (); }
		}

		public virtual float ValueAsSingle {
			get { throw new NotImplementedException (); }
		}

		public virtual Type ValueType {
			get { throw new NotImplementedException (); }
		}

		public virtual XmlSchemaType XmlType {
			get { throw new NotImplementedException (); }
		}

		protected XmlReader GetValidatingReader (XmlSchemaSet schemas, ValidationEventHandler handler)
		{
			throw new NotImplementedException ();
		}
#endif
	}
}
