//
// MonoCustomAttrs.cs: Hooks into the runtime to get custom attributes for reflection handles
//
// Authors:
// 	Paolo Molaro (lupus@ximian.com)
// 	Gonzalo Paniagua Javier (gonzalo@ximian.com)
// 	Marek Safar (marek.safar@gmail.com)
//
// (c) 2002,2003 Ximian, Inc. (http://www.ximian.com)
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
// Copyright (C) 2013 Xamarin, Inc (http://www.xamarin.com)
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
using System.Reflection;
using System.Collections;
using System.Runtime.CompilerServices;
#if !FULL_AOT_RUNTIME
using System.Reflection.Emit;
#endif

using System.Collections.Generic;

namespace System
{
	static class MonoCustomAttrs
	{
		static Assembly corlib;
		[ThreadStatic]
		static Dictionary<Type, AttributeUsageAttribute> usage_cache;

		/* Treat as user types all corlib types extending System.Type that are not MonoType and TypeBuilder */
		static bool IsUserCattrProvider (object obj)
		{
			Type type = obj as Type;
#if !FULL_AOT_RUNTIME
			if ((type is MonoType) || (type is TypeBuilder))
#else
			if (type is MonoType)
#endif
				return false;
			if ((obj is Type))
				return true;
			if (corlib == null)
				 corlib = typeof (int).Assembly;
			return obj.GetType ().Assembly != corlib;
		}
	
		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		internal static extern object[] GetCustomAttributesInternal (ICustomAttributeProvider obj, Type attributeType, bool pseudoAttrs);

		internal static object[] GetPseudoCustomAttributes (ICustomAttributeProvider obj, Type attributeType) {
			object[] pseudoAttrs = null;

			/* FIXME: Add other types */
			if (obj is MonoMethod)
				pseudoAttrs = ((MonoMethod)obj).GetPseudoCustomAttributes ();
			else if (obj is FieldInfo)
				pseudoAttrs = ((FieldInfo)obj).GetPseudoCustomAttributes ();
			else if (obj is ParameterInfo)
				pseudoAttrs = ((ParameterInfo)obj).GetPseudoCustomAttributes ();
			else if (obj is Type)
				pseudoAttrs = ((Type)obj).GetPseudoCustomAttributes ();

			if ((attributeType != null) && (pseudoAttrs != null)) {
				for (int i = 0; i < pseudoAttrs.Length; ++i)
					if (attributeType.IsAssignableFrom (pseudoAttrs [i].GetType ()))
						if (pseudoAttrs.Length == 1)
							return pseudoAttrs;
						else
							return new object [] { pseudoAttrs [i] };
				return EmptyArray<object>.Value;
			}

			return pseudoAttrs;
		}

		internal static object[] GetCustomAttributesBase (ICustomAttributeProvider obj, Type attributeType, bool inheritedOnly)
		{
			object[] attrs;
			if (IsUserCattrProvider (obj))
				attrs = obj.GetCustomAttributes (attributeType, true);
			else
				attrs = GetCustomAttributesInternal (obj, attributeType, false);

			//
			// All pseudo custom attributes are Inherited = false hence we can avoid
			// building attributes array which would be discarded by inherited checks
			//
			if (!inheritedOnly) {
				object[] pseudoAttrs = GetPseudoCustomAttributes (obj, attributeType);
				if (pseudoAttrs != null) {
					object[] res = new object [attrs.Length + pseudoAttrs.Length];
					System.Array.Copy (attrs, res, attrs.Length);
					System.Array.Copy (pseudoAttrs, 0, res, attrs.Length, pseudoAttrs.Length);
					return res;
				}
			}

			return attrs;
		}

