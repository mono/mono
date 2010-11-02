//
// Copyright (C) 2010 Novell Inc. http://novell.com
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
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Windows.Markup;
using System.Xaml.Schema;
using System.Xml;

namespace System.Xaml
{
	internal class PrefixLookup : INamespacePrefixLookup
	{
		public PrefixLookup (XamlSchemaContext schemaContext)
		{
			sctx = schemaContext;
			Namespaces = new List<NamespaceDeclaration> ();
		}
		
		XamlSchemaContext sctx;
		
		public bool IsCollectingNamespaces { get; set; }
		
		public List<NamespaceDeclaration> Namespaces { get; private set; }

		public string LookupPrefix (string ns)
		{
			var nd = Namespaces.FirstOrDefault (n => n.Namespace == ns);
			if (nd == null && IsCollectingNamespaces)
				return AddNamespace (ns);
			else
				return nd != null ? nd.Prefix : null;
		}
		
		public string AddNamespace (string ns)
		{
			var l = Namespaces;
			string prefix, s;
			if (ns == XamlLanguage.Xaml2006Namespace)
				prefix = "x";
			else if (!l.Any (i => i.Prefix == String.Empty))
				prefix = String.Empty;
			else if ((s = GetAcronym (ns)) != null && !l.Any (i => i.Prefix == s))
				prefix = s;
			else
				prefix = sctx.GetPreferredPrefix (ns);
			l.Add (new NamespaceDeclaration (ns, prefix));
			return prefix;
		}
		
		string GetAcronym (string ns)
		{
			int idx = ns.IndexOf (';');
			if (idx < 0)
				return null;
			string pre = "clr-namespace:";
			if (!ns.StartsWith (pre, StringComparison.Ordinal))
				return null;
			ns = ns.Substring (pre.Length, idx - pre.Length);
			string ac = "";
			foreach (string nsp in ns.Split ('.'))
				if (nsp.Length > 0)
					ac += nsp [0];
			return ac.Length > 0 ? ac.ToLower (CultureInfo.InvariantCulture) : null;
		}
	}

	internal struct XamlNodeIterator
	{
		static readonly XamlObject null_object = new XamlObject (XamlLanguage.Null, null);

		public XamlNodeIterator (object root, XamlSchemaContext schemaContext, PrefixLookup prefixLookup)
		{
			ctx = schemaContext;
			this.root = root;
			this.prefix_lookup = prefixLookup;
		}
		
		XamlSchemaContext ctx;
		object root;
		// FIXME: this will become IServiceProvider.
		PrefixLookup prefix_lookup;
		
		public XamlSchemaContext SchemaContext {
			get { return ctx; }
		}
		
		XamlType GetType (object obj)
		{
			return ctx.GetXamlType (new InstanceContext (obj).GetWrappedValue ().GetType ());
		}
		
		// returns StartObject, StartMember, Value, EndMember and EndObject. (NamespaceDeclaration is not included)
		public IEnumerable<XamlNodeInfo> GetNodes ()
		{
			var xobj = new XamlObject (GetType (root), root);
			foreach (var node in GetNodes (null, xobj))
				yield return node;
		}
		
		IEnumerable<XamlNodeInfo> GetNodes (XamlMember xm, XamlObject xobj)
		{
			return GetNodes (xm, xobj, null);
		}

