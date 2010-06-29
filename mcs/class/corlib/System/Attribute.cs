//
// System.Attribute.cs
//
// Authors:
//   Miguel de Icaza (miguel@ximian.com) - Original
//   Nick D. Drochak II (ndrochak@gol.com) - Implemented most of the guts
//   Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2002, 2003 Ximian, Inc.  http://www.ximian.com
// Copyright (C) 2004-2005 Novell, Inc (http://www.novell.com)
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

using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;

namespace System
{
	[AttributeUsage (AttributeTargets.All)]
	[Serializable]
	[ComVisible (true)]
	[ComDefaultInterface (typeof (_Attribute))]
	[ClassInterfaceAttribute (ClassInterfaceType.None)]
	public abstract class Attribute : _Attribute {
		protected Attribute ()
		{
		}

		public virtual object TypeId {
			get {
				// Derived classes should override this default behaviour as appropriate
				return this.GetType ();
			}
		}

		private static void CheckParameters (object element, Type attributeType)
		{
			// neither parameter is allowed to be null
			if (element == null)
				throw new ArgumentNullException ("element");

			if (attributeType == null)
				throw new ArgumentNullException ("attributeType");

			if (!typeof (Attribute).IsAssignableFrom (attributeType))
				throw new ArgumentException (Locale.GetText (
					"Type is not derived from System.Attribute."), "attributeType");
		}

		private static Attribute FindAttribute (object[] attributes)
		{
			// if there exists more than one attribute of the given type, throw an exception
			if (attributes.Length > 1) {
				throw new AmbiguousMatchException (Locale.GetText (
					"<element> has more than one attribute of type <attribute_type>"));
			}

			if (attributes.Length < 1)
				return null;

			// tested above for '> 1' and and '< 1', so only '== 1' is left,
			// i.e. we found the attribute
			return (Attribute) attributes[0];
		}

		public static Attribute GetCustomAttribute (ParameterInfo element, Type attributeType)
		{
			return GetCustomAttribute (element, attributeType, true);
		}

		public static Attribute GetCustomAttribute (MemberInfo element, Type attributeType)
		{
			return GetCustomAttribute (element, attributeType, true);
		}

		public static Attribute GetCustomAttribute (Assembly element, Type attributeType)
		{
			return GetCustomAttribute (element, attributeType, true);
		}

		public static Attribute GetCustomAttribute (Module element, Type attributeType)
		{
			return GetCustomAttribute (element, attributeType, true);
		}

		public static Attribute GetCustomAttribute (Module element, Type attributeType, bool inherit)
		{
			// neither parameter is allowed to be null
			CheckParameters (element, attributeType);

			// Module inheritance hierarchies CAN NOT be searched for attributes, so the second
			// parameter of GetCustomAttributes () is IGNORED.
			object[] attributes = element.GetCustomAttributes (attributeType, inherit);

			return FindAttribute (attributes);
		}

		public static Attribute GetCustomAttribute (Assembly element, Type attributeType, bool inherit)
		{
			// neither parameter is allowed to be null
			CheckParameters (element, attributeType);

			// Assembly inheritance hierarchies CAN NOT be searched for attributes, so the second
			// parameter of GetCustomAttributes () is IGNORED.
			object[] attributes = element.GetCustomAttributes (attributeType, inherit);

			return FindAttribute (attributes);
		}

		public static Attribute GetCustomAttribute (ParameterInfo element, Type attributeType, bool inherit)
		{
			// neither parameter is allowed to be null
			CheckParameters (element, attributeType);

			object[] attributes = GetCustomAttributes (element, attributeType, inherit);

			return FindAttribute (attributes);
		}

		public static Attribute GetCustomAttribute (MemberInfo element, Type attributeType, bool inherit)
		{
			// neither parameter is allowed to be null
			CheckParameters (element, attributeType);

			// MemberInfo inheritance hierarchies can be searched for attributes, so the second
			// parameter of GetCustomAttribute () is respected.
			return MonoCustomAttrs.GetCustomAttribute (element, attributeType, inherit);
		}

		public static Attribute[] GetCustomAttributes (Assembly element)
		{
			return GetCustomAttributes (element, true);
		}

		public static Attribute[] GetCustomAttributes (ParameterInfo element)
		{
			return GetCustomAttributes (element, true);
		}

