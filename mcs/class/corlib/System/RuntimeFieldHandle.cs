//
// System.RuntimeFieldHandle.cs
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//

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

using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.InteropServices;
using System.Runtime.ConstrainedExecution;
using System.Runtime.CompilerServices;

namespace System
{
	[ComVisible (true)]
	[Serializable]
	public struct RuntimeFieldHandle : ISerializable
	{
		IntPtr value;

		internal RuntimeFieldHandle (IntPtr v)
		{
			value = v;
		}

		RuntimeFieldHandle (SerializationInfo info, StreamingContext context)
		{
			if (info == null)
				throw new ArgumentNullException ("info");

			RuntimeFieldInfo mf = ((RuntimeFieldInfo) info.GetValue ("FieldObj", typeof (RuntimeFieldInfo)));
			value = mf.FieldHandle.Value;
			if (value == IntPtr.Zero)
				throw new SerializationException ("Insufficient state.");
		}

		public IntPtr Value {
			get {
				return value;
			}
		}

		internal bool IsNullHandle ()
		{
			return value == IntPtr.Zero;
		}

		public void GetObjectData (SerializationInfo info, StreamingContext context)
		{
			if (info == null)
				throw new ArgumentNullException ("info");

			if (value == IntPtr.Zero)
				throw new SerializationException ("Object fields may not be properly initialized");

			info.AddValue ("FieldObj", (RuntimeFieldInfo) FieldInfo.GetFieldFromHandle (this), typeof (RuntimeFieldInfo));
		}

		[ReliabilityContractAttribute (Consistency.WillNotCorruptState, Cer.Success)]
		public override bool Equals (object obj)
		{
			if (obj == null || GetType () != obj.GetType ())
				return false;

			return value == ((RuntimeFieldHandle)obj).Value;
		}

		[ReliabilityContractAttribute (Consistency.WillNotCorruptState, Cer.Success)]
		public bool Equals (RuntimeFieldHandle handle)
		{
			return value == handle.Value;
		}

		public override int GetHashCode ()
		{
			return value.GetHashCode ();
		}

		public static bool operator == (RuntimeFieldHandle left, RuntimeFieldHandle right)
		{
			return left.Equals (right);
		}

		public static bool operator != (RuntimeFieldHandle left, RuntimeFieldHandle right)
		{
			return !left.Equals (right);
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		static extern void SetValueInternal (FieldInfo fi, object obj, object value);

		internal static void SetValue (RuntimeFieldInfo field, Object obj, Object value, RuntimeType fieldType, FieldAttributes fieldAttr, RuntimeType declaringType, ref bool domainInitialized)
		{
			SetValueInternal (field, obj, value);
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		static unsafe extern internal Object GetValueDirect (RuntimeFieldInfo field, RuntimeType fieldType, void *pTypedRef, RuntimeType contextType);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		static unsafe extern internal void SetValueDirect (RuntimeFieldInfo field, RuntimeType fieldType, void* pTypedRef, Object value, RuntimeType contextType);
	}

}
