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
// Copyright (c) 2007 Novell, Inc.
//
// Author:
// 	Carlos Alberto Cortez <calberto.cortez@gmail.com>
//

using System;
using System.Collections;
using System.ComponentModel;
using System.Reflection;

#if NET_2_0

using System.Collections.Generic;

namespace System.Windows.Forms
{
	public static class ListBindingHelper 
	{
		public static object GetList (object source)
		{
			return GetList (source, String.Empty);
		}

		public static object GetList (object dataSource, string dataMember)
		{
			if (dataMember == null || dataMember.Length == 0)
				return dataSource is IListSource ? ((IListSource) dataSource).GetList () : dataSource;

			PropertyDescriptor property = GetProperty (dataSource.GetType (), dataMember);
			if (property == null || property.PropertyType != typeof (IList))
				throw new ArgumentException ("dataMember");

			return property.GetValue (dataSource);
		}

		public static Type GetListItemType (object list)
		{
			return GetListItemType (list, String.Empty);
		}

		public static Type GetListItemType (object dataSource, string dataMember)
		{
			if (dataSource == null)
				return null;

			if (dataMember != null && dataMember.Length > 0) {
				PropertyDescriptor property = GetProperty (dataSource.GetType (), dataMember);
				if (property == null)
					return typeof (object);

				return property.PropertyType;
			}

			if (dataSource is Array)
				return dataSource.GetType ().GetElementType ();

			// IEnumerable seems to have higher precedence over IList
			if (dataSource is IEnumerable) {
				IEnumerator enumerator = ((IEnumerable) dataSource).GetEnumerator ();
				if (enumerator.MoveNext ())
					return enumerator.Current.GetType ();

				if (dataSource is IList || dataSource.GetType () == typeof (IList<>)) {
					PropertyInfo property = GetPropertyByReflection (dataSource.GetType (), "Item");
					return property.PropertyType;
				}

				// fallback to object
				return typeof (object);
			}

			return dataSource.GetType ();
		}

		static PropertyDescriptor GetProperty (Type type, string property_name)
		{
			PropertyDescriptorCollection properties = TypeDescriptor.GetProperties (type);
			foreach (PropertyDescriptor prop in properties)
				if (prop.Name == property_name)
					return prop;

			return null;
		}

		// 
		// Need to use reflection as we need to bypass the TypeDescriptor.GetProperties () limitations
		//
		static PropertyInfo GetPropertyByReflection (Type type, string property_name)
		{
			foreach (PropertyInfo prop in type.GetProperties (BindingFlags.Public | BindingFlags.Instance))
				if (prop.Name == property_name)
					return prop;

			return null;
		}
	}
}

#endif
