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
namespace System {

	public abstract class Attribute {

		protected Attribute ()
		{
		}

		public virtual object TypeId {
			get {
				// Derived classes should override this default behaviour as appropriate
				return this.GetType();
			}
		}

		private static void CheckParameters(object element, Type attributeType)
		{
			// neither parameter is allowed to be null
			if (null == element) 
			{
				throw new ArgumentNullException("element");
			}

			if (null == attributeType) 
			{
				throw new ArgumentNullException("attributeType");
			}

		}

		private static System.Attribute FindAttribute(object[] attributes)
		{
			// if there exists more than one attribute of the given type, throw an exception
			if (attributes.Length > 1) 
			{
				throw new System.Reflection.AmbiguousMatchException("<element> has more than one attribute of type <attributeType>");
			}

			if (attributes.Length < 1) 
			{
				return (System.Attribute) null;
			}

			// tested above for '> 1' and and '< 1', so only '== 1' is left, i.e. we found the attribute
			return (System.Attribute) attributes[0];
		}

		private static void CheckAncestry(Type attributeType)
		{
			// attributeType must be derived from type System.Attribute
			Type t = typeof(System.Attribute);
			if (!attributeType.IsSubclassOf(t))
			{
				throw new ArgumentException("Parameter is not a type derived from System.Attribute", "attributeType");
			}
		}

		public static Attribute GetCustomAttribute(System.Reflection.ParameterInfo element, Type attributeType){
			return GetCustomAttribute(element, attributeType, true);
		}

		public static Attribute GetCustomAttribute(System.Reflection.MemberInfo element, Type attributeType){
			return GetCustomAttribute(element, attributeType, true);
		}

		public static Attribute GetCustomAttribute(System.Reflection.Assembly element, Type attributeType){			return GetCustomAttribute(element, attributeType, true);
		}

		public static Attribute GetCustomAttribute(System.Reflection.Module element, Type attributeType){
			return GetCustomAttribute(element, attributeType, true);
		}

		public static Attribute GetCustomAttribute(System.Reflection.Module element, Type attributeType, bool inherit){
			// neither parameter is allowed to be null
			CheckParameters(element, attributeType);

			// attributeType must be derived from type System.Attribute
			CheckAncestry(attributeType);

			// Module inheritance hierarchies CAN NOT be searched for attributes, so the second
			// parameter of GetCustomAttributes() is INGNORED.
			object[] attributes = element.GetCustomAttributes(attributeType, inherit);

			return FindAttribute(attributes);
		}

		public static Attribute GetCustomAttribute(System.Reflection.Assembly element, Type attributeType, bool inherit){
			// neither parameter is allowed to be null
			CheckParameters(element, attributeType);

			// attributeType must be derived from type System.Attribute
			CheckAncestry(attributeType);

			// Assembly inheritance hierarchies CAN NOT be searched for attributes, so the second
			// parameter of GetCustomAttributes() is INGNORED.
			object[] attributes = element.GetCustomAttributes(attributeType, inherit);

			return FindAttribute(attributes);
		}
		public static Attribute GetCustomAttribute(System.Reflection.ParameterInfo element, Type attributeType, bool inherit){
			// neither parameter is allowed to be null
			CheckParameters(element, attributeType);

			// attributeType must be derived from type System.Attribute
			CheckAncestry(attributeType);

			// ParameterInfo inheritance hierarchies CAN NOT be searched for attributes, so the second
			// parameter of GetCustomAttributes() is INGNORED.
			object[] attributes = element.GetCustomAttributes(attributeType, inherit);

			return FindAttribute(attributes);
		}
		public static Attribute GetCustomAttribute(System.Reflection.MemberInfo element, Type attributeType, bool inherit){
			// neither parameter is allowed to be null
			CheckParameters(element, attributeType);

			// attributeType must be derived from type System.Attribute
			CheckAncestry(attributeType);

			// MemberInfo inheritance hierarchies can be searched for attributes, so the second
			// parameter of GetCustomAttributes() is respected.
			object[] attributes = element.GetCustomAttributes(attributeType, inherit);

			return FindAttribute(attributes);
		}

