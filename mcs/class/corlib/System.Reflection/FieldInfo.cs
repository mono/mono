//
// System.Reflection.FieldInfo.cs
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
// Copyright (C) 2004-2005 Novell, Inc (http://www.novell.com)
// Copyright 2011 Xamarin Inc (http://www.xamarin.com).
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

using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace System.Reflection {

	[Serializable]
	partial class FieldInfo : MemberInfo {

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private static extern FieldInfo internal_from_handle_type (IntPtr field_handle, IntPtr type_handle);

		public static FieldInfo GetFieldFromHandle (RuntimeFieldHandle handle)
		{
			if (handle.Value == IntPtr.Zero)
				throw new ArgumentException ("The handle is invalid.");
			return internal_from_handle_type (handle.Value, IntPtr.Zero);
		}

		[ComVisible (false)]
		public static FieldInfo GetFieldFromHandle (RuntimeFieldHandle handle, RuntimeTypeHandle declaringType)
		{
			if (handle.Value == IntPtr.Zero)
				throw new ArgumentException ("The handle is invalid.");
			FieldInfo fi = internal_from_handle_type (handle.Value, declaringType.Value);
			if (fi == null)
				throw new ArgumentException ("The field handle and the type handle are incompatible.");
			return fi;
		}

		internal virtual int GetFieldOffset ()
		{
			throw new SystemException ("This method should not be called");
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern MarshalAsAttribute get_marshal_info ();

		internal object[] GetPseudoCustomAttributes ()
		{
			int count = 0;

			if (IsNotSerialized)
				count ++;

			if (DeclaringType.IsExplicitLayout)
				count ++;

			MarshalAsAttribute marshalAs = get_marshal_info ();
			if (marshalAs != null)
				count ++;

			if (count == 0)
				return null;
			object[] attrs = new object [count];
			count = 0;

			if (IsNotSerialized)
				attrs [count ++] = new NonSerializedAttribute ();
			if (DeclaringType.IsExplicitLayout)
				attrs [count ++] = new FieldOffsetAttribute (GetFieldOffset ());
			if (marshalAs != null)
				attrs [count ++] = marshalAs;

			return attrs;
		}

		internal CustomAttributeData[] GetPseudoCustomAttributesData ()
		{
			int count = 0;

			if (IsNotSerialized)
				count++;

			if (DeclaringType.IsExplicitLayout)
				count++;

			MarshalAsAttribute marshalAs = get_marshal_info ();
			if (marshalAs != null)
				count++;

			if (count == 0)
				return null;
			CustomAttributeData[] attrsData = new CustomAttributeData [count];
			count = 0;

			if (IsNotSerialized)
				attrsData [count++] = new CustomAttributeData ((typeof (NonSerializedAttribute)).GetConstructor (Type.EmptyTypes));
			if (DeclaringType.IsExplicitLayout) {
				var ctorArgs = new CustomAttributeTypedArgument[] { new CustomAttributeTypedArgument (typeof (int), GetFieldOffset ()) };
				attrsData [count++] = new CustomAttributeData (
					(typeof (FieldOffsetAttribute)).GetConstructor (new[] { typeof (int) }),
					ctorArgs,
					EmptyArray<CustomAttributeNamedArgument>.Value);
			}

			if (marshalAs != null) {
				var ctorArgs = new CustomAttributeTypedArgument[] { new CustomAttributeTypedArgument (typeof (UnmanagedType), marshalAs.Value) };
				attrsData [count++] = new CustomAttributeData (
					(typeof (MarshalAsAttribute)).GetConstructor (new[] { typeof (UnmanagedType) }),
					ctorArgs,
					EmptyArray<CustomAttributeNamedArgument>.Value);//FIXME Get named params
			}

			return attrsData;
		}

	}
}
