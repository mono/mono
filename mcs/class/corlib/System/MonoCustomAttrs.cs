// System.MonoCustomAttrs.cs
// Hooks into the runtime to get custom attributes for reflection handles
//
// Paolo Molaro (lupus@ximian.com)
//
// (c) 2002 Ximian, Inc.

using System;
using System.Reflection;
using System.Collections;
using System.Runtime.CompilerServices;

namespace System {
	internal class MonoCustomAttrs {

		static Hashtable handle_to_attrs = new Hashtable ();

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		internal static extern object[] GetCustomAttributes (ICustomAttributeProvider obj);

		private static object[] from_cache (ICustomAttributeProvider obj) {
			object[] res = (object[])handle_to_attrs [obj];
			if (res != null)
				return res;
			res = GetCustomAttributes (obj);
			handle_to_attrs.Add (obj, res);
			return res;
		}

		internal static object[] GetCustomAttributes (ICustomAttributeProvider obj, Type attributeType, bool inherit) {
			// handle inherit
			object[] res = from_cache (obj);
			// shortcut
			if (res.Length == 1 && res[0].GetType () == attributeType)
				return res;
			ArrayList a = new ArrayList ();
			foreach (object attr in res) {
				if (attributeType.Equals (attr.GetType ()))
					a.Add (attr);
			}
			return a.ToArray ();
		}

		internal static object[] GetCustomAttributes (ICustomAttributeProvider obj, bool inherit) {
			// handle inherit
			return from_cache (obj);
		}
		internal static bool IsDefined (ICustomAttributeProvider obj, Type attributeType, bool inherit) {
			// handle inherit
			object[] res = from_cache (obj);
			foreach (object attr in res) {
				if (attributeType.Equals (attr.GetType ()))
					return true;
			}
			return false;
		}
	}
}
