//
// System.Attribute.cs
//
// Authors:
//   Miguel de Icaza (miguel@ximian.com) - Original
//   Nick D. Drochak II (ndrochak@gol.com) - Implemented most of the guts
//
// (C) Ximian, Inc.  http://www.ximian.com
//

using System;
using System.Globalization;

namespace System {

	[AttributeUsage(AttributeTargets.All)]
	[Serializable]
	public abstract class Attribute {

		protected Attribute ()
		{
		}

		public virtual object TypeId {
			get {
				// Derived classes should override this default behaviour as appropriate
				return this.GetType ();
			}
		}

		private static void CheckParameters (object element, Type attribute_type)
		{
			// neither parameter is allowed to be null
			if (null == element) 
			{
				throw new ArgumentNullException ("element");
			}

			if (null == attribute_type) 
			{
				throw new ArgumentNullException ("attribute_type");
			}
		}

		private static System.Attribute FindAttribute (object[] attributes)
		{
			// if there exists more than one attribute of the given type, throw an exception
			if (attributes.Length > 1) {
				throw new System.Reflection.AmbiguousMatchException (
					Locale.GetText ("<element> has more than one attribute of type <attribute_type>"));
			}

			if (attributes.Length < 1) 
				return null;

			// tested above for '> 1' and and '< 1', so only '== 1' is left,
			// i.e. we found the attribute
			
			return (System.Attribute) attributes[0];
		}

		private static void CheckAncestry (Type attribute_type)
		{
			// attribute_type must be derived from type System.Attribute
			Type t = typeof (System.Attribute);
			
			/* fixme: thgi does not work for target monolib2
			if (!attribute_type.IsSubclassOf (t))
			{
				throw new ArgumentException ("Parameter is not a type derived from System.Attribute", "attribute_type");
			}
			*/
		}

		public static Attribute GetCustomAttribute (System.Reflection.ParameterInfo element,
							    Type attribute_type)
		{
			return GetCustomAttribute (element, attribute_type, true);
		}

		public static Attribute GetCustomAttribute (System.Reflection.MemberInfo element,
							    Type attribute_type)
		{
			return GetCustomAttribute (element, attribute_type, true);
		}

		public static Attribute GetCustomAttribute (System.Reflection.Assembly element,
							    Type attribute_type)
		{
			return GetCustomAttribute (element, attribute_type, true);
		}

		public static Attribute GetCustomAttribute (System.Reflection.Module element,
							    Type attribute_type)
		{
			return GetCustomAttribute (element, attribute_type, true);
		}

		public static Attribute GetCustomAttribute (System.Reflection.Module element,
							    Type attribute_type, bool inherit)
		{
			// neither parameter is allowed to be null
			CheckParameters (element, attribute_type);

			// attribute_type must be derived from type System.Attribute
			CheckAncestry (attribute_type);

			// Module inheritance hierarchies CAN NOT be searched for attributes, so the second
			// parameter of GetCustomAttributes () is INGNORED.
			object[] attributes = element.GetCustomAttributes (attribute_type, inherit);

			return FindAttribute (attributes);
		}

		public static Attribute GetCustomAttribute (System.Reflection.Assembly element,
							    Type attribute_type, bool inherit)
		{
			// neither parameter is allowed to be null
			CheckParameters (element, attribute_type);

			// attribute_type must be derived from type System.Attribute
			CheckAncestry (attribute_type);

			// Assembly inheritance hierarchies CAN NOT be searched for attributes, so the second
			// parameter of GetCustomAttributes () is INGNORED.
			object[] attributes = element.GetCustomAttributes (attribute_type, inherit);

			return FindAttribute (attributes);
		}

		public static Attribute GetCustomAttribute (System.Reflection.ParameterInfo element,
							    Type attribute_type, bool inherit)
		{
			// neither parameter is allowed to be null
			CheckParameters (element, attribute_type);

			// attribute_type must be derived from type System.Attribute
			CheckAncestry (attribute_type);

			// ParameterInfo inheritance hierarchies CAN NOT be searched for attributes, so the second
			// parameter of GetCustomAttributes () is INGNORED.
			object[] attributes = element.GetCustomAttributes (attribute_type, inherit);

			return FindAttribute (attributes);
		}

		public static Attribute GetCustomAttribute (System.Reflection.MemberInfo element,
							    Type attribute_type, bool inherit)
		{
			// neither parameter is allowed to be null
			CheckParameters (element, attribute_type);

			// attribute_type must be derived from type System.Attribute
			CheckAncestry (attribute_type);

			// MemberInfo inheritance hierarchies can be searched for attributes, so the second
			// parameter of GetCustomAttributes () is respected.
			object[] attributes = element.GetCustomAttributes (attribute_type, inherit);

			return FindAttribute (attributes);
		}

		public static Attribute[] GetCustomAttributes (System.Reflection.Assembly element)
		{
			return System.Attribute.GetCustomAttributes (element, true);
		}

		public static Attribute[] GetCustomAttributes (System.Reflection.ParameterInfo element)
		{
			return System.Attribute.GetCustomAttributes (element, true);
		}

		public static Attribute[] GetCustomAttributes (System.Reflection.MemberInfo element){
			return System.Attribute.GetCustomAttributes (element, true);
		}

		public static Attribute[] GetCustomAttributes (System.Reflection.Module element){
			return System.Attribute.GetCustomAttributes (element, true);
		}

		public static Attribute[] GetCustomAttributes (System.Reflection.Assembly element,
							       Type attribute_type)
		{
			return System.Attribute.GetCustomAttributes (element, attribute_type, true);
		}