		public static Attribute[] GetCustomAttributes(System.Reflection.Assembly element){
			return System.Attribute.GetCustomAttributes(element, true);
		}
		public static Attribute[] GetCustomAttributes(System.Reflection.ParameterInfo element){
			return System.Attribute.GetCustomAttributes(element, true);
		}
		public static Attribute[] GetCustomAttributes(System.Reflection.MemberInfo element){
			return System.Attribute.GetCustomAttributes(element, true);
		}
		public static Attribute[] GetCustomAttributes(System.Reflection.Module element){
			return System.Attribute.GetCustomAttributes(element, true);
		}
		public static Attribute[] GetCustomAttributes(System.Reflection.Assembly element, Type attributeType){
			return System.Attribute.GetCustomAttributes(element, attributeType, true);
		}
		public static Attribute[] GetCustomAttributes(System.Reflection.Module element, Type attributeType){
			return System.Attribute.GetCustomAttributes(element, attributeType, true);
		}
		public static Attribute[] GetCustomAttributes(System.Reflection.ParameterInfo element, Type attributeType){
			return System.Attribute.GetCustomAttributes(element, attributeType, true);
		}
		public static Attribute[] GetCustomAttributes(System.Reflection.MemberInfo element, Type attributeType)
		{
			return System.Attribute.GetCustomAttributes(element, attributeType, true);
		}
		public static Attribute[] GetCustomAttributes(System.Reflection.Assembly element, Type attributeType, bool inherit)
		{
			// element parameter is not allowed to be null
			CheckParameters(element, attributeType);

			// make a properly typed array to return containing the custom attributes
			System.Attribute[] attributes = (System.Attribute[]) element.GetCustomAttributes(attributeType, inherit);

			return attributes;
		}
		public static Attribute[] GetCustomAttributes(System.Reflection.ParameterInfo element, Type attributeType, bool inherit){
			// element parameter is not allowed to be null
			CheckParameters(element, attributeType);

			// make a properly typed array to return containing the custom attributes
			System.Attribute[] attributes = (System.Attribute[]) element.GetCustomAttributes(attributeType, inherit);

			return attributes;
		}
		public static Attribute[] GetCustomAttributes(System.Reflection.Module element, Type attributeType, bool inherit){
			// element parameter is not allowed to be null
			CheckParameters(element, attributeType);

			// make a properly typed array to return containing the custom attributes
			System.Attribute[] attributes = (System.Attribute[]) element.GetCustomAttributes(attributeType, inherit);

			return attributes;
		}
		public static Attribute[] GetCustomAttributes(System.Reflection.MemberInfo element, Type attributeType, bool inherit)
		{
			// element parameter is not allowed to be null
			CheckParameters(element, attributeType);

			// make a properly typed array to return containing the custom attributes
			System.Attribute[] attributes = (System.Attribute[]) element.GetCustomAttributes(attributeType, inherit);

			return attributes;
		}
		public static Attribute[] GetCustomAttributes(System.Reflection.Module element, bool inherit)
		{
			// element parameter is not allowed to be null
			CheckParameters(element, typeof(System.Attribute));

			// make a properly typed array to return containing the custom attributes
			System.Attribute[] attributes = (System.Attribute[]) element.GetCustomAttributes(inherit);

			return attributes;
		}
		public static Attribute[] GetCustomAttributes(System.Reflection.Assembly element, bool inherit){
			// element parameter is not allowed to be null
			CheckParameters(element, typeof(System.Attribute));

			// make a properly typed array to return containing the custom attributes
			System.Attribute[] attributes = (System.Attribute[]) element.GetCustomAttributes(inherit);

			return attributes;
		}
		public static Attribute[] GetCustomAttributes(System.Reflection.MemberInfo element, bool inherit){
			// element parameter is not allowed to be null
			CheckParameters(element, typeof(System.Attribute));

			// make a properly typed array to return containing the custom attributes
			System.Attribute[] attributes = (System.Attribute[]) element.GetCustomAttributes(inherit);

			return attributes;
		}
		public static Attribute[] GetCustomAttributes(System.Reflection.ParameterInfo element, bool inherit){
			// element parameter is not allowed to be null
			CheckParameters(element, typeof(System.Attribute));

			// make a properly typed array to return containing the custom attributes
			System.Attribute[] attributes = (System.Attribute[]) element.GetCustomAttributes(inherit);

			return attributes;
		}

		public override int GetHashCode(){
			// TODO: Implement me
			return 0;
		}
		
		public virtual bool IsDefaultAttribute(){
			// Derived classes should override this default behaviour as appropriate
			return false;
		}
		
		public static bool IsDefined(System.Reflection.Module element, Type attributeType){
			return (System.Attribute.GetCustomAttributes(element, attributeType).Length > 0);
		}
		public static bool IsDefined(System.Reflection.ParameterInfo element, Type attributeType){
			return (System.Attribute.GetCustomAttributes(element, attributeType).Length > 0);
		}
		public static bool IsDefined(System.Reflection.MemberInfo element, Type attributeType){
			return (System.Attribute.GetCustomAttributes(element, attributeType).Length > 0);
		}
		public static bool IsDefined(System.Reflection.Assembly element, Type attributeType){
			return (System.Attribute.GetCustomAttributes(element, attributeType).Length > 0);
		}
		public static bool IsDefined(System.Reflection.MemberInfo element, Type attributeType, bool inherit){
			return (System.Attribute.GetCustomAttributes(element, attributeType, inherit).Length > 0);
		}
		public static bool IsDefined(System.Reflection.Assembly element, Type attributeType, bool inherit){
			return (System.Attribute.GetCustomAttributes(element, attributeType, inherit).Length > 0);
		}
		public static bool IsDefined(System.Reflection.Module element, Type attributeType, bool inherit){
			return (System.Attribute.GetCustomAttributes(element, attributeType, inherit).Length > 0);
		}
		public static bool IsDefined(System.Reflection.ParameterInfo element, Type attributeType, bool inherit){
			return (System.Attribute.GetCustomAttributes(element, attributeType, inherit).Length > 0);
		}
		
		public virtual bool Match(object obj){
			// default action is the same as Equals.  Derived classes should override as appropriate
			return this.Equals(obj);
		}

	}

}
