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
			get { return Clone ().MoveToFirstAttribute (); }
		}

		public virtual bool HasChildren {
			get { return Clone ().MoveToFirstChild (); }
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
			return new CompiledExpression (xpath, parser.Compile (xpath));
		}
		
		internal virtual XPathExpression Compile (string xpath, System.Xml.Xsl.IStaticXsltContext ctx)
		{
			XPathParser parser = new XPathParser (ctx);
			return new CompiledExpression (xpath, parser.Compile (xpath));
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
		
		internal virtual object Evaluate (XPathExpression expr, XPathNodeIterator context, NSResolver ctx)
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

		internal XPathNodeIterator EvaluateNodeSet (XPathExpression expr, XPathNodeIterator context, NSResolver ctx)
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

		internal string EvaluateString (XPathExpression expr, XPathNodeIterator context, NSResolver ctx)
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

		internal double EvaluateNumber (XPathExpression expr, XPathNodeIterator context, NSResolver ctx)
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

		internal bool EvaluateBoolean (XPathExpression expr, XPathNodeIterator context, NSResolver ctx)
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

#if NET_2_0
		public virtual string GetAttribute (string localName, string namespaceURI)
		{
			XPathNavigator nav = Clone ();
			if (nav.MoveToAttribute (localName, namespaceURI))
				return nav.Value;
			else
				return String.Empty;
		}

		public virtual string GetNamespace (string name)
		{
			XPathNavigator nav = Clone ();
			if (nav.MoveToNamespace (name))
				return nav.Value;
			else
				return String.Empty;
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
		
		internal virtual XPathNodeIterator Select (XPathExpression expr, NSResolver ctx)
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

		public virtual bool CheckValidity (XmlSchemaSet schemas, ValidationEventHandler handler)
		{
			XmlReaderSettings settings = new XmlReaderSettings ();
			settings.NameTable = NameTable;
			settings.SetSchemas (schemas);
			settings.ValidationEventHandler += handler;
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

		[MonoTODO]
		public virtual object Evaluate (string xpath, IXmlNamespaceResolver nsResolver)
		{
			return Evaluate (Compile (xpath), null, nsResolver);
		}

		[MonoTODO]
		public virtual IDictionary GetNamespacesInScope (XmlNamespaceScope scope)
		{
			Hashtable table = new Hashtable ();
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

		public virtual string LookupNamespace (string prefix)
		{
			return LookupNamespace (prefix, false);
		}

		[Obsolete]
		public virtual string LookupNamespace (string prefix, bool atomizedNames)
		{
			XPathNavigator nav = Clone ();
			if (nav.NodeType != XPathNodeType.Element)
				nav.MoveToParent ();
			if (nav.MoveToNamespace (prefix)) {
				if (atomizedNames)
					return nav.NameTable.Add (nav.Value);
				else
					return nav.Value;
			}
			return null;
		}

		public virtual string LookupPrefix (string namespaceUri)
		{
			return LookupPrefix (namespaceUri, false);
		}

		[Obsolete]
		public virtual string LookupPrefix (string namespaceUri, bool atomizedNames)
		{
			XPathNavigator nav = Clone ();
			if (nav.NodeType != XPathNodeType.Element)
				nav.MoveToParent ();
			if (!nav.MoveToFirstNamespace ())
				return null;
			do {
				if (atomizedNames) {
					if (Object.ReferenceEquals (nav.Value, namespaceUri))
						return nav.Name;
				} else {
					if (nav.Value == namespaceUri)
						return nav.Name;
				}
			} while (nav.MoveToNextNamespace ());
			return null;
		}

		[Obsolete]
		public virtual bool MoveToAttribute (string localName, string namespaceURI, bool atomizedNames)
		{
			return MoveToAttribute (localName, namespaceURI);
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

		public virtual bool MoveToChild (XPathNodeType type)
		{
			return MoveTo (SelectChildren (type));
		}

		public virtual bool MoveToChild (string localName, string namespaceURI)
		{
			return MoveTo (SelectChildren (localName, namespaceURI));
		}

		[Obsolete]
		public virtual bool MoveToChild (string localName, string namespaceURI, bool atomizedNames)
		{
			return MoveToChild (localName, namespaceURI);
		}

		[Obsolete]
		public virtual bool MoveToDescendant (XPathNodeType type)
		{
			return MoveTo (SelectDescendants (type, false));
		}

		[Obsolete]
		public virtual bool MoveToDescendant (string localName, string namespaceURI)
		{
			return MoveTo (SelectDescendants (localName, namespaceURI, false));
		}

		[Obsolete]
		public virtual bool MoveToDescendant (string localName, string namespaceURI, bool atomizedNames)
		{
			return MoveToDescendant (localName, namespaceURI);
		}

		public virtual bool MoveToNext (string localName, string namespaceURI)
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

		[Obsolete]
		public virtual bool MoveToNext (string localName, string namespaceURI, bool atomizedNames)
		{
			return MoveToNext (localName, namespaceURI);
		}

		public virtual bool MoveToNext (XPathNodeType type)
		{
			XPathNavigator nav = Clone ();
			while (nav.MoveToNext ()) {
				if (nav.NodeType == type) {
					MoveTo (nav);
					return true;
				}
			}
			return false;
		}

		[MonoTODO]
		public virtual bool MoveToFollowing (string localName,
			string namespaceURI)
		{
			return MoveToFollowing (localName, namespaceURI, null);
		}

		[MonoTODO]
		public virtual bool MoveToFollowing (string localName,
			string namespaceURI, XPathNavigator end)
		{
			XPathNavigator nav = Clone ();
			bool skip = false;
			do {
				if (!skip && nav.MoveToDescendant (localName,
					namespaceURI)) {
					if (end != null) {
						switch (nav.ComparePosition (end)) {
						case XmlNodeOrder.After:
						case XmlNodeOrder.Unknown:
							return false;
						}
					}
					MoveTo (nav);
					return true;
				}
				else
					skip = false;
				if (!nav.MoveToNext ()) {
					if (!nav.MoveToParent ())
						break;
					skip = true;
				}
			} while (true);
			return false;
		}

		[MonoTODO]
		public virtual bool MoveToFollowing (XPathNodeType type)
		{
			return MoveToFollowing (type, null);
		}

		[MonoTODO]
		public virtual bool MoveToFollowing (XPathNodeType type,
			XPathNavigator end)
		{
			XPathNavigator nav = Clone ();
			bool skip = false;
			do {
				if (!skip && nav.MoveToDescendant (type)) {
					if (end != null) {
						switch (nav.ComparePosition (end)) {
						case XmlNodeOrder.After:
						case XmlNodeOrder.Unknown:
							return false;
						}
					}
					MoveTo (nav);
					return true;
				}
				else
					skip = false;
				if (!nav.MoveToNext ()) {
					if (!nav.MoveToParent ())
						break;
					skip = true;
				}
			} while (true);
			return false;
		}

		[MonoTODO]
		public virtual XmlReader ReadSubtree ()
		{
			return new XPathNavigatorReader (this);
		}

		public virtual XPathNodeIterator Select (string xpath, IXmlNamespaceResolver nsResolver)
		{
			return Select (Compile (xpath), nsResolver);
		}

		public virtual XPathNavigator SelectSingleNode (string xpath)
		{
			return SelectSingleNode (xpath, null);
		}

		public virtual XPathNavigator SelectSingleNode (string xpath, IXmlNamespaceResolver nsResolver)
		{
			XPathExpression expr = Compile (xpath);
			expr.SetContext (nsResolver);
			return SelectSingleNode (expr);
		}

		public XPathNavigator SelectSingleNode (XPathExpression expression)
		{
			XPathNodeIterator iter = Select (expression);
			if (iter.MoveNext ())
				return iter.Current;
			else
				return null;
		}

		[MonoTODO]
		public override object ValueAs (Type type, IXmlNamespaceResolver nsResolver)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual void WriteSubtree (XmlWriter writer)
		{
			XmlReader st = ReadSubtree ();
			writer.WriteNode (st, false);
		}

		[MonoTODO]
		public virtual string InnerXml {
			get {
				XmlReader r = ReadSubtree ();
				r.Read (); // start
				// skip the element itself (or will reach to 
				// EOF if other than element) unless writing
				// doc itself
				int depth = r.Depth;
				if (NodeType != XPathNodeType.Root)
					r.Read ();
				StringWriter sw = new StringWriter ();
				XmlWriter xtw = XmlWriter.Create (sw);
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

		[MonoTODO]
		public override bool IsNode {
			get { return true; }
		}

		[MonoTODO]
		public virtual IKeyComparer NavigatorComparer {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public virtual string OuterXml {
			get {
				StringWriter sw = new StringWriter ();
				XmlTextWriter xtw = new XmlTextWriter (sw);
				WriteSubtree (xtw);
				xtw.Close ();
				return sw.ToString ();
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

		[MonoTODO]
		public virtual IXmlSchemaInfo SchemaInfo {
			get {
				return null;
			}
		}

		[MonoTODO]
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

		[MonoTODO]
		public virtual object UnderlyingObject {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public override bool ValueAsBoolean {
			get { return XQueryConvert.StringToBoolean (Value); }
		}

		[MonoTODO]
		public override DateTime ValueAsDateTime {
			get { return XmlConvert.ToDateTime (Value); }
		}

		[MonoTODO]
		public override decimal ValueAsDecimal {
			get { return XQueryConvert.StringToDecimal (Value); }
		}

		[MonoTODO]
		public override double ValueAsDouble {
			get { return XQueryConvert.StringToDouble (Value); }
		}

		[MonoTODO]
		public override int ValueAsInt32 {
			get { return XQueryConvert.StringToInt (Value); }
		}

		[MonoTODO]
		public override long ValueAsInt64 {
			get { return XQueryConvert.StringToInteger (Value); }
		}

		[MonoTODO]
		public override ICollection ValueAsList {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public override float ValueAsSingle {
			get { return XQueryConvert.StringToFloat (Value); }
		}

		[MonoTODO]
		public override Type ValueType {
			get {
				return SchemaInfo != null &&
					SchemaInfo.SchemaType != null &&
					SchemaInfo.SchemaType.Datatype != null ?
					SchemaInfo.SchemaType.Datatype.ValueType
					: null;
			}
		}

		[MonoTODO]
		public override XmlSchemaType XmlType {
			get {
				if (SchemaInfo != null)
					return SchemaInfo.SchemaType;
				return null;
			}
		}

		[MonoTODO]
		protected XmlReader GetValidatingReader (XmlSchemaSet schemas, ValidationEventHandler handler, XmlSchemaType schemaType)
		{
			throw new NotImplementedException ();
		}







		private XmlReader CreateFragmentReader (string fragment)
		{
			return new XmlTextReader (fragment, XmlNodeType.Element, new XmlParserContext (NameTable, null, null, XmlSpace.None));
		}

		public virtual XmlWriter AppendChild ()
		{
			throw new NotSupportedException ();
		}

		[MonoTODO]
		public virtual XPathNavigator AppendChild (
			string xmlFragments)
		{
			// FIXME: should XmlParserContext be something?
			return AppendChild (CreateFragmentReader (xmlFragments));
		}

		[MonoTODO]
		public virtual XPathNavigator AppendChild (
			XmlReader reader)
		{
			XmlWriter w = AppendChild ();
			while (!reader.EOF)
				w.WriteNode (reader, false);
			w.Close ();
			XPathNavigator nav = Clone ();
			nav.MoveToFirstChild ();
			while (nav.MoveToNext ())
				;
			return nav;
		}

		[MonoTODO]
		public virtual XPathNavigator AppendChild (
			XPathNavigator nav)
		{
			return AppendChild (new XPathNavigatorReader (nav));
		}

		public void AppendChildElement (string prefix, string name, string ns, string value)
		{
			XmlWriter xw = AppendChild ();
			xw.WriteStartElement (prefix, name, ns);
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

		public virtual XmlWriter CreateAttributes ()
		{
			throw new NotSupportedException ();
		}

		public virtual bool DeleteSelf ()
		{
			throw new NotSupportedException ();
		}

		public virtual XmlWriter InsertAfter ()
		{
			XPathNavigator nav = Clone ();
			if (nav.MoveToNext ())
				return nav.InsertBefore ();
			else
				return AppendChild ();
		}

		public virtual XPathNavigator InsertAfter (string xmlFragments)
		{
			return InsertAfter (CreateFragmentReader (xmlFragments));
		}

		[MonoTODO]
		public virtual XPathNavigator InsertAfter (XmlReader reader)
		{
			using (XmlWriter w = InsertAfter ()) {
				w.WriteNode (reader, false);
			}
			XPathNavigator nav = Clone ();
			nav.MoveToNext ();
			return nav;
		}

		[MonoTODO]
		public virtual XPathNavigator InsertAfter (XPathNavigator nav)
		{
			return InsertAfter (new XPathNavigatorReader (nav));
		}

		public virtual XmlWriter InsertBefore ()
		{
			throw new NotSupportedException ();
		}

		public virtual XPathNavigator InsertBefore (string xmlFragments)
		{
			return InsertBefore (CreateFragmentReader (xmlFragments));
		}

		[MonoTODO]
		public virtual XPathNavigator InsertBefore (XmlReader reader)
		{
			using (XmlWriter w = InsertBefore ()) {
				w.WriteNode (reader, false);
			}
			XPathNavigator nav = Clone ();
			nav.MoveToPrevious ();
			return nav;
		}

		[MonoTODO]
		public virtual XPathNavigator InsertBefore (XPathNavigator nav)
		{
			return InsertBefore (new XPathNavigatorReader (nav));
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
				return InsertBefore ();
		}

		public virtual XPathNavigator PrependChild (string xmlFragments)
		{
			return PrependChild (CreateFragmentReader (xmlFragments));
		}

		[MonoTODO]
		public virtual XPathNavigator PrependChild (XmlReader reader)
		{
			using (XmlWriter w = PrependChild ()) {
				w.WriteNode (reader, false);
			}
			XPathNavigator nav = Clone ();
			nav.MoveToFirstChild ();
			return nav;
		}

		[MonoTODO]
		public virtual XPathNavigator PrependChild (XPathNavigator nav)
		{
			return PrependChild (new XPathNavigatorReader (nav));
		}

		public virtual void PrependChildElement (string prefix, 
			string localName, string namespaceURI, string value)
		{
			using (XmlWriter w = PrependChild ()) {
				w.WriteElementString (prefix, localName, namespaceURI, value);
			}
		}

		[MonoTODO]
		public virtual bool ReplaceSelf (string xmlFragment)
		{
			return ReplaceSelf (XmlReader.Create (new StringReader (xmlFragment)));
		}

		[MonoTODO]
		public virtual bool ReplaceSelf (XmlReader reader)
		{
			InsertBefore (reader);
			return DeleteSelf ();
		}

		[MonoTODO]
		public virtual bool ReplaceSelf (XPathNavigator navigator)
		{
			return ReplaceSelf (new XPathNavigatorReader (navigator));
		}

		// Dunno the exact purpose, but maybe internal editor use
		[MonoTODO]
		public virtual void SetTypedValue (object value)
		{
			throw new NotSupportedException ();
		}

		public virtual void SetValue (string value)
		{
			throw new NotSupportedException ();
		}

		[MonoTODO]
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