		IEnumerable<XamlNodeInfo> GetNodes (XamlMember xm, XamlObject xobj, XamlType overrideMemberType)
		{
			// collection items: each item is exposed as a standalone object that has StartObject, EndObject and contents.
			if (xm == XamlLanguage.Items) {
				foreach (var xn in GetItemsNodes (xm, xobj))
					yield return xn;
				yield break;
			}
			
			// Arguments: each argument is written as a standalone object
			if (xm == XamlLanguage.Arguments) {
				foreach (var argm in xobj.Type.GetSortedConstructorArguments ()) {
					var argv = argm.Invoker.GetValue (xobj.GetRawValue ());
					var xarg = new XamlObject (argm.Type, argv);
					foreach (var cn in GetNodes (null, xarg))
						yield return cn;
				}
				yield break;
			}

			// PositionalParameters: items are from constructor arguments, and are all in simple string value, written as Value node sequentially.
			if (xm == XamlLanguage.PositionalParameters) {
				foreach (var argm in xobj.Type.GetSortedConstructorArguments ()) {
					// Unlike XamlLanguage.Items, it only outputs string value. So, convert values here.
					var argv = argm.Type.GetStringValue (xobj.GetMemberValue (argm), prefix_lookup);
					yield return new XamlNodeInfo ((string) argv);
				}
				yield break;
			}

			if (xm == XamlLanguage.Initialization) {
				yield return new XamlNodeInfo (xobj.Type.GetStringValue (xobj.GetRawValue (), prefix_lookup));
				yield break;
			}

			// Value - only for non-top-level node (thus xm != null)
			if (xm != null) {
				// overrideMemberType is (so far) used for XamlLanguage.Key.
				var xtt = overrideMemberType ?? xm.Type;
				if (xtt.IsContentValue ()) {
					// though null value is special: it is written as a standalone object.
					var val = xobj.GetRawValue ();
					if (val == null)
						foreach (var xn in GetNodes (null, null_object))
							yield return xn;
					else
						yield return new XamlNodeInfo (xtt.GetStringValue (val, prefix_lookup));
					yield break;
				}
			}

			// collection items: return GetObject and Items.
			if (xm != null && xm.Type.IsCollection && !xm.IsWritePublic) {
				yield return new XamlNodeInfo (XamlNodeType.GetObject, xobj);
				// Write Items member only when there are items (i.e. do not write it if it is empty).
				var xnm = new XamlNodeMember (xobj, XamlLanguage.Items);
				var en = GetNodes (XamlLanguage.Items, xnm.Value).GetEnumerator ();
				if (en.MoveNext ()) {
					yield return new XamlNodeInfo (XamlNodeType.StartMember, xnm);
					do {
						yield return en.Current;
					} while (en.MoveNext ());
					yield return new XamlNodeInfo (XamlNodeType.EndMember, xnm);
				}
				yield return new XamlNodeInfo (XamlNodeType.EndObject, xobj);
			} else {
				// Object
				yield return new XamlNodeInfo (XamlNodeType.StartObject, xobj);
				foreach (var xn in GetObjectMemberNodes (xobj))
					yield return xn;
				yield return new XamlNodeInfo (XamlNodeType.EndObject, xobj);
			}
		}

		IEnumerable<XamlNodeInfo> GetObjectMemberNodes (XamlObject xobj)
		{
			var xce = xobj.Children ().GetEnumerator ();
			while (xce.MoveNext ()) {
				// XamlLanguage.Items does not show up if the content is empty.
				if (xce.Current.Member == XamlLanguage.Items)
					if (!GetNodes (xce.Current.Member, xce.Current.Value).GetEnumerator ().MoveNext ())
						continue;

				// Other collections as well, but needs different iteration (as nodes contain GetObject and EndObject).
				if (!xce.Current.Member.IsWritePublic && xce.Current.Member.Type != null && xce.Current.Member.Type.IsCollection) {
					var e = GetNodes (xce.Current.Member, xce.Current.Value).GetEnumerator ();
					if (!(e.MoveNext () && e.MoveNext () && e.MoveNext ())) // GetObject, EndObject and more
						continue;
				}

				yield return new XamlNodeInfo (XamlNodeType.StartMember, xce.Current);
				foreach (var cn in GetNodes (xce.Current.Member, xce.Current.Value))
					yield return cn;
				yield return new XamlNodeInfo (XamlNodeType.EndMember, xce.Current);
			}
		}

		IEnumerable<XamlNodeInfo> GetItemsNodes (XamlMember xm, XamlObject xobj)
		{
			var ie = xobj.Type.Invoker.GetItems (xobj.GetRawValue ());
			while (ie.MoveNext ()) {
				var iobj = ie.Current;
				// If it is dictionary, then retrieve the key, and rewrite the item as the Value part.
				object ikey = null;
				XamlNodeMember xknm = default (XamlNodeMember);
				if (xobj.Type.IsDictionary) {
					Type kvpType = iobj.GetType ();
					bool isNonGeneric = kvpType == typeof (DictionaryEntry);
					var kp = isNonGeneric ? null : kvpType.GetProperty ("Key");
					var vp = isNonGeneric ? null : kvpType.GetProperty ("Value");
					xknm = new XamlNodeMember (xobj, XamlLanguage.Key);
					ikey = isNonGeneric ? ((DictionaryEntry) iobj).Key : kp.GetValue (iobj, null);
					iobj = isNonGeneric ? ((DictionaryEntry) iobj).Value : vp.GetValue (iobj, null);
				}

				var xiobj = new XamlObject (GetType (iobj), iobj);
				var en = GetNodes (null, xiobj).GetEnumerator ();
				en.MoveNext ();
				yield return en.Current;
				if (ikey != null) {
					// Key member is written *inside* the item object.
					yield return new XamlNodeInfo (XamlNodeType.StartMember, xknm);
					foreach (var xn in GetNodes (XamlLanguage.Key, new XamlObject (GetType (ikey), ikey), xobj.Type.KeyType))
						yield return xn;
					yield return new XamlNodeInfo (XamlNodeType.EndMember, xknm);
				}
				while (en.MoveNext ())
					yield return en.Current;
			}
		}

