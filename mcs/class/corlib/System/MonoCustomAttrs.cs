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
			object[] r;
			object[] res = from_cache (obj);
			// shortcut
			if (res.Length == 1 && (res[0].GetType () == attributeType || res[0].GetType().IsSubclassOf(attributeType))) {
				r = (object[])Array.CreateInstance (attributeType, 1);
				r [0] = res [0];
				return r;
			}
			ArrayList a = new ArrayList ();
			Type btype = obj as Type;
			do {
				foreach (object attr in res) {
					if (attributeType.Equals (attr.GetType ()) || attr.GetType().IsSubclassOf(attributeType))
						a.Add (attr);
				}
				if (btype != null && ((btype = btype.BaseType) != null)) {
					res = from_cache (btype);
				} else {
					break;
				}
			} while (inherit && btype != null && ((btype = btype.BaseType) != null));
			r = (object[])Array.CreateInstance (attributeType, a.Count);
			a.CopyTo (r);
			return r;
		}

		internal static object[] GetCustomAttributes (ICustomAttributeProvider obj, bool inherit) {
			Type btype = obj as Type;
			if (!inherit || btype == null) {
				return (Object[])from_cache (obj).Clone ();
			} else {
				ArrayList a = new ArrayList ();
				a.AddRange (from_cache (obj));
				while ((btype = btype.BaseType) != null) {
					a.AddRange (from_cache (btype));
				}
				Attribute[] r = new Attribute [a.Count];
				a.CopyTo (r);
				return (object[])r;
			}
		}
		internal static bool IsDefined (ICustomAttributeProvider obj, Type attributeType, bool inherit) {
			object[] res = from_cache (obj);
			foreach (object attr in res) {
				if (attributeType.Equals (attr.GetType ()))
					return true;
			}
			Type btype = obj as Type;
			if (inherit && (btype != null) && ((btype = btype.BaseType) != null))
				return IsDefined (btype, attributeType, inherit);
			return false;
		}
	}
}