		internal static Attribute GetCustomAttribute (ICustomAttributeProvider obj,
								Type attributeType,
								bool inherit)
		{
			object[] res = GetCustomAttributes (obj, attributeType, inherit);
			if (res.Length == 0)
			{
				return null;
			}
			else if (res.Length > 1)
			{
				string msg = "'{0}' has more than one attribute of type '{1}";
				msg = String.Format (msg, obj, attributeType);
				throw new AmbiguousMatchException (msg);
			}

			return (Attribute) res[0];
		}

		internal static object[] GetCustomAttributes (ICustomAttributeProvider obj, Type attributeType, bool inherit)
		{
			if (obj == null)
				throw new ArgumentNullException ("obj");
			if (attributeType == null)
				throw new ArgumentNullException ("attributeType");	

			if (attributeType == typeof (MonoCustomAttrs))
				attributeType = null;
			
			object[] r;
			object[] res = GetCustomAttributesBase (obj, attributeType, false);
			// shortcut
			if (!inherit && res.Length == 1) {
				if (res [0] == null)
					throw new CustomAttributeFormatException ("Invalid custom attribute format");

				if (attributeType != null) {
					if (attributeType.IsAssignableFrom (res[0].GetType ())) {
						r = (object[]) Array.CreateInstance (attributeType, 1);
						r[0] = res[0];
					} else {
						r = (object[]) Array.CreateInstance (attributeType, 0);
					}
				} else {
					r = (object[]) Array.CreateInstance (res[0].GetType (), 1);
					r[0] = res[0];
				}
				return r;
			}

			if (inherit && GetBase (obj) == null)
				inherit = false;

			// if AttributeType is sealed, and Inherited is set to false, then 
			// there's no use in scanning base types 
			if ((attributeType != null && attributeType.IsSealed) && inherit) {
				AttributeUsageAttribute usageAttribute = RetrieveAttributeUsage (
					attributeType);
				if (!usageAttribute.Inherited)
					inherit = false;
			}

			var initialSize = Math.Max (res.Length, 16);
			List<Object> a = null;
			ICustomAttributeProvider btype = obj;
			object[] array;

			/* Non-inherit case */
			if (!inherit) {
				if (attributeType == null) {
					foreach (object attr in res) {
						if (attr == null)
							throw new CustomAttributeFormatException ("Invalid custom attribute format");
					}
					var result = new Attribute [res.Length];
					res.CopyTo (result, 0);
					return result;
				}

				a = new List<object> (initialSize);
				foreach (object attr in res) {
					if (attr == null)
						throw new CustomAttributeFormatException ("Invalid custom attribute format");

					Type attrType = attr.GetType ();
					if (attributeType != null && !attributeType.IsAssignableFrom (attrType))
						continue;
					a.Add (attr);
				}

				if (attributeType == null || attributeType.IsValueType)
					array = new Attribute [a.Count];
				else
					array = Array.CreateInstance (attributeType, a.Count) as object[];
				a.CopyTo (array, 0);
				return array;
			}

			/* Inherit case */
			var attributeInfos = new Dictionary<Type, AttributeInfo> (initialSize);
			int inheritanceLevel = 0;
			a = new List<object> (initialSize);

			do {
				foreach (object attr in res) {
					AttributeUsageAttribute usage;
					if (attr == null)
						throw new CustomAttributeFormatException ("Invalid custom attribute format");

					Type attrType = attr.GetType ();
					if (attributeType != null) {
						if (!attributeType.IsAssignableFrom (attrType))
							continue;
					}

					AttributeInfo firstAttribute;
					if (attributeInfos.TryGetValue (attrType, out firstAttribute))
						usage = firstAttribute.Usage;
					else
						usage = RetrieveAttributeUsage (attrType);

					// only add attribute to the list of attributes if 
					// - we are on the first inheritance level, or the attribute can be inherited anyway
					// and (
					// - multiple attributes of the type are allowed
					// or (
					// - this is the first attribute we've discovered
					// or
					// - the attribute is on same inheritance level than the first 
					//   attribute that was discovered for this attribute type ))
					if ((inheritanceLevel == 0 || usage.Inherited) && (usage.AllowMultiple || 
						(firstAttribute == null || (firstAttribute != null 
							&& firstAttribute.InheritanceLevel == inheritanceLevel))))
						a.Add (attr);

					if (firstAttribute == null)
						attributeInfos.Add (attrType, new AttributeInfo (usage, inheritanceLevel));
				}

				if ((btype = GetBase (btype)) != null) {
					inheritanceLevel++;
					res = GetCustomAttributesBase (btype, attributeType, true);
				}
			} while (inherit && btype != null);