		public static Attribute[] GetCustomAttributes (MemberInfo element)
		{
			return GetCustomAttributes (element, true);
		}

		public static Attribute[] GetCustomAttributes (Module element)
		{
			return GetCustomAttributes (element, true);
		}

		public static Attribute[] GetCustomAttributes (Assembly element, Type attributeType)
		{
			return GetCustomAttributes (element, attributeType, true);
		}

		public static Attribute[] GetCustomAttributes (Module element, Type attributeType)
		{
			return GetCustomAttributes (element, attributeType, true);
		}

		public static Attribute[] GetCustomAttributes (ParameterInfo element, Type attributeType)
		{
			return GetCustomAttributes (element, attributeType, true);
		}

		public static Attribute[] GetCustomAttributes (MemberInfo element, Type type)
		{
			return GetCustomAttributes (element, type, true);
		}

		public static Attribute[] GetCustomAttributes (Assembly element, Type attributeType, bool inherit)
		{
			// element parameter is not allowed to be null
			CheckParameters (element, attributeType);

			return (Attribute []) element.GetCustomAttributes (attributeType, inherit);
		}

		public static Attribute[] GetCustomAttributes (ParameterInfo element, Type attributeType, bool inherit)
		{
			// element parameter is not allowed to be null
			CheckParameters (element, attributeType);

			Attribute [] attributes;
			if (inherit && TryGetParamCustomAttributes (element, attributeType, out attributes))
				return attributes;

			return (Attribute []) element.GetCustomAttributes (attributeType, inherit);
		}

		public static Attribute[] GetCustomAttributes (Module element, Type attributeType, bool inherit)
		{
			// element parameter is not allowed to be null
			CheckParameters (element, attributeType);

			return (Attribute []) element.GetCustomAttributes (attributeType, inherit);
		}

		public static Attribute[] GetCustomAttributes (MemberInfo element, Type type, bool inherit)
		{
			// element parameter is not allowed to be null
			CheckParameters (element, type);

			// MS ignores the inherit param in PropertyInfo's ICustomAttributeProvider 
			// implementation, but not in the Attributes, so directly get the attributes
			// from MonoCustomAttrs instead of going throught the PropertyInfo's 
			// ICustomAttributeProvider
			MemberTypes mtype = element.MemberType;
			if (mtype == MemberTypes.Property)
				return (Attribute []) MonoCustomAttrs.GetCustomAttributes (element, type, inherit);
			return (Attribute []) element.GetCustomAttributes (type, inherit);
		}

		public static Attribute[] GetCustomAttributes (Module element, bool inherit)
		{
			return GetCustomAttributes (element, typeof (Attribute), inherit);
		}

		public static Attribute[] GetCustomAttributes (Assembly element, bool inherit)
		{
			return GetCustomAttributes (element, typeof (Attribute), inherit);
		}

		public static Attribute[] GetCustomAttributes (MemberInfo element, bool inherit)
		{
			return GetCustomAttributes (element, typeof (Attribute), inherit);
		}

		public static Attribute[] GetCustomAttributes (ParameterInfo element, bool inherit)
		{
			// element parameter is not allowed to be null
			CheckParameters (element, typeof (Attribute));

			return GetCustomAttributes (element, typeof (Attribute), inherit);
		}

