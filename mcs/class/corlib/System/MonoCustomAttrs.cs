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

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		internal static extern object[] GetCustomAttributes (ICustomAttributeProvider obj);

		internal static Attribute GetCustomAttribute (ICustomAttributeProvider obj,
							      Type attributeType,
							      bool inherit)
		{
			if (obj == null)
				throw new ArgumentNullException ("attribute_type"); // argument name in the caller

			object[] res = GetCustomAttributes (obj);
			ICustomAttributeProvider btype = obj;
			Attribute result = null;

			do {
				foreach (object attr in res) {
					if (!attributeType.IsAssignableFrom (attr.GetType ()))
						continue;

					if (result != null) {
						string msg = "'{0}' has more than one attribute of type '{1}";
						msg = String.Format (msg, obj, attributeType);
						throw new AmbiguousMatchException (msg);
					}
					result = (Attribute) attr;
				}

				if (inherit && (btype = GetBase (btype)) != null)
					res = GetCustomAttributes (btype);

			// Stop when encounters the first one for a given provider.
			} while (inherit && result == null && btype != null);

			return result;
		}

		internal static object[] GetCustomAttributes (ICustomAttributeProvider obj, Type attributeType, bool inherit)
		{
			if (obj == null)
				return (object []) Array.CreateInstance (attributeType, 0);

			object[] r;
			object[] res = GetCustomAttributes (obj);
			// shortcut
			if (!inherit && res.Length == 1) {
				if (attributeType.IsAssignableFrom (res[0].GetType ())) {
					r = (object []) Array.CreateInstance (attributeType, 1);
					r [0] = res [0];
				} else {
					r = (object []) Array.CreateInstance (attributeType, 0);
				}
				return r;
			}

			ArrayList a = new ArrayList (res.Length < 16 ? res.Length : 16);
			ICustomAttributeProvider btype = obj;
			do {
				foreach (object attr in res)
					if (attributeType.IsAssignableFrom (attr.GetType ()))
						a.Add (attr);

				if ((btype = GetBase (btype)) != null)
					res = GetCustomAttributes (btype);
			} while (inherit && btype != null);

			return (object []) a.ToArray (attributeType);
		}

		internal static object [] GetCustomAttributes (ICustomAttributeProvider obj, bool inherit)
		{
			if (obj == null)
				return new object [0]; //FIXME: Should i throw an exception here?

			if (!inherit)
				return (object []) GetCustomAttributes (obj).Clone ();

			ArrayList a = new ArrayList ();
			ICustomAttributeProvider btype = obj;
			a.AddRange (GetCustomAttributes (btype));
			while ((btype = GetBase (btype)) != null)
				a.AddRange (GetCustomAttributes (btype));

			return (object []) a.ToArray (typeof (Attribute));
		}

		internal static bool IsDefined (ICustomAttributeProvider obj, Type attributeType, bool inherit)
		{
			object[] res = GetCustomAttributes (obj);
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

			if (obj is Type)
				return ((Type) obj).BaseType;

			MethodInfo method = null;
			if (obj is MonoProperty) {
				MonoProperty prop = (MonoProperty) obj;
				method = prop.GetGetMethod ();
				if (method == null)
					method = prop.GetSetMethod ();
			} else if (obj is MonoMethod) {
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

