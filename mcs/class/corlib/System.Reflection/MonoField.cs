
//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

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
	
	internal class MonoField : FieldInfo {
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
		internal override extern int GetFieldOffset ();

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
			if (val != null) {
				val = binder.ChangeType (val, type, culture);
				if (val == null)
					throw new ArgumentException ("Object type cannot be converted to target type.", "val");
			}
			SetValueInternal (this, obj, val);
		}

#if NET_2_0 || BOOTSTRAP_NET_2_0
		[MonoTODO]
		public override Type[] OptionalCustomModifiers {
			get {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public override Type[] RequiredCustomModifiers {
			get {
				throw new NotImplementedException ();
			}
		}
#endif

#if NET_2_0 || BOOTSTRAP_NET_2_0
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public override extern FieldInfo Mono_GetGenericFieldDefinition ();
#endif
	}
}
