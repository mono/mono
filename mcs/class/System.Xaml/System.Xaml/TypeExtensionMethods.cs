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
using System.Linq;
using System.Reflection;
using System.Windows.Markup;
using System.Xaml.Schema;

namespace System.Xaml
{
	static class TypeExtensionMethods
	{
		public static string GetXamlName (this Type type)
		{
			if (!type.IsNested)
				return type.Name;
			return type.DeclaringType.GetXamlName () + "+" + type.Name;
		}

		public static T GetCustomAttribute<T> (this ICustomAttributeProvider type, bool inherit) where T : Attribute
		{
			foreach (var a in type.GetCustomAttributes (typeof (T), inherit))
				return (T) (object) a;
			return null;
		}

		public static T GetCustomAttribute<T> (this XamlType type) where T : Attribute
		{
			if (type.UnderlyingType == null)
				return null;

			T ret = type.CustomAttributeProvider.GetCustomAttribute<T> (true);
			if (ret != null)
				return ret;
			if (type.BaseType != null)
				return type.BaseType.GetCustomAttribute<T> ();
			return null;
		}

		public static bool ImplementsAnyInterfacesOf (this Type type, params Type [] definitions)
		{
			return definitions.Any (t => ImplementsInterface (type, t));
		}

		public static bool ImplementsInterface (this Type type, Type definition)
		{
			if (type == null)
				throw new ArgumentNullException ("type");
			if (definition == null)
				throw new ArgumentNullException ("definition");

			foreach (var iface in type.GetInterfaces ())
				if (iface == definition || (iface.IsGenericType && iface.GetGenericTypeDefinition () == definition))
					return true;
			return false;
		}

		public static object GetPropertyOrFieldValue (this XamlMember xm, object target)
		{
			// FIXME: consider ValueSerializer etc.
			var mi = xm.UnderlyingMember;
			var fi = mi as FieldInfo;
			if (fi != null)
				return fi.GetValue (target);
			var pi = mi as PropertyInfo;
			if (pi != null)
				return ((PropertyInfo) mi).GetValue (target, null);
			// FIXME: should this be done here??
			if (xm == XamlLanguage.Initialization)
				return target;

			throw new NotImplementedException ();
		}

		public static IEnumerable<XamlMember> GetAllReadWriteMembers (this XamlType type)
		{
			if (type != XamlLanguage.Null) // FIXME: probably by different condition
				yield return XamlLanguage.Initialization;
			foreach (var m in type.GetAllMembers ())
				yield return m;
		}

		public static bool ListEquals (this IList<XamlType> a1, IList<XamlType> a2)
		{
			if (a1 == null)
				return a2 == null;
			if (a2 == null)
				return false;
			if (a1.Count != a2.Count)
				return false;
			for (int i = 0; i < a1.Count; i++)
				if (a1 [i] != a2 [i])
					return false;
			return true;
		}

		public static bool ListEquals (this IEnumerable<XamlTypeName> a1, IEnumerable<XamlTypeName> a2)
		{
			if (a1 == null)
				return a2 == null || !a2.GetEnumerator ().MoveNext ();
			if (a2 == null)
				return false || !a1.GetEnumerator ().MoveNext ();

			var e1 = a1.GetEnumerator ();
			var e2 = a2.GetEnumerator ();
			while (true) {
				if (!e1.MoveNext ())
					return !e2.MoveNext ();
				if (!e2.MoveNext ())
					return false;
				if(!e1.Current.NameEquals (e2.Current))
					return false;
			}
		}
		
		public static bool NameEquals (this XamlType t, XamlTypeName n)
		{
//Console.Error.WriteLine ("**** {0} {1} {2} {3}", t.Name, t.Name == n.Name, t.PreferredXamlNamespace == n.Namespace, ListEquals (t.TypeArguments.ToTypeNames (), n.TypeArguments));
			return t.Name == n.Name && t.PreferredXamlNamespace == n.Namespace && ListEquals (t.TypeArguments.ToTypeNames (), n.TypeArguments);
		}

		public static IEnumerable<XamlTypeName> ToTypeNames (this IEnumerable<XamlType> types)
		{
			if (types != null)
				foreach (var t in types)
					yield return new XamlTypeName (t.PreferredXamlNamespace, t.Name, ToTypeNames (t.TypeArguments));
		}

		public static bool NameEquals (this XamlTypeName n1, XamlTypeName n2)
		{
			if (n1 == null)
				return n2 == null;
			if (n2 == null)
				return false;
			if (n1.Name != n2.Name || n1.Namespace != n2.Namespace || !n1.TypeArguments.ListEquals (n2.TypeArguments))
				return false;
			return true;
		}
	}
}
