//
// System.Reflection/PropertyInfo.cs
//
// Author:
//   Paolo Molaro (lupus@ximian.com)
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com
//

using System;
using System.Reflection;
using System.Globalization;

namespace System.Reflection {
	[Serializable]
	public abstract class PropertyInfo : MemberInfo {

		public abstract PropertyAttributes Attributes { get; }
		public abstract bool CanRead { get; }
		public abstract bool CanWrite { get; }

		public bool IsSpecialName {
			get {return (Attributes & PropertyAttributes.SpecialName) != 0;}
		}

		public override MemberTypes MemberType {
			get {return MemberTypes.Property;}
		}
		public abstract Type PropertyType { get; }
	
		protected PropertyInfo () { }

		public MethodInfo[] GetAccessors ()
		{
			return GetAccessors (false);
		}
		
		public abstract MethodInfo[] GetAccessors (bool nonPublic);

		public MethodInfo GetGetMethod()
		{
			return GetGetMethod (false);
		}
		public abstract MethodInfo GetGetMethod(bool nonPublic);
		
		public abstract ParameterInfo[] GetIndexParameters();

		public MethodInfo GetSetMethod()
		{
			return GetSetMethod (false);
		}
		
		public abstract MethodInfo GetSetMethod (bool nonPublic);
		
		public virtual object GetValue (object obj, object[] index)
		{
			return GetValue(obj, BindingFlags.Default, null, index, null);
		}
		
		public abstract object GetValue (object obj, BindingFlags invokeAttr, Binder binder, object[] index, CultureInfo culture);
		
		public virtual void SetValue (object obj, object value, object[] index)
		{
			SetValue (obj, value, BindingFlags.Default, null, index, null);
		}
		
		public abstract void SetValue (object obj, object value, BindingFlags invokeAttr, Binder binder, object[] index, CultureInfo culture);
	}
}