		public static Attribute[] GetCustomAttributes (System.Reflection.Module element,
							       Type attribute_type)
		{
			return System.Attribute.GetCustomAttributes (element, attribute_type, true);
		}

		public static Attribute[] GetCustomAttributes (System.Reflection.ParameterInfo element,
							       Type attribute_type)
		{
			return System.Attribute.GetCustomAttributes (element, attribute_type, true);
		}

		public static Attribute[] GetCustomAttributes (System.Reflection.MemberInfo element,
							       Type attribute_type)
		{
			return System.Attribute.GetCustomAttributes (element, attribute_type, true);
		}

		public static Attribute[] GetCustomAttributes (System.Reflection.Assembly element,
							       Type attribute_type, bool inherit)
		{
			// element parameter is not allowed to be null
			CheckParameters (element, attribute_type);

			// make a properly typed array to return containing the custom attributes
			System.Attribute[] attributes;

			attributes = (System.Attribute[]) element.GetCustomAttributes (
				attribute_type, inherit);

			return attributes;
		}

		public static Attribute[] GetCustomAttributes (System.Reflection.ParameterInfo element,
							       Type attribute_type, bool inherit)
		{
			// element parameter is not allowed to be null
			CheckParameters (element, attribute_type);

			// make a properly typed array to return containing the custom attributes
			System.Attribute[] attributes = (System.Attribute[]) element.GetCustomAttributes (
				attribute_type, inherit);

			return attributes;
		}

		public static Attribute[] GetCustomAttributes (System.Reflection.Module element,
							       Type attribute_type, bool inherit)
		{
			// element parameter is not allowed to be null
			CheckParameters (element, attribute_type);

			// make a properly typed array to return containing the custom attributes
			System.Attribute[] attributes = (System.Attribute[]) element.GetCustomAttributes (
				attribute_type, inherit);

			return attributes;
		}

		public static Attribute[] GetCustomAttributes (System.Reflection.MemberInfo element,
							       Type attribute_type, bool inherit)
		{
			// element parameter is not allowed to be null
			CheckParameters (element, attribute_type);

			// make a properly typed array to return containing the custom attributes
			System.Attribute[] attributes = (System.Attribute[]) element.GetCustomAttributes (
				attribute_type, inherit);

			return attributes;
		}

		public static Attribute[] GetCustomAttributes (System.Reflection.Module element,
							       bool inherit)
		{
			// element parameter is not allowed to be null
			CheckParameters (element, typeof (System.Attribute));

			// make a properly typed array to return containing the custom attributes
			System.Attribute[] attributes = (System.Attribute[]) element.GetCustomAttributes (
				inherit);

			return attributes;
		}
		
		public static Attribute[] GetCustomAttributes (System.Reflection.Assembly element,
							       bool inherit)
		{
			// element parameter is not allowed to be null
			CheckParameters (element, typeof (System.Attribute));

			// make a properly typed array to return containing the custom attributes
			System.Attribute[] attributes = (System.Attribute[]) element.GetCustomAttributes (
				inherit);

			return attributes;
		}

		public static Attribute[] GetCustomAttributes (System.Reflection.MemberInfo element,
							       bool inherit)
		{
			// element parameter is not allowed to be null
			CheckParameters (element, typeof (System.Attribute));

			// make a properly typed array to return containing the custom attributes
			System.Attribute[] attributes = (System.Attribute[]) element.GetCustomAttributes (
				inherit);

			return attributes;
		}

		public static Attribute[] GetCustomAttributes (System.Reflection.ParameterInfo element,
							       bool inherit)
		{
			// element parameter is not allowed to be null
			CheckParameters (element, typeof (System.Attribute));

			// make a properly typed array to return containing the custom attributes
			System.Attribute[] attributes = (System.Attribute[]) element.GetCustomAttributes (
				inherit);

			return attributes;
		}

		public override int GetHashCode ()
		{
			return ((Object) this).GetHashCode ();
		}
		
		public virtual bool IsDefaultAttribute ()
		{
			// Derived classes should override this default behaviour as appropriate
			return false;
		}
		
		public static bool IsDefined (System.Reflection.Module element, Type attribute_type)
		{
			return (System.Attribute.GetCustomAttributes (element, attribute_type).Length > 0);
		}

		public static bool IsDefined (System.Reflection.ParameterInfo element, Type attribute_type)
		{
			return (System.Attribute.GetCustomAttributes (element, attribute_type).Length > 0);
		}

		public static bool IsDefined (System.Reflection.MemberInfo element, Type attribute_type)
		{
			return (System.Attribute.GetCustomAttributes (element, attribute_type).Length > 0);
		}

		public static bool IsDefined (System.Reflection.Assembly element, Type attribute_type)
		{
			return (System.Attribute.GetCustomAttributes (element, attribute_type).Length > 0);
		}

		public static bool IsDefined (System.Reflection.MemberInfo element, Type attribute_type,
					      bool inherit)
		{
			return (System.Attribute.GetCustomAttributes (
				element, attribute_type, inherit).Length > 0);
		}

		public static bool IsDefined (System.Reflection.Assembly element, Type attribute_type,
					      bool inherit)
		{
			return (System.Attribute.GetCustomAttributes (
				element, attribute_type, inherit).Length > 0);
		}

		public static bool IsDefined (System.Reflection.Module element, Type attribute_type,
					      bool inherit)
		{
			return (System.Attribute.GetCustomAttributes (
				element, attribute_type, inherit).Length > 0);
		}

		public static bool IsDefined (System.Reflection.ParameterInfo element,
					      Type attribute_type, bool inherit)
		{
			return (System.Attribute.GetCustomAttributes (
				element, attribute_type, inherit).Length > 0);
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

			return ((Attribute) obj) == this;
		}
	}
}
