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
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows.Markup;
using System.Xaml;
using System.Xaml.Schema;
using System.Xml;
using System.Xml.Serialization;

namespace System.Xaml
{
	internal class XamlObjectNodeIterator
	{
		static readonly XamlObject null_object = new XamlObject (XamlLanguage.Null, null);

		public XamlObjectNodeIterator (object root, XamlSchemaContext schemaContext, IValueSerializerContext vctx)
		{
			ctx = schemaContext;
			this.root = root;
			value_serializer_ctx = vctx;
		}
		
		XamlSchemaContext ctx;
		object root;
		IValueSerializerContext value_serializer_ctx;
		
		PrefixLookup PrefixLookup {
			get { return (PrefixLookup) value_serializer_ctx.GetService (typeof (INamespacePrefixLookup)); }
		}
		XamlNameResolver NameResolver {
			get { return (XamlNameResolver) value_serializer_ctx.GetService (typeof (IXamlNameResolver)); }
		}

		public XamlSchemaContext SchemaContext {
			get { return ctx; }
		}
		
		XamlType GetType (object obj)
		{
			return obj == null ? XamlLanguage.Null : ctx.GetXamlType (obj.GetType ());
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
			return GetNodes (xm, xobj, null, false);
		}

		IEnumerable<XamlNodeInfo> GetNodes (XamlMember xm, XamlObject xobj, XamlType overrideMemberType, bool partOfPositionalParameters)
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

			// PositionalParameters: items are from constructor arguments, written as Value node sequentially. Note that not all of them are in simple string value. Also, null values are not written as NullExtension
			if (xm == XamlLanguage.PositionalParameters) {
				foreach (var argm in xobj.Type.GetSortedConstructorArguments ()) {
					foreach (var cn in GetNodes (argm, new XamlObject (argm.Type, xobj.GetMemberValue (argm)), null, true))
						yield return cn;
				}
				yield break;
			}

			if (xm == XamlLanguage.Initialization) {
				yield return new XamlNodeInfo (TypeExtensionMethods.GetStringValue (xobj.Type, xm, xobj.GetRawValue (), value_serializer_ctx));
				yield break;
			}

			// Value - only for non-top-level node (thus xm != null)
			if (xm != null) {
				// overrideMemberType is (so far) used for XamlLanguage.Key.
				var xtt = overrideMemberType ?? xm.Type;
				if (!xtt.IsMarkupExtension && // this condition is to not serialize MarkupExtension whose type has TypeConverterAttribute (e.g. StaticExtension) as a string.
				    (xtt.IsContentValue (value_serializer_ctx) || xm.IsContentValue (value_serializer_ctx))) {
					// though null value is special: it is written as a standalone object.
					var val = xobj.GetRawValue ();
					if (val == null) {
						if (!partOfPositionalParameters)
							foreach (var xn in GetNodes (null, null_object))
								yield return xn;
						else
							yield return new XamlNodeInfo (String.Empty);
					}
					else
						yield return new XamlNodeInfo (TypeExtensionMethods.GetStringValue (xtt, xm, val, value_serializer_ctx));
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
			} else if (xm != null && xm.Type.IsXData) {
				var sw = new StringWriter ();
				var xw = XmlWriter.Create (sw, new XmlWriterSettings () { OmitXmlDeclaration = true, ConformanceLevel = ConformanceLevel.Auto });
				var val = xobj.GetRawValue () as IXmlSerializable;
				if (val == null)
					yield break; // do not output anything
				val.WriteXml (xw);
				xw.Close ();
				var obj = new XData () { Text = sw.ToString () };
				foreach (var xn in GetNodes (null, new XamlObject (XamlLanguage.XData, obj)))
					yield return xn;
			} else {
				// Object - could become Reference
				var val = xobj.GetRawValue ();
				if (!xobj.Type.IsContentValue (value_serializer_ctx) && val != null) {
					string refName = NameResolver.GetName (val);
					if (refName != null) {
						// The target object is already retrieved, so we don't return the same object again.
						NameResolver.SaveAsReferenced (val); // Record it as named object.
						// Then return Reference object instead.
						foreach (var xn in GetNodes (null, new XamlObject (XamlLanguage.Reference, new Reference (refName))))
							yield return xn;
						yield break;
					} else {
						// The object appeared in the xaml tree for the first time. So we store the reference with a unique name so that it could be referenced later.
						refName = GetReferenceName (xobj);
						if (NameResolver.IsCollectingReferences && NameResolver.Contains (refName))
							throw new InvalidOperationException (String.Format ("There is already an object of type {0} named as '{1}'. Object names must be unique.", val.GetType (), refName));
						NameResolver.SetNamedObject (refName, val, true); // probably fullyInitialized is always true here.
					}
				}
				yield return new XamlNodeInfo (XamlNodeType.StartObject, xobj);
				// If this object is referenced and there is no [RuntimeNameProperty] member, then return Name property in addition.
				if (val != null && xobj.Type.GetAliasedProperty (XamlLanguage.Name) == null) {
					string name = NameResolver.GetReferencedName (val);
					if (name != null) {
						var sobj = new XamlObject (XamlLanguage.String, name);
						foreach (var cn in GetMemberNodes (new XamlNodeMember (sobj, XamlLanguage.Name), new XamlNodeInfo [] { new XamlNodeInfo (name)}))
							yield return cn;
					}
				}
				foreach (var xn in GetObjectMemberNodes (xobj))
					yield return xn;
				yield return new XamlNodeInfo (XamlNodeType.EndObject, xobj);
			}
		}
		
