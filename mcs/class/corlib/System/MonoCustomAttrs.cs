// System.MonoCustomAttrs.cs
// Hooks into the runtime to get custom attributes for reflection handles
//
// Authors:
// 	Paolo Molaro (lupus@ximian.com)
// 	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (c) 2002,2003 Ximian, Inc. (http://www.ximian.com)
//

using System;
using System.Reflection;
using System.Collections;
using System.Runtime.CompilerServices;

namespace System {
	internal class MonoCustomAttrs {

		static Hashtable handle_to_attrs = new Hashtable ();

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		internal static extern object[] GetCustomAttributes (ICustomAttributeProvider obj);

		private static object[] from_cache (ICustomAttributeProvider obj)
		{
			object[] res = (object []) handle_to_attrs [obj];
			if (res != null)
				return res;
			res = GetCustomAttributes (obj);
			handle_to_attrs.Add (obj, res);
			return res;
		}

		internal static object[] GetCustomAttributes (ICustomAttributeProvider obj, Type attributeType, bool inherit)
		{
			if (obj == null)
				return new object [0]; //FIXME: Should i throw an exception here?

			object[] r;
			object[] res = from_cache (obj);
			// shortcut
			if (res.Length == 1) {
				if (attributeType.IsAssignableFrom (res[0].GetType ())) {
					r = new object [] {res [0]};
				} else {
					r = new object [0];
				}
				return r;
			}

			ArrayList a = new ArrayList ();
			ICustomAttributeProvider btype = obj;
			do {
				foreach (object attr in res)
					if (attributeType.IsAssignableFrom (attr.GetType ()))
						a.Add (attr);

				if ((btype = GetBase (btype)) != null)
					res = from_cache (btype);
			} while (inherit && btype != null);

			return (object []) a.ToArray (attributeType);
		}

		internal static object [] GetCustomAttributes (ICustomAttributeProvider obj, bool inherit)
		{
			if (obj == null)
				return new object [0]; //FIXME: Should i throw an exception here?

			if (!inherit)
				return (object []) from_cache (obj).Clone ();

			ArrayList a = new ArrayList ();
			ICustomAttributeProvider btype = obj;
			a.AddRange (from_cache (btype));
			while ((btype = GetBase (btype)) != null)
				a.AddRange (from_cache (btype));

			return (object []) a.ToArray (typeof (Attribute));
		}

		internal static bool IsDefined (ICustomAttributeProvider obj, Type attributeType, bool inherit)
		{
			object[] res = from_cache (obj);
			foreach (object attr in res) {
				if (attributeType.Equals (attr.GetType ()))
					return true;
			}

			ICustomAttributeProvider btype = GetBase (obj);
			if (inherit && (btype != null))
				return IsDefined (btype, attributeType, inherit);

			return false;
		}

		// Handles Type, MonoProperty and MonoMethod.
		// The runtime has also cases for MonoEvent, MonoField, Assembly and ParameterInfo,
		// but for those we return null here.
		static ICustomAttributeProvider GetBase (ICustomAttributeProvider obj)
		{
			if (obj == null)
				return null;

			Type t = obj.GetType ();
			if (t == typeof (Type))
				return t.BaseType;

			MethodInfo method = null;
			if (t == typeof (MonoProperty)) {
				MonoProperty prop = (MonoProperty) obj;
				method = prop.GetGetMethod ();
				if (method == null)
					method = prop.GetSetMethod ();
			} else if (t == typeof (MonoMethod)) {
				method = (MethodInfo) obj; 
			}

			/**
			 * ParameterInfo -> null
			 * Assembly -> null
			 * MonoEvent -> null
			 * MonoField -> null
			 */
			if (method == null || !method.IsVirtual)
				return null;

			MethodInfo baseMethod = method.GetBaseDefinition ();
			if (baseMethod == method)
				return null;

			return baseMethod;
		}
	}
}