		public override int GetHashCode ()
		{
			int result = TypeId.GetHashCode ();

			FieldInfo[] fields = GetType ().GetFields (BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
			foreach (FieldInfo field in fields) {
				object value = field.GetValue (this);
				result ^= value == null ? 0 : value.GetHashCode ();
			}
			return result;
		}

		public virtual bool IsDefaultAttribute ()
		{
			// Derived classes should override this default behaviour as appropriate
			return false;
		}

		public static bool IsDefined (Module element, Type attributeType)
		{
			return IsDefined (element, attributeType, false);
		}

		public static bool IsDefined (ParameterInfo element, Type attributeType)
		{
			return IsDefined (element, attributeType, true);
		}

		public static bool IsDefined (MemberInfo element, Type attributeType)
		{
			return IsDefined (element, attributeType, true);
		}

		public static bool IsDefined (Assembly element, Type attributeType)
		{
			return IsDefined (element, attributeType, true);
		}

		public static bool IsDefined (MemberInfo element, Type attributeType, bool inherit)
		{
			CheckParameters (element, attributeType);

			MemberTypes mtype = element.MemberType;
			if (mtype != MemberTypes.Constructor && mtype != MemberTypes.Event &&
				mtype != MemberTypes.Field       && mtype != MemberTypes.Method &&
				mtype != MemberTypes.Property    && mtype != MemberTypes.TypeInfo &&
				mtype != MemberTypes.NestedType)
				throw new NotSupportedException (Locale.GetText (
					"Element is not a constructor, method, property, event, type or field."));
			// MS ignores the inherit param in PropertyInfo's ICustomAttributeProvider 
			// implementation, but not in the Attributes, so directly get the attributes
			// from MonoCustomAttrs instead of going throught the PropertyInfo's 
			// ICustomAttributeProvider
			if (mtype == MemberTypes.Property)
				return MonoCustomAttrs.IsDefined (element, attributeType, inherit);
			return ((MemberInfo) element).IsDefined (attributeType, inherit);
		}

		public static bool IsDefined (Assembly element, Type attributeType, bool inherit)
		{
			CheckParameters (element, attributeType);

			return element.IsDefined (attributeType, inherit);
		}

		public static bool IsDefined (Module element, Type attributeType, bool inherit)
		{
			CheckParameters (element, attributeType);

			return element.IsDefined (attributeType, inherit);
		}

		public static bool IsDefined (ParameterInfo element, Type attributeType, bool inherit)
		{
			CheckParameters (element, attributeType);

			if (element.IsDefined (attributeType, inherit))
				return true;

			if (inherit)
				return IsDefinedOnParameter (element, attributeType);

			return false;
		}

		static bool IsDefinedOnParameter (ParameterInfo parameter, Type attributeType)
		{
			var member = parameter.Member;
			if (member.MemberType != MemberTypes.Method)
				return false;

			var method = ((MethodInfo) member).GetBaseMethod ();

			while (true) {
				var param = method.GetParameters () [parameter.Position];
				if (param.IsDefined (attributeType, false))
					return true;

				var base_method = method.GetBaseMethod ();
				if (base_method == method)
					break;

				method = base_method;
			}

			return false;
		}

		static bool TryGetParamCustomAttributes (ParameterInfo parameter, Type attributeType, out Attribute [] attributes)
		{
			attributes = null;

			if (parameter.Member.MemberType != MemberTypes.Method)
				return false;

			var method = (MethodInfo) parameter.Member;
			var definition = method.GetBaseDefinition ();

			if (method == definition)
				return false;

			var types = new List<Type> ();
			var custom_attributes = new List<Attribute> ();

			while (true) {
				var param = method.GetParameters () [parameter.Position];
				var param_attributes = (Attribute []) param.GetCustomAttributes (attributeType, false);
				foreach (var param_attribute in param_attributes) {
					var param_type = param_attribute.GetType ();
					if (types.Contains (param_type))
						continue;

					types.Add (param_type);
					custom_attributes.Add (param_attribute);
				}

				var base_method = method.GetBaseMethod ();
				if (base_method == method)
					break;

				method = base_method;
			}

			attributes = (Attribute []) Array.CreateInstance (attributeType, custom_attributes.Count);
			custom_attributes.CopyTo (attributes, 0);

			return true;
		}

		public virtual bool Match (object obj)
		{
			// default action is the same as Equals.
			// Derived classes should override as appropriate
			return this.Equals (obj);
		}

		public override bool Equals (object obj)
		{
			if (obj == null || !(obj is Attribute))
				return false;

			//
			// This is needed because Attribute.Equals does a deep
			// compare.  Ran into this with vbnc
			//
			return ValueType.DefaultEquals (this, obj);
		}

		void _Attribute.GetIDsOfNames ([In] ref Guid riid, IntPtr rgszNames, uint cNames, uint lcid, IntPtr rgDispId)
		{
			throw new NotImplementedException ();
		}

		void _Attribute.GetTypeInfo (uint iTInfo, uint lcid, IntPtr ppTInfo)
		{
			throw new NotImplementedException ();
		}

		void _Attribute.GetTypeInfoCount (out uint pcTInfo)
		{
			throw new NotImplementedException ();
		}

		void _Attribute.Invoke (uint dispIdMember, [In] ref Guid riid, uint lcid, short wFlags, IntPtr pDispParams,
			IntPtr pVarResult, IntPtr pExcepInfo, IntPtr puArgErr)
		{
			throw new NotImplementedException ();
		}
	}
}