			if (attributeType == null || attributeType.IsValueType)
				array = new Attribute [a.Count];
			else
				array = Array.CreateInstance (attributeType, a.Count) as object[];

			// copy attributes to array
			a.CopyTo (array, 0);

			return array;
		}

		internal static object[] GetCustomAttributes (ICustomAttributeProvider obj, bool inherit)
		{
			if (obj == null)
				throw new ArgumentNullException ("obj");

			if (!inherit)
				return (object[]) GetCustomAttributesBase (obj, null, false).Clone ();

			return GetCustomAttributes (obj, typeof (MonoCustomAttrs), inherit);
		}

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		static extern CustomAttributeData [] GetCustomAttributesDataInternal (ICustomAttributeProvider obj);

		internal static IList<CustomAttributeData> GetCustomAttributesData (ICustomAttributeProvider obj)
		{
			if (obj == null)
				throw new ArgumentNullException ("obj");

			CustomAttributeData [] attrs = GetCustomAttributesDataInternal (obj);
			return Array.AsReadOnly<CustomAttributeData> (attrs);
		}

		internal static bool IsDefined (ICustomAttributeProvider obj, Type attributeType, bool inherit)
		{
			if (attributeType == null)
				throw new ArgumentNullException ("attributeType");

			AttributeUsageAttribute usage = null;
			do {
				if (IsUserCattrProvider (obj))
					return obj.IsDefined (attributeType, inherit);

				if (IsDefinedInternal (obj, attributeType))
					return true;

				object[] pseudoAttrs = GetPseudoCustomAttributes (obj, attributeType);
				if (pseudoAttrs != null) {
					for (int i = 0; i < pseudoAttrs.Length; ++i)
						if (attributeType.IsAssignableFrom (pseudoAttrs[i].GetType ()))
							return true;
				}

				if (usage == null) {
					if (!inherit)
						return false;

					usage = RetrieveAttributeUsage (attributeType);
					if (!usage.Inherited)
						return false;
				}

				obj = GetBase (obj);
			} while (obj != null);

			return false;
		}

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		internal static extern bool IsDefinedInternal (ICustomAttributeProvider obj, Type AttributeType);

		static PropertyInfo GetBasePropertyDefinition (MonoProperty property)
		{
			MethodInfo method = property.GetGetMethod (true);
			if (method == null || !method.IsVirtual)
				method = property.GetSetMethod (true);
			if (method == null || !method.IsVirtual)
				return null;

			MethodInfo baseMethod = method.GetBaseMethod ();
			if (baseMethod != null && baseMethod != method) {
				ParameterInfo[] parameters = property.GetIndexParameters ();
				if (parameters != null && parameters.Length > 0) {
					Type[] paramTypes = new Type[parameters.Length];
					for (int i=0; i < paramTypes.Length; i++)
						paramTypes[i] = parameters[i].ParameterType;
					return baseMethod.DeclaringType.GetProperty (property.Name, property.PropertyType, 
										     paramTypes);
				} else {
					return baseMethod.DeclaringType.GetProperty (property.Name, property.PropertyType);
				}
			}
			return null;

		}

