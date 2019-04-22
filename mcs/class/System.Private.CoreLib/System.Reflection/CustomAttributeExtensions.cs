using System;
using System.Reflection;
using System.Collections.Generic;

namespace System.Reflection
{
	public static class CustomAttributeExtensions
	{
		public static Attribute GetCustomAttribute (this Assembly element, Type attributeType) => Attribute.GetCustomAttribute (element, attributeType, true);
		public static Attribute GetCustomAttribute (this MemberInfo element, Type attributeType) => Attribute.GetCustomAttribute (element, attributeType, true);
		public static Attribute GetCustomAttribute (this MemberInfo element, Type attributeType, bool inherit) => Attribute.GetCustomAttribute (element, attributeType, inherit);
		public static Attribute GetCustomAttribute (this Module element, Type attributeType) => Attribute.GetCustomAttribute (element, attributeType, true);
		public static Attribute GetCustomAttribute (this ParameterInfo element, Type attributeType) => Attribute.GetCustomAttribute (element, attributeType, true);
		public static Attribute GetCustomAttribute (this ParameterInfo element, Type attributeType, bool inherit) => Attribute.GetCustomAttribute (element, attributeType, inherit);

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

		public static IEnumerable<Attribute> GetCustomAttributes(this Assembly element) => Attribute.GetCustomAttributes (element, true);
		public static IEnumerable<Attribute> GetCustomAttributes(this Assembly element, Type attributeType) => Attribute.GetCustomAttributes  (element, attributeType, true);
		public static IEnumerable<Attribute> GetCustomAttributes(this MemberInfo element)  => Attribute.GetCustomAttributes  (element, true);
		public static IEnumerable<Attribute> GetCustomAttributes(this MemberInfo element, bool inherit)  => Attribute.GetCustomAttributes (element, inherit);
		public static IEnumerable<Attribute> GetCustomAttributes(this MemberInfo element, Type attributeType)  => Attribute.GetCustomAttributes (element, attributeType, true);
		public static IEnumerable<Attribute> GetCustomAttributes(this MemberInfo element, Type attributeType, bool inherit)  => Attribute.GetCustomAttributes (element, attributeType, inherit);
		public static IEnumerable<Attribute> GetCustomAttributes(this Module element) => Attribute.GetCustomAttributes (element, true);
		public static IEnumerable<Attribute> GetCustomAttributes(this Module element, Type attributeType) => Attribute.GetCustomAttributes (element, attributeType, true);
		public static IEnumerable<Attribute> GetCustomAttributes(this ParameterInfo element) => Attribute.GetCustomAttributes (element, true);
		public static IEnumerable<Attribute> GetCustomAttributes(this ParameterInfo element, bool inherit) => Attribute.GetCustomAttributes (element, inherit);
		public static IEnumerable<Attribute> GetCustomAttributes(this ParameterInfo element, Type attributeType) => Attribute.GetCustomAttributes (element, attributeType, true);
		public static IEnumerable<Attribute> GetCustomAttributes(this ParameterInfo element, Type attributeType, bool inherit) => Attribute.GetCustomAttributes (element, attributeType, true);

		public static IEnumerable<T> GetCustomAttributes<T>(this Assembly element) where T : Attribute {
			return (IEnumerable<T>)GetCustomAttributes (element, typeof(T));
		}

		public static IEnumerable<T> GetCustomAttributes<T>(this MemberInfo element) where T : Attribute {
			return (IEnumerable<T>)GetCustomAttributes (element, typeof(T), true);
		}

		public static IEnumerable<T> GetCustomAttributes<T>(this MemberInfo element, bool inherit) where T : Attribute {
			return (IEnumerable<T>)GetCustomAttributes (element, typeof(T), inherit);
		}

		public static IEnumerable<T> GetCustomAttributes<T>(this Module element) where T : Attribute {
			return (IEnumerable<T>)GetCustomAttributes (element, typeof(T));
		}

		public static IEnumerable<T> GetCustomAttributes<T>(this ParameterInfo element) where T : Attribute {
			return (IEnumerable<T>)GetCustomAttributes (element, typeof(T), true);
		}

		public static IEnumerable<T> GetCustomAttributes<T>(this ParameterInfo element, bool inherit) where T : Attribute {
			return (IEnumerable<T>)GetCustomAttributes (element, typeof(T), inherit);
		}

		public static bool IsDefined (this Assembly element, Type attributeType) => Attribute.IsDefined (element, attributeType, true);

		public static bool IsDefined (this MemberInfo element, Type attributeType) => Attribute.IsDefined (element, attributeType, true);

		public static bool IsDefined (this MemberInfo element, Type attributeType, bool inherit) => Attribute.IsDefined (element, attributeType, inherit);

		public static bool IsDefined (this Module element, Type attributeType) => Attribute.IsDefined (element, attributeType, true);

		public static bool IsDefined (this ParameterInfo element, Type attributeType) => Attribute.IsDefined (element, attributeType, true);

		public static bool IsDefined (this ParameterInfo element, Type attributeType, bool inherit) => Attribute.IsDefined (element, attributeType, true);

	}
}