		// Namespace retrieval. 
		// It is iterated before iterating the actual object nodes,
		// and results are cached for use in XamlObjectReader.
		public void CollectNamespaces ()
		{
			prefix_lookup.IsCollectingNamespaces = true;
			foreach (var xn in GetNodes ()) {
				if (xn.NodeType == XamlNodeType.GetObject)
					continue; // it is out of consideration here.
				if (xn.NodeType == XamlNodeType.StartObject) {
					foreach (var ns in NamespacesInType (xn.Object.Type))
						prefix_lookup.LookupPrefix (ns);
				} else if (xn.NodeType == XamlNodeType.StartMember) {
					var xm = xn.Member.Member;
					// This filtering is done as a black list so far. There does not seem to be any usable property on XamlDirective.
					if (xm == XamlLanguage.Items || xm == XamlLanguage.PositionalParameters || xm == XamlLanguage.Initialization)
						continue;
					prefix_lookup.LookupPrefix (xn.Member.Member.PreferredXamlNamespace);
				} else {
					if (xn.NodeType == XamlNodeType.Value && xn.Value is Type)
						// this tries to lookup existing prefix, and if there isn't any, then adds a new declaration.
						XamlLanguage.Type.GetStringValue (xn.Value, prefix_lookup);
					continue;
				}
			}
			prefix_lookup.Namespaces.Sort ((nd1, nd2) => String.CompareOrdinal (nd1.Prefix, nd2.Prefix));
			prefix_lookup.IsCollectingNamespaces = false;
		}
		
		IEnumerable<string> NamespacesInType (XamlType xt)
		{
			yield return xt.PreferredXamlNamespace;
			if (xt.TypeArguments != null) {
				// It is for x:TypeArguments
				yield return XamlLanguage.Xaml2006Namespace;
				foreach (var targ in xt.TypeArguments)
					foreach (var ns in NamespacesInType (targ))
						yield return ns;
			}
		}
	}
	
	internal struct XamlNodeInfo
	{
		public XamlNodeInfo (XamlNodeType nodeType, XamlObject value)
		{
			node_type = nodeType;
			this.value = value;
			member = default (XamlNodeMember);
		}
		
		public XamlNodeInfo (XamlNodeType nodeType, XamlNodeMember member)
		{
			node_type = nodeType;
			this.value = default (XamlObject);
			this.member = member;
		}
		
		public XamlNodeInfo (string value)
		{
			node_type = XamlNodeType.Value;
			this.value = value;
			member = default (XamlNodeMember);
		}
		
		XamlNodeType node_type;
		object value;
		XamlNodeMember member;
		
		public XamlNodeType NodeType {
			get { return node_type; }
		}
		public XamlObject Object {
			get { return (XamlObject) value; }
		}
		public XamlNodeMember Member {
			get { return member; }
		}
		public object Value {
			get { return value; }
		}
	}
	
	internal struct XamlObject
	{
		public XamlObject (XamlType type, object instance)
			: this (type, new InstanceContext (instance))
		{
		}

		public XamlObject (XamlType type, InstanceContext context)
		{
			this.type = type;
			this.context = context;
		}
		
		readonly XamlType type;
		readonly InstanceContext context;
		
		public XamlType Type {
			get { return type; }
		}
		
		public InstanceContext Context {
			get { return context; }
		}
		
		XamlType GetType (object obj)
		{
			return type.SchemaContext.GetXamlType (new InstanceContext (obj).GetWrappedValue ().GetType ());
		}
		
		public IEnumerable<XamlNodeMember> Children ()
		{
			// FIXME: consider XamlLanguage.Key
			foreach (var xm in type.GetAllObjectReaderMembersByType (null))
				yield return new XamlNodeMember (this, xm);
		}
		
		public object GetRawValue ()
		{
			return context.GetRawValue ();
		}
		
		public object GetWrappedValue ()
		{
			return context.GetWrappedValue ();
		}
	}
	
