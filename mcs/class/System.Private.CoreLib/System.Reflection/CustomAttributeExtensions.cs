using System.Collections.Generic;

namespace System.Reflection
{
	public static class CustomAttributeExtensions
	{
		static System.Attribute GetAttr (ICustomAttributeProvider element, System.Type attributeType, bool inherit) {
			if (element == null)
				throw new ArgumentNullException (nameof (element));
			if (attributeType == null)
				throw new ArgumentNullException (nameof (attributeType));
			var attrs = MonoCustomAttrs.GetCustomAttributes (element, attributeType, inherit);
			if (attrs == null || attrs.Length == 0)
				return null;
			if (attrs.Length > 1)
				throw new AmbiguousMatchException ();
			return (System.Attribute)attrs [0];
		}

		public static System.Attribute GetCustomAttribute (this System.Reflection.Assembly element, System.Type attributeType) => GetAttr (element, attributeType, false);

		public static System.Attribute GetCustomAttribute (this System.Reflection.MemberInfo element, System.Type attributeType) => GetAttr (element, attributeType, false);

		public static System.Attribute GetCustomAttribute (this System.Reflection.MemberInfo element, System.Type attributeType, bool inherit) => GetAttr (element, attributeType, inherit);

		public static System.Attribute GetCustomAttribute (this System.Reflection.Module element, System.Type attributeType) => GetAttr (element, attributeType, false);
		public static System.Attribute GetCustomAttribute (this System.Reflection.ParameterInfo element, System.Type attributeType) => GetAttr (element, attributeType, false);
		public static System.Attribute GetCustomAttribute (this System.Reflection.ParameterInfo element, System.Type attributeType, bool inherit) => GetAttr (element, attributeType, inherit);

		public static T GetCustomAttribute<T>(this System.Reflection.Assembly element) where T : System.Attribute {
			return (T)GetCustomAttribute (element, typeof (T));
		}

		public static T GetCustomAttribute<T>(this System.Reflection.MemberInfo element) where T : System.Attribute {
			return (T)GetCustomAttribute (element, typeof (T));
		}

		public static T GetCustomAttribute<T>(this System.Reflection.MemberInfo element, bool inherit) where T : System.Attribute {
			return (T)GetCustomAttribute (element, typeof (T), inherit);
		}

		public static T GetCustomAttribute<T>(this System.Reflection.Module element) where T : System.Attribute {
			return (T)GetCustomAttribute (element, typeof (T));
		}

		public static T GetCustomAttribute<T>(this System.Reflection.ParameterInfo element) where T : System.Attribute {
		    return (T)GetCustomAttribute (element, typeof (T));
		}

		public static T GetCustomAttribute<T>(this System.Reflection.ParameterInfo element, bool inherit) where T : System.Attribute {
		    return (T)GetCustomAttribute (element, typeof (T), inherit);
		}

		public static System.Collections.Generic.IEnumerable<System.Attribute> GetCustomAttributes(this System.Reflection.Assembly element) { throw new NotImplementedException (); }
		public static System.Collections.Generic.IEnumerable<System.Attribute> GetCustomAttributes(this System.Reflection.Assembly element, System.Type attributeType) { throw new NotImplementedException (); }
		public static System.Collections.Generic.IEnumerable<System.Attribute> GetCustomAttributes(this System.Reflection.MemberInfo element) { throw new NotImplementedException (); }
		public static System.Collections.Generic.IEnumerable<System.Attribute> GetCustomAttributes(this System.Reflection.MemberInfo element, bool inherit) { throw new NotImplementedException (); }
		public static System.Collections.Generic.IEnumerable<System.Attribute> GetCustomAttributes(this System.Reflection.MemberInfo element, System.Type attributeType) { throw new NotImplementedException (); }
		public static System.Collections.Generic.IEnumerable<System.Attribute> GetCustomAttributes(this System.Reflection.MemberInfo element, System.Type attributeType, bool inherit) { throw new NotImplementedException (); }
		public static System.Collections.Generic.IEnumerable<System.Attribute> GetCustomAttributes(this System.Reflection.Module element) { throw new NotImplementedException (); }
		public static System.Collections.Generic.IEnumerable<System.Attribute> GetCustomAttributes(this System.Reflection.Module element, System.Type attributeType) { throw new NotImplementedException (); }
		public static System.Collections.Generic.IEnumerable<System.Attribute> GetCustomAttributes(this System.Reflection.ParameterInfo element) { throw new NotImplementedException (); }
		public static System.Collections.Generic.IEnumerable<System.Attribute> GetCustomAttributes(this System.Reflection.ParameterInfo element, bool inherit) { throw new NotImplementedException (); }
		public static System.Collections.Generic.IEnumerable<System.Attribute> GetCustomAttributes(this System.Reflection.ParameterInfo element, System.Type attributeType) { throw new NotImplementedException (); }
		public static System.Collections.Generic.IEnumerable<System.Attribute> GetCustomAttributes(this System.Reflection.ParameterInfo element, System.Type attributeType, bool inherit) { throw new NotImplementedException (); }
		public static System.Collections.Generic.IEnumerable<T> GetCustomAttributes<T>(this System.Reflection.Assembly element) where T : System.Attribute { throw new NotImplementedException (); }
		public static System.Collections.Generic.IEnumerable<T> GetCustomAttributes<T>(this System.Reflection.MemberInfo element) where T : System.Attribute { throw new NotImplementedException (); }
		public static System.Collections.Generic.IEnumerable<T> GetCustomAttributes<T>(this System.Reflection.MemberInfo element, bool inherit) where T : System.Attribute { throw new NotImplementedException (); }
		public static System.Collections.Generic.IEnumerable<T> GetCustomAttributes<T>(this System.Reflection.Module element) where T : System.Attribute { throw new NotImplementedException (); }
		public static System.Collections.Generic.IEnumerable<T> GetCustomAttributes<T>(this System.Reflection.ParameterInfo element) where T : System.Attribute { throw new NotImplementedException (); }
		public static System.Collections.Generic.IEnumerable<T> GetCustomAttributes<T>(this System.Reflection.ParameterInfo element, bool inherit) where T : System.Attribute { throw new NotImplementedException (); }

		public static bool IsDefined(this System.Reflection.Assembly element, System.Type attributeType) { throw new NotImplementedException (); }

		public static bool IsDefined(this System.Reflection.MemberInfo element, System.Type attributeType) {
			return MonoCustomAttrs.IsDefined (element, attributeType, false);
		}

		public static bool IsDefined(this System.Reflection.MemberInfo element, System.Type attributeType, bool inherit) {
			return MonoCustomAttrs.IsDefined (element, attributeType, inherit);
		}

		public static bool IsDefined(this System.Reflection.Module element, System.Type attributeType) { throw new NotImplementedException (); }
		public static bool IsDefined(this System.Reflection.ParameterInfo element, System.Type attributeType) { throw new NotImplementedException (); }
		public static bool IsDefined(this System.Reflection.ParameterInfo element, System.Type attributeType, bool inherit) { throw new NotImplementedException (); }
	}
}