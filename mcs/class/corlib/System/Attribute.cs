//
// System.Attribute.cs
//
// Authors:
//   Miguel de Icaza (miguel@ximian.com) - Original
//   Nick D. Drochak II (ndrochak@gol.com) - Implemented most of the guts
//   Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2002, 2003 Ximian, Inc.  http://www.ximian.com
//

using System.Reflection;

namespace System
{
	[AttributeUsage (AttributeTargets.All)]
	[Serializable]
	public abstract class Attribute
	{
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

			// ParameterInfo inheritance hierarchies CAN NOT be searched for attributes, so the second
			// parameter of GetCustomAttributes () is IGNORED.
			object[] attributes = element.GetCustomAttributes (attributeType, inherit);

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

		public static Attribute[] GetCustomAttributes (MemberInfo element, Type attributeType)
		{
			return GetCustomAttributes (element, attributeType, true);
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

			return (Attribute []) element.GetCustomAttributes (attributeType, inherit);
		}

		public static Attribute[] GetCustomAttributes (Module element, Type attributeType, bool inherit)
		{
			// element parameter is not allowed to be null
			CheckParameters (element, attributeType);

			return (Attribute []) element.GetCustomAttributes (attributeType, inherit);
		}

		public static Attribute[] GetCustomAttributes (MemberInfo element, Type attributeType, bool inherit)
		{
			// element parameter is not allowed to be null
			CheckParameters (element, attributeType);

			return (Attribute []) element.GetCustomAttributes (attributeType, inherit);
		}

		public static Attribute[] GetCustomAttributes (Module element, bool inherit)
		{
			// element parameter is not allowed to be null
			CheckParameters (element, typeof (Attribute));

			return (Attribute []) element.GetCustomAttributes (inherit);
		}

		public static Attribute[] GetCustomAttributes (Assembly element, bool inherit)
		{
			// element parameter is not allowed to be null
			CheckParameters (element, typeof (Attribute));

			return (Attribute []) element.GetCustomAttributes (inherit);
		}

		public static Attribute[] GetCustomAttributes (MemberInfo element, bool inherit)
		{
			// element parameter is not allowed to be null
			CheckParameters (element, typeof (Attribute));

			return (Attribute []) element.GetCustomAttributes (inherit);
		}

		public static Attribute[] GetCustomAttributes (ParameterInfo element, bool inherit)
		{
			// element parameter is not allowed to be null
			CheckParameters (element, typeof (Attribute));

			return (Attribute []) element.GetCustomAttributes (inherit);
		}

		public override int GetHashCode ()
		{
			return base.GetHashCode ();
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

			return IsDefined (element.Member, attributeType, inherit);
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