		static EventInfo GetBaseEventDefinition (MonoEvent evt)
		{
			MethodInfo method = evt.GetAddMethod (true);
			if (method == null || !method.IsVirtual)
				method = evt.GetRaiseMethod (true);
			if (method == null || !method.IsVirtual)
				method = evt.GetRemoveMethod (true);
			if (method == null || !method.IsVirtual)
				return null;

			MethodInfo baseMethod = method.GetBaseMethod ();
			if (baseMethod != null && baseMethod != method) {
				BindingFlags flags = method.IsPublic ? BindingFlags.Public : BindingFlags.NonPublic;
				flags |= method.IsStatic ? BindingFlags.Static : BindingFlags.Instance;

				return baseMethod.DeclaringType.GetEvent (evt.Name, flags);
			}
			return null;
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
			if (obj is MonoProperty)
				return GetBasePropertyDefinition ((MonoProperty) obj);
			else if (obj is MonoEvent)
				return GetBaseEventDefinition ((MonoEvent)obj);
			else if (obj is MonoMethod)
				method = (MethodInfo) obj;

			/**
			 * ParameterInfo -> null
			 * Assembly -> null
			 * MonoEvent -> null
			 * MonoField -> null
			 */
			if (method == null || !method.IsVirtual)
				return null;

			MethodInfo baseMethod = method.GetBaseMethod ();
			if (baseMethod == method)
				return null;

			return baseMethod;
		}

		private static AttributeUsageAttribute RetrieveAttributeUsageNoCache (Type attributeType)
		{
			if (attributeType == typeof (AttributeUsageAttribute))
				/* Avoid endless recursion */
				return new AttributeUsageAttribute (AttributeTargets.Class);

			AttributeUsageAttribute usageAttribute = null;
			object[] attribs = GetCustomAttributes (attributeType, typeof(AttributeUsageAttribute), false);
			if (attribs.Length == 0)
			{
				// if no AttributeUsage was defined on the attribute level, then
				// try to retrieve if from its base type
				if (attributeType.BaseType != null)
				{
					usageAttribute = RetrieveAttributeUsage (attributeType.BaseType);

				}
				if (usageAttribute != null)
				{
					// return AttributeUsage of base class
					return usageAttribute;

				}
				// return default AttributeUsageAttribute if no AttributeUsage 
				// was defined on attribute, or its base class
				return DefaultAttributeUsage;
			}
			// check if more than one AttributeUsageAttribute has been specified 
			// on the type
			// NOTE: compilers should prevent this, but that doesn't prevent
			// anyone from using IL ofcourse
			if (attribs.Length > 1)
			{
				throw new FormatException ("Duplicate AttributeUsageAttribute cannot be specified on an attribute type.");
			}

			return ((AttributeUsageAttribute) attribs[0]);
		}

		static AttributeUsageAttribute RetrieveAttributeUsage (Type attributeType)
		{
			AttributeUsageAttribute usageAttribute = null;
			/* Usage a thread-local cache to speed this up, since it is called a lot from GetCustomAttributes () */
			if (usage_cache == null)
				usage_cache = new Dictionary<Type, AttributeUsageAttribute> ();
			if (usage_cache.TryGetValue (attributeType, out usageAttribute))
				return usageAttribute;
			usageAttribute = RetrieveAttributeUsageNoCache (attributeType);
			usage_cache [attributeType] = usageAttribute;
			return usageAttribute;
		}

		private static readonly AttributeUsageAttribute DefaultAttributeUsage =
			new AttributeUsageAttribute (AttributeTargets.All);

		private class AttributeInfo
		{
			private AttributeUsageAttribute _usage;
			private int _inheritanceLevel;

			public AttributeInfo (AttributeUsageAttribute usage, int inheritanceLevel)
			{
				_usage = usage;
				_inheritanceLevel = inheritanceLevel;
			}

			public AttributeUsageAttribute Usage
			{
				get
				{
					return _usage;
				}
			}

			public int InheritanceLevel
			{
				get
				{
					return _inheritanceLevel;
				}
			}
		}
	}
}

