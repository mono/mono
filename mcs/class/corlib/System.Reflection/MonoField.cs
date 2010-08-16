
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
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace System.Reflection {

	[Serializable]
	internal class MonoField : FieldInfo, ISerializable {
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

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		extern Type ResolveType ();

		public override Type FieldType { 
			get {
				if (type == null)
					type = ResolveType ();
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
			if (!IsStatic) {
				if (obj == null)
					throw new TargetException ("Non-static field requires a target");
				if (!DeclaringType.IsAssignableFrom (obj.GetType ()))
					throw new ArgumentException (string.Format (
						"Field {0} defined on type {1} is not a field on the target object which is of type {2}.",
					 	Name, DeclaringType, obj.GetType ()),
					 	"obj");
			}
			
			if (!IsLiteral)
				CheckGeneric ();
			return GetValueInternal (obj);
		}

		public override string ToString () {
			return String.Format ("{0} {1}", FieldType, name);
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private static extern void SetValueInternal (FieldInfo fi, object obj, object value);

		public override void SetValue (object obj, object val, BindingFlags invokeAttr, Binder binder, CultureInfo culture)
		{
			if (!IsStatic) {
				if (obj == null)
					throw new TargetException ("Non-static field requires a target");
				if (!DeclaringType.IsAssignableFrom (obj.GetType ()))
					throw new ArgumentException (string.Format (
						"Field {0} defined on type {1} is not a field on the target object which is of type {2}.",
					 	Name, DeclaringType, obj.GetType ()),
					 	"obj");
			}
			if (IsLiteral)
				throw new FieldAccessException ("Cannot set a constant field");
			if (binder == null)
				binder = Binder.DefaultBinder;
			CheckGeneric ();
			if (val != null) {
				object newVal;
				newVal = binder.ChangeType (val, FieldType, culture);
				if (newVal == null)
					throw new ArgumentException ("Object type " + val.GetType() + " cannot be converted to target type: " + FieldType, "val");
				val = newVal;
			}
			SetValueInternal (this, obj, val);
		}
		
		internal MonoField Clone (string newName)
		{
			MonoField field = new MonoField ();
			field.name = newName;
			field.type = type;
			field.attrs = attrs;
			field.klass = klass;
			field.fhandle = fhandle;
			return field;
		}

		// ISerializable
		public void GetObjectData (SerializationInfo info, StreamingContext context) 
		{
			MemberInfoSerializationHolder.Serialize (info, Name, ReflectedType,
				ToString(), MemberTypes.Field);
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public override extern object GetRawConstantValue ();

#if NET_4_0
		public override IList<CustomAttributeData> GetCustomAttributesData () {
			return CustomAttributeData.GetCustomAttributes (this);
		}
#endif

		void CheckGeneric () {
			if (DeclaringType.ContainsGenericParameters)
				throw new InvalidOperationException ("Late bound operations cannot be performed on fields with types for which Type.ContainsGenericParameters is true.");
	    }
	}
}
