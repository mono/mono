
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
	
	internal sealed class MonoField : FieldInfo {
		internal IntPtr klass;
		internal RuntimeFieldHandle fhandle;
		string name;
		Type type;
		FieldAttributes attrs;
		
		public override FieldAttributes Attributes {
			get {
				return attrs;
			}
		}
		public override RuntimeFieldHandle FieldHandle {
			get {
				return fhandle;
			}
		}

		public override Type FieldType { 
			get {
				return type;
			}
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern Type GetParentType (bool declaring);

		public override Type ReflectedType {
			get {
				return GetParentType (false);
			}
		}
		public override Type DeclaringType {
			get {
				return GetParentType (true);
			}
		}
		public override string Name {
			get {
				return name;
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
			return String.Format ("{0} {1}", type, name);
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private static extern void SetValueInternal (FieldInfo fi, object obj, object value);

		public override void SetValue (object obj, object val, BindingFlags invokeAttr, Binder binder, CultureInfo culture)
		{
			if (!IsStatic && obj == null)
				throw new ArgumentNullException ("obj");
			if (binder == null)
				binder = Binder.DefaultBinder;
			object realval = binder.ChangeType (val, type, culture);
			SetValueInternal (this, obj, realval);
		}
	}
}
