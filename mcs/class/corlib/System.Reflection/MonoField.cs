
//
// System.Reflection/MonoField.cs
// The class used to represent Fields from the mono runtime.
//
// Author:
//   Paolo Molaro (lupus@ximian.com)
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com
//

using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace System.Reflection {
	
	internal struct MonoFieldInfo {
		public Type parent;
		public Type type;
		public String name;
		public FieldAttributes attrs;
		
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		internal static extern void get_field_info (MonoField field, out MonoFieldInfo info);
	}

	internal class MonoField : FieldInfo {
		internal IntPtr klass;
		internal RuntimeFieldHandle fhandle;
		
		public override FieldAttributes Attributes {
			get {
				MonoFieldInfo info;
				MonoFieldInfo.get_field_info (this, out info);
				return info.attrs;
			}
		}
		public override RuntimeFieldHandle FieldHandle {
			get {return fhandle;}
		}

		public override Type FieldType { 
			get {
				MonoFieldInfo info;
				MonoFieldInfo.get_field_info (this, out info);
				return info.type;
			}
		}

		public override Type ReflectedType {
			get {
				MonoFieldInfo info;
				MonoFieldInfo.get_field_info (this, out info);
				return info.parent;
			}
		}
		public override Type DeclaringType {
			get {
				MonoFieldInfo info;
				MonoFieldInfo.get_field_info (this, out info);
				return info.parent;
			}
		}
		public override string Name {
			get {
				MonoFieldInfo info;
				MonoFieldInfo.get_field_info (this, out info);
				return info.name;
			}
		}

		public override bool IsDefined (Type attributeType, bool inherit) {
			return MonoCustomAttrs.IsDefined (this, attributeType, inherit);
		}

		public override object[] GetCustomAttributes( bool inherit) {
			return MonoCustomAttrs.GetCustomAttributes (this, inherit);
		}
		public override object[] GetCustomAttributes( Type attributeType, bool inherit) {
			return MonoCustomAttrs.GetCustomAttributes (this, attributeType, inherit);
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern object GetValueInternal (object obj);

		public override object GetValue (object obj)
		{
			return GetValueInternal (obj);
		}

		public override string ToString () {
			MonoFieldInfo info;
			MonoFieldInfo.get_field_info (this, out info);
			return String.Format ("{0} {1}", info.type, info.name);
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private static extern void SetValueInternal (FieldInfo fi, object obj, object value);

		public override void SetValue (object obj, object val, BindingFlags invokeAttr, Binder binder, CultureInfo culture)
		{
			if (!IsStatic && obj == null)
				throw new ArgumentNullException ("obj");
			if (binder == null)
				binder = Binder.DefaultBinder;
			object realval = binder.ChangeType (val, FieldType, culture);
			SetValueInternal (this, obj, realval);
		}
	}
}
