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
		
		public XamlNodeInfo (object value)
		{
			node_type = XamlNodeType.Value;
			this.value = value;
			member = default (XamlNodeMember);
		}
		
		public XamlNodeInfo (NamespaceDeclaration ns)
		{
			node_type = XamlNodeType.NamespaceDeclaration;
			this.value = ns;
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
			return type.SchemaContext.GetXamlType (obj.GetType ());
		}
		
		public object GetRawValue ()
		{
			return context.GetRawValue ();
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
			return obj == null ? XamlLanguage.Null : owner.Type.SchemaContext.GetXamlType (new InstanceContext (obj).GetRawValue ().GetType ());
		}
	}
	
	// Its original purpose was to enable delayed reflection, but it's not supported yet.
	internal struct InstanceContext
	{
		public InstanceContext (object value)
		{
			this.value = value;
		}
		
		object value;
		
		public object GetRawValue ()
		{
			return value; // so far.
		}
	}

	internal static class TypeExtensionMethods2
	{
		// Note that this returns XamlMember which might not actually appear in XamlObjectReader. For example, XamlLanguage.Items won't be returned when there is no item in the collection.
		public static IEnumerable<XamlMember> GetAllObjectReaderMembersByType (this XamlType type, IValueSerializerContext vsctx)
		{
			if (type.HasPositionalParameters (vsctx)) {
				yield return XamlLanguage.PositionalParameters;
				yield break;
			}

			// Note that if the XamlType has the default constructor, we don't need "Arguments".
			IEnumerable<XamlMember> args = type.ConstructionRequiresArguments ? type.GetSortedConstructorArguments () : null;
			if (args != null && args.Any ())
				yield return XamlLanguage.Arguments;

			if (type.IsContentValue (vsctx)) {
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

			if (xm.IsAttachable)
				return xobj.GetRawValue (); // attachable property value

			// FIXME: this looks like an ugly hack. Is this really true? What if there's MarkupExtension that uses another MarkupExtension type as a member type.
			var obj = xobj.Context.GetRawValue ();
			if (xm == XamlLanguage.Initialization)
				return obj;
			if (xm == XamlLanguage.Items) // collection itself.
				return obj;
			if (xm == XamlLanguage.Arguments) // object itself
				return obj;
			if (xm == XamlLanguage.PositionalParameters)
				return xobj.GetRawValue (); // dummy value
			return xm.Invoker.GetValue (xobj.GetRawValue ());
		}
	}
}
