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
using System.Reflection;
using System.Windows.Markup;

namespace System.Xaml.Schema
{
	public class XamlTypeInvoker
	{
		static readonly XamlTypeInvoker unknown = new XamlTypeInvoker ();
		public static XamlTypeInvoker UnknownInvoker {
			get { return unknown; }
		}

		protected XamlTypeInvoker ()
		{
		}
		
		public XamlTypeInvoker (XamlType type)
		{
			if (type == null)
				throw new ArgumentNullException ("type");
			this.type = type;
		}
		
		XamlType type;

		void ThrowIfUnknown ()
		{
			if (type == null || type.UnderlyingType == null)
				throw new NotSupportedException (String.Format ("Current operation is valid only when the underlying type on a XamlType is known, but it is unknown for '{0}'", type));
		}

		public EventHandler<XamlSetMarkupExtensionEventArgs> SetMarkupExtensionHandler {
			get { return type == null ? null : type.SetMarkupExtensionHandler; }
		}

		public EventHandler<XamlSetTypeConverterEventArgs> SetTypeConverterHandler {
			get { return type == null ? null : type.SetTypeConverterHandler; }
		}

		public virtual void AddToCollection (object instance, object item)
		{
			if (instance == null)
				throw new ArgumentNullException ("instance");
			if (item == null)
				throw new ArgumentNullException ("item");

			var ct = instance.GetType ();
			var xct = type == null ? null : type.SchemaContext.GetXamlType (ct);
			MethodInfo mi = null;

			if (type != null && type.UnderlyingType != null) {
				if (!xct.IsCollection) // not sure why this check is done only when UnderlyingType exists...
					throw new NotSupportedException (String.Format ("Non-collection type '{0}' does not support this operation", xct));
				if (ct.IsAssignableFrom (type.UnderlyingType))
					mi = GetAddMethod (type.SchemaContext.GetXamlType (item.GetType ()));
			}

			if (mi == null) {
				if (ct.IsGenericType) {
					mi = ct.GetMethod ("Add", ct.GetGenericArguments ());
					if (mi == null)
						mi = LookupAddMethod (ct, typeof (ICollection<>).MakeGenericType (ct.GetGenericArguments ()));
				} else {
					mi = ct.GetMethod ("Add", new Type [] {typeof (object)});
					if (mi == null)
						mi = LookupAddMethod (ct, typeof (IList));
				}
			}

			if (mi == null)
				throw new InvalidOperationException (String.Format ("The collection type '{0}' does not have 'Add' method", ct));
			
			mi.Invoke (instance, new object [] {item});
		}

		public virtual void AddToDictionary (object instance, object key, object item)
		{
			if (instance == null)
				throw new ArgumentNullException ("instance");

			var t = instance.GetType ();
			// FIXME: this likely needs similar method lookup to AddToCollection().

			MethodInfo mi = null;
			if (t.IsGenericType) {
				mi = instance.GetType ().GetMethod ("Add", t.GetGenericArguments ());
				if (mi == null)
					mi = LookupAddMethod (t, typeof (IDictionary<,>).MakeGenericType (t.GetGenericArguments ()));
			} else {
				mi = instance.GetType ().GetMethod ("Add", new Type [] {typeof (object), typeof (object)});
				if (mi == null)
					mi = LookupAddMethod (t, typeof (IDictionary));
			}
			mi.Invoke (instance, new object [] {key, item});
		}
		
		MethodInfo LookupAddMethod (Type ct, Type iface)
		{
			var map = ct.GetInterfaceMap (iface);
			for (int i = 0; i < map.TargetMethods.Length; i++)
				if (map.InterfaceMethods [i].Name == "Add")
					return map.TargetMethods [i];
			return null;
		}

		public virtual object CreateInstance (object [] arguments)
		{
			ThrowIfUnknown ();
			return Activator.CreateInstance (type.UnderlyingType, arguments);
		}

		public virtual MethodInfo GetAddMethod (XamlType contentType)
		{
			return type == null || type.UnderlyingType == null || type.ItemType == null || type.LookupCollectionKind () == XamlCollectionKind.None ? null : type.UnderlyingType.GetMethod ("Add", new Type [] {contentType.UnderlyingType});
		}

		public virtual MethodInfo GetEnumeratorMethod ()
		{
			return type.UnderlyingType == null || type.LookupCollectionKind () == XamlCollectionKind.None ? null : type.UnderlyingType.GetMethod ("GetEnumerator");
		}
		
		public virtual IEnumerator GetItems (object instance)
		{
			if (instance == null)
				throw new ArgumentNullException ("instance");
			return ((IEnumerable) instance).GetEnumerator ();
		}
	}
}