		int used_reference_ids;
		
		string GetReferenceName (XamlObject xobj)
		{
			var xm = xobj.Type.GetAliasedProperty (XamlLanguage.Name);
			if (xm != null)
				return (string) xm.Invoker.GetValue (xobj.GetRawValue ());
			return "__ReferenceID" + used_reference_ids++;
		}

		IEnumerable<XamlNodeInfo> GetMemberNodes (XamlNodeMember member, IEnumerable<XamlNodeInfo> contents)
		{
				yield return new XamlNodeInfo (XamlNodeType.StartMember, member);
				foreach (var cn in contents)
					yield return cn;
				yield return new XamlNodeInfo (XamlNodeType.EndMember, member);
		}

		IEnumerable<XamlNodeMember> GetNodeMembers (XamlObject xobj, IValueSerializerContext vsctx)
		{
			// XData.XmlReader is not returned.
			if (xobj.Type == XamlLanguage.XData) {
				yield return new XamlNodeMember (xobj, XamlLanguage.XData.GetMember ("Text"));
				yield break;
			}

			// FIXME: find out why root Reference has PositionalParameters.
			if (xobj.GetRawValue() != root && xobj.Type == XamlLanguage.Reference)
				yield return new XamlNodeMember (xobj, XamlLanguage.PositionalParameters);
			else {
				var inst = xobj.GetRawValue ();
				var atts = new KeyValuePair<AttachableMemberIdentifier,object> [AttachablePropertyServices.GetAttachedPropertyCount (inst)];
				AttachablePropertyServices.CopyPropertiesTo (inst, atts, 0);
				foreach (var p in atts) {
					var axt = ctx.GetXamlType (p.Key.DeclaringType);
					yield return new XamlNodeMember (new XamlObject (axt, p.Value), axt.GetAttachableMember (p.Key.MemberName));
				}
				foreach (var xm in xobj.Type.GetAllObjectReaderMembersByType (vsctx))
					yield return new XamlNodeMember (xobj, xm);
			}
		}

		IEnumerable<XamlNodeInfo> GetObjectMemberNodes (XamlObject xobj)
		{
			var xce = GetNodeMembers (xobj, value_serializer_ctx).GetEnumerator ();
			while (xce.MoveNext ()) {
				// XamlLanguage.Items does not show up if the content is empty.
				if (xce.Current.Member == XamlLanguage.Items) {
					// FIXME: this is nasty, but this name resolution is the only side effect of this iteration model. Save-Restore procedure is required.
					NameResolver.Save ();
					try {
						if (!GetNodes (xce.Current.Member, xce.Current.Value).GetEnumerator ().MoveNext ())
							continue;
					} finally {
						NameResolver.Restore ();
					}
				}

				// Other collections as well, but needs different iteration (as nodes contain GetObject and EndObject).
				if (!xce.Current.Member.IsWritePublic && xce.Current.Member.Type != null && xce.Current.Member.Type.IsCollection) {
					var e = GetNodes (xce.Current.Member, xce.Current.Value).GetEnumerator ();
					// FIXME: this is nasty, but this name resolution is the only side effect of this iteration model. Save-Restore procedure is required.
					NameResolver.Save ();
					try {
						if (!(e.MoveNext () && e.MoveNext () && e.MoveNext ())) // GetObject, EndObject and more
							continue;
					} finally {
						NameResolver.Restore ();
					}
				}

				foreach (var cn in GetMemberNodes (xce.Current, GetNodes (xce.Current.Member, xce.Current.Value)))
					yield return cn;
			}
		}

