using System;
using System.Reflection;
using System.Collections.Generic;

namespace System.Reflection
{
	public static class CustomAttributeExtensions
	{
		static Attribute GetAttr (ICustomAttributeProvider element, Type attributeType, bool inherit) {
			if (element == null)
				throw new ArgumentNullException (nameof (element));
			if (attributeType == null)
				throw new ArgumentNullException (nameof (attributeType));
			var attrs = MonoCustomAttrs.GetCustomAttributes (element, attributeType, inherit);
			if (attrs == null || attrs.Length == 0)
				return null;
			if (attrs.Length > 1)
				throw new AmbiguousMatchException ();
			return (Attribute)attrs [0];
		}

		public static Attribute GetCustomAttribute (this Assembly element, Type attributeType) => GetAttr (element, attributeType, true);

		public static Attribute GetCustomAttribute (this MemberInfo element, Type attributeType) => GetAttr (element, attributeType, true);

		public static Attribute GetCustomAttribute (this MemberInfo element, Type attributeType, bool inherit) => GetAttr (element, attributeType, inherit);

		public static Attribute GetCustomAttribute (this Module element, Type attributeType) => GetAttr (element, attributeType, true);
		public static Attribute GetCustomAttribute (this ParameterInfo element, Type attributeType) => GetAttr (element, attributeType, true);
		public static Attribute GetCustomAttribute (this ParameterInfo element, Type attributeType, bool inherit) => GetAttr (element, attributeType, inherit);

		public static T GetCustomAttribute<T>(this Assembly element) where T : Attribute {
			return (T)GetCustomAttribute (element, typeof (T));
		}

		public static T GetCustomAttribute<T>(this MemberInfo element) where T : Attribute {
			return (T)GetCustomAttribute (element, typeof (T));
		}

		public static T GetCustomAttribute<T>(this MemberInfo element, bool inherit) where T : Attribute {
			return (T)GetCustomAttribute (element, typeof (T), inherit);
		}

		public static T GetCustomAttribute<T>(this Module element) where T : Attribute {
			return (T)GetCustomAttribute (element, typeof (T));
		}

		public static T GetCustomAttribute<T>(this ParameterInfo element) where T : Attribute {
		    return (T)GetCustomAttribute (element, typeof (T));
		}

		public static T GetCustomAttribute<T>(this ParameterInfo element, bool inherit) where T : Attribute {
		    return (T)GetCustomAttribute (element, typeof (T), inherit);
		}

		public static IEnumerable<Attribute> GetCustomAttributes(this Assembly element) {
			return (IEnumerable<Attribute>)MonoCustomAttrs.GetCustomAttributes (element, true);
		}

		public static IEnumerable<Attribute> GetCustomAttributes(this Assembly element, Type attributeType) {
			return (IEnumerable<Attribute>)MonoCustomAttrs.GetCustomAttributes (element, attributeType, true);
		}

		public static IEnumerable<Attribute> GetCustomAttributes(this MemberInfo element) {
			return (IEnumerable<Attribute>)MonoCustomAttrs.GetCustomAttributes (element, true);
		}

		public static IEnumerable<Attribute> GetCustomAttributes(this MemberInfo element, bool inherit) {
			return (IEnumerable<Attribute>)MonoCustomAttrs.GetCustomAttributes (element, inherit);
		}

		public static IEnumerable<Attribute> GetCustomAttributes(this MemberInfo element, Type attributeType) {
			return (IEnumerable<Attribute>)MonoCustomAttrs.GetCustomAttributes (element, attributeType, true);
		}

		public static IEnumerable<Attribute> GetCustomAttributes(this MemberInfo element, Type attributeType, bool inherit) {
			return (IEnumerable<Attribute>)MonoCustomAttrs.GetCustomAttributes (element, attributeType, inherit);
		}

		public static IEnumerable<Attribute> GetCustomAttributes(this Module element) {
			return (IEnumerable<Attribute>)MonoCustomAttrs.GetCustomAttributes (element, true);
		}

		public static IEnumerable<Attribute> GetCustomAttributes(this Module element, Type attributeType) {
			return (IEnumerable<Attribute>)MonoCustomAttrs.GetCustomAttributes (element, attributeType, true);
		}

		public static IEnumerable<Attribute> GetCustomAttributes(this ParameterInfo element) {
			return (IEnumerable<Attribute>)MonoCustomAttrs.GetCustomAttributes (element, true);
		}

		public static IEnumerable<Attribute> GetCustomAttributes(this ParameterInfo element, bool inherit) {
			return (IEnumerable<Attribute>)MonoCustomAttrs.GetCustomAttributes (element, inherit);
		}

		public static IEnumerable<Attribute> GetCustomAttributes(this ParameterInfo element, Type attributeType) {
			return (IEnumerable<Attribute>)MonoCustomAttrs.GetCustomAttributes (element, attributeType, true);
		}

		public static IEnumerable<Attribute> GetCustomAttributes(this ParameterInfo element, Type attributeType, bool inherit) {
			return (IEnumerable<Attribute>)MonoCustomAttrs.GetCustomAttributes (element, attributeType, true);
		}

		public static IEnumerable<T> GetCustomAttributes<T>(this Assembly element) where T : Attribute {
			return (IEnumerable<T>)MonoCustomAttrs.GetCustomAttributes (element, typeof(T), true);
		}

		public static IEnumerable<T> GetCustomAttributes<T>(this MemberInfo element) where T : Attribute {
			return (IEnumerable<T>)MonoCustomAttrs.GetCustomAttributes (element, typeof(T), true);
		}

		public static IEnumerable<T> GetCustomAttributes<T>(this MemberInfo element, bool inherit) where T : Attribute {
			return (IEnumerable<T>)MonoCustomAttrs.GetCustomAttributes (element, typeof(T), inherit);
		}

		public static IEnumerable<T> GetCustomAttributes<T>(this Module element) where T : Attribute {
			return (IEnumerable<T>)MonoCustomAttrs.GetCustomAttributes (element, typeof(T), true);
		}

		public static IEnumerable<T> GetCustomAttributes<T>(this ParameterInfo element) where T : Attribute {
			return (IEnumerable<T>)MonoCustomAttrs.GetCustomAttributes (element, typeof(T), true);
		}

		public static IEnumerable<T> GetCustomAttributes<T>(this ParameterInfo element, bool inherit) where T : Attribute {
			return (IEnumerable<T>)MonoCustomAttrs.GetCustomAttributes (element, typeof(T), inherit);
		}

		public static bool IsDefined (this Assembly element, Type attributeType) => MonoCustomAttrs.IsDefined (element, attributeType, true);

		public static bool IsDefined (this MemberInfo element, Type attributeType) => MonoCustomAttrs.IsDefined (element, attributeType, true);

		public static bool IsDefined (this MemberInfo element, Type attributeType, bool inherit) => MonoCustomAttrs.IsDefined (element, attributeType, inherit);

		public static bool IsDefined (this Module element, Type attributeType) => MonoCustomAttrs.IsDefined (element, attributeType, true);

		public static bool IsDefined (this ParameterInfo element, Type attributeType) => MonoCustomAttrs.IsDefined (element, attributeType, true);

		public static bool IsDefined (this ParameterInfo element, Type attributeType, bool inherit) => MonoCustomAttrs.IsDefined (element, attributeType, true);

	}
}