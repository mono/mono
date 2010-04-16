// System.MonoCustomAttrs.cs
// Hooks into the runtime to get custom attributes for reflection handles
//
// Authors:
// 	Paolo Molaro (lupus@ximian.com)
// 	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (c) 2002,2003 Ximian, Inc. (http://www.ximian.com)
//

//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
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
using System.Reflection.Emit;
using System.Collections.Generic;

namespace System
{
	internal class MonoCustomAttrs
	{
		static Assembly corlib;

		/* Treat as user types all corlib types extending System.Type that are not MonoType and TypeBuilder */
		static bool IsUserCattrProvider (object obj)
		{
			Type type = obj as Type;
			if ((type is MonoType) || (type is TypeBuilder))
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
				return new object [0];
			}
			else
				return pseudoAttrs;
		}

		internal static object[] GetCustomAttributesBase (ICustomAttributeProvider obj, Type attributeType)
		{
			object[] attrs;
			if (IsUserCattrProvider (obj))
				attrs = obj.GetCustomAttributes (attributeType, true);
			else
				attrs = GetCustomAttributesInternal (obj, attributeType, false);

			object[] pseudoAttrs = GetPseudoCustomAttributes (obj, attributeType);
			if (pseudoAttrs != null) {
				object[] res = new object [attrs.Length + pseudoAttrs.Length];
				System.Array.Copy (attrs, res, attrs.Length);
				System.Array.Copy (pseudoAttrs, 0, res, attrs.Length, pseudoAttrs.Length);
				return res;
			}
			else
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
			object[] res = GetCustomAttributesBase (obj, attributeType);
			// shortcut
			if (!inherit && res.Length == 1)
			{
				if (res [0] == null)
					throw new CustomAttributeFormatException ("Invalid custom attribute format");

				if (attributeType != null)
				{
					if (attributeType.IsAssignableFrom (res[0].GetType ()))
					{
						r = (object[]) Array.CreateInstance (attributeType, 1);
						r[0] = res[0];
					}
					else
					{
						r = (object[]) Array.CreateInstance (attributeType, 0);
					}
				}
				else
				{
					r = (object[]) Array.CreateInstance (res[0].GetType (), 1);
					r[0] = res[0];
				}
				return r;
			}

			// if AttributeType is sealed, and Inherited is set to false, then 
			// there's no use in scanning base types 
			if ((attributeType != null && attributeType.IsSealed) && inherit)
			{
				AttributeUsageAttribute usageAttribute = RetrieveAttributeUsage (
					attributeType);
				if (!usageAttribute.Inherited)
				{
					inherit = false;
				}
			}

			int initialSize = res.Length < 16 ? res.Length : 16;

			Hashtable attributeInfos = new Hashtable (initialSize);
			ArrayList a = new ArrayList (initialSize);
			ICustomAttributeProvider btype = obj;

			int inheritanceLevel = 0;

			do
			{
				foreach (object attr in res)
				{
					AttributeUsageAttribute usage;
					if (attr == null)
						throw new CustomAttributeFormatException ("Invalid custom attribute format");

					Type attrType = attr.GetType ();
					if (attributeType != null)
					{
						if (!attributeType.IsAssignableFrom (attrType))
						{
							continue;
						}
					}

					AttributeInfo firstAttribute = (AttributeInfo) attributeInfos[attrType];
					if (firstAttribute != null)
					{
						usage = firstAttribute.Usage;
					}
					else
					{
						usage = RetrieveAttributeUsage (attrType);
					}

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
					{
						a.Add (attr);
					}

					if (firstAttribute == null)
					{
						attributeInfos.Add (attrType, new AttributeInfo (usage, inheritanceLevel));
					}
				}

				if ((btype = GetBase (btype)) != null)
				{
					inheritanceLevel++;
					res = GetCustomAttributesBase (btype, attributeType);
				}
			} while (inherit && btype != null);

			object[] array = null;
			if (attributeType == null || attributeType.IsValueType)
			{
				array = (object[]) Array.CreateInstance (typeof(Attribute), a.Count);
			}
			else
			{
				array = Array.CreateInstance (attributeType, a.Count) as object[];
			}

			// copy attributes to array
			a.CopyTo (array, 0);

			return array;
		}

		internal static object[] GetCustomAttributes (ICustomAttributeProvider obj, bool inherit)
		{
			if (obj == null)
				throw new ArgumentNullException ("obj");

			if (!inherit)
				return (object[]) GetCustomAttributesBase (obj, null).Clone ();

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

			if (IsUserCattrProvider (obj))
				return obj.IsDefined (attributeType, inherit);

			if (IsDefinedInternal (obj, attributeType))
				return true;

			object[] pseudoAttrs = GetPseudoCustomAttributes (obj, attributeType);
			if (pseudoAttrs != null) {
				for (int i = 0; i < pseudoAttrs.Length; ++i)
					if (attributeType.IsAssignableFrom (pseudoAttrs [i].GetType ()))
						return true;
			}

#if ONLY_1_1
			if (inherit) {
				AttributeUsageAttribute usage = RetrieveAttributeUsage (attributeType);
				if (!usage.Inherited)
					inherit = false;
			}
#endif

			// FIXME (bug #82431):
			// on 2.0 profile we should always walk the inheritance
			// chain and base the behavior on the inheritance level:
			//
			// 0  : return true if "attributeType" is assignable from
			// any of the custom attributes
			//
			// > 0: return true if "attributeType" is assignable from
			// any of the custom attributes and AttributeUsageAttribute
			// .Inherited of the assignable attribute is true

			ICustomAttributeProvider btype;
			if (inherit && ((btype = GetBase (obj)) != null))
				return IsDefined (btype, attributeType, inherit);

			return false;
		}

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		internal static extern bool IsDefinedInternal (ICustomAttributeProvider obj, Type AttributeType);

		static PropertyInfo GetBasePropertyDefinition (PropertyInfo property)
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

			MethodInfo baseMethod = method.GetBaseDefinition ();
			if (baseMethod == method)
				return null;

			return baseMethod;
		}

		private static AttributeUsageAttribute RetrieveAttributeUsage (Type attributeType)
		{
			if (attributeType == typeof (AttributeUsageAttribute))
				/* Avoid endless recursion */
				return new AttributeUsageAttribute (AttributeTargets.Class);

			AttributeUsageAttribute usageAttribute = null;
			object[] attribs = GetCustomAttributes (attributeType,
				MonoCustomAttrs.AttributeUsageType, false);
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

		private static readonly Type AttributeUsageType = typeof(AttributeUsageAttribute);
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