		IEnumerable<XamlNodeInfo> GetItemsNodes (XamlMember xm, XamlObject xobj)
		{
			var obj = xobj.GetRawValue ();
			if (obj == null)
				yield break;
			var ie = xobj.Type.Invoker.GetItems (obj);
			while (ie.MoveNext ()) {
				var iobj = ie.Current;
				// If it is dictionary, then retrieve the key, and rewrite the item as the Value part.
				object ikey = null;
				if (xobj.Type.IsDictionary) {
					Type kvpType = iobj.GetType ();
					bool isNonGeneric = kvpType == typeof (DictionaryEntry);
					var kp = isNonGeneric ? null : kvpType.GetProperty ("Key");
					var vp = isNonGeneric ? null : kvpType.GetProperty ("Value");
					ikey = isNonGeneric ? ((DictionaryEntry) iobj).Key : kp.GetValue (iobj, null);
					iobj = isNonGeneric ? ((DictionaryEntry) iobj).Value : vp.GetValue (iobj, null);
				}

				var wobj = TypeExtensionMethods.GetExtensionWrapped (iobj);
				var xiobj = new XamlObject (GetType (wobj), wobj);
				if (ikey != null) {
					// Key member is written *inside* the item object.
					//
					// It is messy, but Key and Value are *sorted*. In most cases Key goes first, but for example PositionalParameters comes first.
					// To achieve this behavior, we compare XamlLanguage.Key and value's Member and returns in order. It's all nasty hack, but at least it could be achieved like this!

					var en = GetNodes (null, xiobj).ToArray ();
					yield return en [0]; // StartObject

					var xknm = new XamlNodeMember (xobj, XamlLanguage.Key);
					var nodes1 = en.Skip (1).Take (en.Length - 2);
					var nodes2 = GetKeyNodes (ikey, xobj.Type.KeyType, xknm);
					foreach (var xn in EnumerateMixingMember (nodes1, XamlLanguage.Key, nodes2))
						yield return xn;
					yield return en [en.Length - 1];
				}
				else
					foreach (var xn in GetNodes (null, xiobj))
						yield return xn;
			}
		}
		
		IEnumerable<XamlNodeInfo> EnumerateMixingMember (IEnumerable<XamlNodeInfo> nodes1, XamlMember m2, IEnumerable<XamlNodeInfo> nodes2)
		{
			if (nodes2 == null) {
				foreach (var cn in nodes1)
					yield return cn;
				yield break;
			}

			var e1 = nodes1.GetEnumerator ();
			var e2 = nodes2.GetEnumerator ();
			int nest = 0;
			while (e1.MoveNext ()) {
				if (e1.Current.NodeType == XamlNodeType.StartMember) {
					if (nest > 0)
						nest++;
					else
						if (TypeExtensionMethods.CompareMembers (m2, e1.Current.Member.Member) < 0) {
							while (e2.MoveNext ())
								yield return e2.Current;
						}
						else
							nest++;
				}
				else if (e1.Current.NodeType == XamlNodeType.EndMember)
					nest--;
				yield return e1.Current;
			}
			while (e2.MoveNext ())
				yield return e2.Current;
		}

		IEnumerable<XamlNodeInfo> GetKeyNodes (object ikey, XamlType keyType, XamlNodeMember xknm)
		{
			foreach (var xn in GetMemberNodes (xknm, GetNodes (XamlLanguage.Key, new XamlObject (GetType (ikey), ikey), keyType, false)))
				yield return xn;
		}

		// Namespace and Reference retrieval.
		// It is iterated before iterating the actual object nodes,
		// and results are cached for use in XamlObjectReader.
		public void PrepareReading ()
		{
			PrefixLookup.IsCollectingNamespaces = true;
			NameResolver.IsCollectingReferences = true;
			foreach (var xn in GetNodes ()) {
				if (xn.NodeType == XamlNodeType.GetObject)
					continue; // it is out of consideration here.
				if (xn.NodeType == XamlNodeType.StartObject) {
					foreach (var ns in NamespacesInType (xn.Object.Type))
						PrefixLookup.LookupPrefix (ns);
				} else if (xn.NodeType == XamlNodeType.StartMember) {
					var xm = xn.Member.Member;
					// This filtering is done as a black list so far. There does not seem to be any usable property on XamlDirective.
					if (xm == XamlLanguage.Items || xm == XamlLanguage.PositionalParameters || xm == XamlLanguage.Initialization)
						continue;
					PrefixLookup.LookupPrefix (xn.Member.Member.PreferredXamlNamespace);
				} else {
					if (xn.NodeType == XamlNodeType.Value && xn.Value is Type)
						// this tries to lookup existing prefix, and if there isn't any, then adds a new declaration.
						TypeExtensionMethods.GetStringValue (XamlLanguage.Type, xn.Member.Member, xn.Value, value_serializer_ctx);
					continue;
				}
			}
			PrefixLookup.Namespaces.Sort ((nd1, nd2) => String.CompareOrdinal (nd1.Prefix, nd2.Prefix));
			PrefixLookup.IsCollectingNamespaces = false;
			NameResolver.IsCollectingReferences = false;
			NameResolver.NameScopeInitializationCompleted (this);
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
}