	internal struct XamlNodeMember
	{
		public XamlNodeMember (XamlObject owner, XamlMember member)
		{
			this.owner = owner;
			this.member = member;
		}
		
		readonly XamlObject owner;
		readonly XamlMember member;
		
		public XamlObject Owner {
			get { return owner; }
		}
		public XamlMember Member {
			get { return member; }
		}
		public XamlObject Value {
			get {
				var mv = Owner.GetMemberValue (Member);
				return new XamlObject (GetType (mv), mv);
			}
		}

		XamlType GetType (object obj)
		{
			return owner.Type.SchemaContext.GetXamlType (new InstanceContext (obj).GetWrappedValue ().GetType ());
		}
	}
	
	// Its original purpose was to enable delayed reflection, but it's not supported yet.
	internal struct InstanceContext
	{
		static readonly NullExtension null_value = new NullExtension ();

		public InstanceContext (object value)
		{
			this.value = value;
		}
		
		object value;
		
		public object GetWrappedValue ()
		{
			var o = GetRawValue ();

			// FIXME: should this manually checked, or is there any way to automate it?
			if (o == null)
				return null_value;
			if (o is Array)
				return new ArrayExtension ((Array) o);
			if (o is Type)
				return new TypeExtension ((Type) o);
			return o;
		}
		
		public object GetRawValue ()
		{
			return value; // so far.
		}
	}

	internal static class TypeExtensionMethods2
	{
		static bool ExaminePositionalParametersApplicable (this XamlType type)
		{
			if (!type.IsMarkupExtension || type.UnderlyingType == null)
				return false;

			var args = type.GetSortedConstructorArguments ();
			if (args == null)
				return false;

			foreach (var arg in args)
				if (arg.Type != null && !arg.Type.IsContentValue () && arg.Type.TypeConverter == null)
					return false;

			Type [] argTypes = (from arg in args select arg.Type.UnderlyingType).ToArray ();
			if (argTypes.Any (at => at == null))
				return false;
			var ci = type.UnderlyingType.GetConstructor (argTypes);
			return ci != null;
		}
	
		// Note that this returns XamlMember which might not actually appear in XamlObjectReader. For example, XamlLanguage.Items won't be returned when there is no item in the collection.
		public static IEnumerable<XamlMember> GetAllObjectReaderMembersByType (this XamlType type, object dictionaryKey)
		{
			// FIXME: find out why only TypeExtension and StaticExtension yield this directive. Seealso XamlObjectReaderTest.Read_CustomMarkupExtension*()
			if (type == XamlLanguage.Type ||
			    type == XamlLanguage.Static ||
			    ExaminePositionalParametersApplicable (type) && type.ConstructionRequiresArguments) {
				yield return XamlLanguage.PositionalParameters;
				yield break;
			}

			// Note that if the XamlType has the default constructor, we don't need "Arguments".
			IEnumerable<XamlMember> args = type.ConstructionRequiresArguments ? type.GetSortedConstructorArguments () : null;
			if (args != null && args.Any ())
				yield return XamlLanguage.Arguments;

			if (dictionaryKey != null)
				yield return XamlLanguage.Key;

			if (type.TypeConverter != null || type.IsContentValue ()) {
				yield return XamlLanguage.Initialization;
				yield break;
			}

			if (type.IsDictionary) {
				yield return XamlLanguage.Items;
				yield break;
			}

			foreach (var m in type.GetAllMembers ()) {
				// do not read constructor arguments twice (they are written inside Arguments).
				if (args != null && args.Contains (m))
					continue;
				// do not return non-public members. Not sure why .NET filters out them though.
				if (!m.IsReadPublic)
					continue;

				yield return m;
			}
			
			if (type.IsCollection)
				yield return XamlLanguage.Items;
		}
	}
	
	internal static class XamlNodeExtensions
	{
		internal static object GetMemberValue (this XamlObject xobj, XamlMember xm)
		{
			if (xm.IsUnknown)
				return null;
			// FIXME: this looks like an ugly hack. Is this really true? What if there's MarkupExtension that uses another MarkupExtension type as a member type.
			var obj = xobj.Context.GetRawValue ();
			if (xm == XamlLanguage.Initialization)
				return obj;
			if (xm == XamlLanguage.Items) // collection itself.
				return obj;
			if (xm == XamlLanguage.Arguments) // object itself
				return obj;
			if (xm == XamlLanguage.PositionalParameters)
				return xobj.GetWrappedValue (); // dummy value
			return xm.Invoker.GetValue (xobj.GetWrappedValue ());
		}
	}
}
