//
// System.Reflection.FieldInfo.cs
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//
// TODO: Mucho left to implement.
//

using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace System.Reflection {

	[Serializable]
	public abstract class FieldInfo : MemberInfo {

		public abstract FieldAttributes Attributes {get;}
		public abstract RuntimeFieldHandle FieldHandle {get;}

		public abstract Type FieldType { get; }

		public abstract object GetValue(object obj);

		public override MemberTypes MemberType {
			get { return MemberTypes.Field;}
		}

		public bool IsLiteral
		{
			get {return (Attributes & FieldAttributes.Literal) != 0;}
		} 

		public bool IsStatic
		{
			get {return (Attributes & FieldAttributes.Static) != 0;}
		} 

		public bool IsInitOnly
		{
			get {return (Attributes & FieldAttributes.InitOnly) != 0;}
		} 
		public Boolean IsPublic
		{ 
			get
			{
				return (Attributes & FieldAttributes.FieldAccessMask) == FieldAttributes.Public;
			}
		}
		public Boolean IsPrivate
		{
			get
			{
				return (Attributes & FieldAttributes.FieldAccessMask) == FieldAttributes.Private;
			}
		}
		public Boolean IsFamily
		{
			get
			{
				return (Attributes & FieldAttributes.FieldAccessMask) == FieldAttributes.Family;
			}
		}
		public Boolean IsAssembly
		{
			get
			{
				return (Attributes & FieldAttributes.FieldAccessMask) == FieldAttributes.Assembly;
			}
		}
		public Boolean IsFamilyAndAssembly
		{
			get {
				return (Attributes & FieldAttributes.FieldAccessMask) == FieldAttributes.FamANDAssem;
			}
		}
		public Boolean IsFamilyOrAssembly
		{
			get
			{
				return (Attributes & FieldAttributes.FieldAccessMask) == FieldAttributes.FamORAssem;
			}
		}
		public Boolean IsPinvokeImpl
		{
			get
			{
				return (Attributes & FieldAttributes.PinvokeImpl) == FieldAttributes.PinvokeImpl;
			}
		}
		public Boolean IsSpecialName
		{
			get
			{
				return (Attributes & FieldAttributes.SpecialName) == FieldAttributes.SpecialName;
			}
		}
		public Boolean IsNotSerialized
		{
			get
			{
				return (Attributes & FieldAttributes.NotSerialized) == FieldAttributes.NotSerialized;
			}
		}

		public abstract void SetValue (object obj, object val, BindingFlags invokeAttr, Binder binder, CultureInfo culture);

		public void SetValue (object obj, object value)
		{
			SetValue (obj, value, 0, null, null);
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private static extern FieldInfo internal_from_handle (RuntimeFieldHandle handle);

		public static FieldInfo GetFieldFromHandle (RuntimeFieldHandle handle)
		{
			return internal_from_handle (handle);
		}
	}
}
