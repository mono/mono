//
// System.RuntimeTypeHandle.cs
//
// Authors:
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

using System.Runtime.Serialization;
using System.Runtime.InteropServices;

#if NET_2_0
using System.Runtime.ConstrainedExecution;
#endif

namespace System
{
#if NET_2_0
	[ComVisible (true)]
#endif
	[MonoTODO ("Serialization needs tests")]
	[Serializable]
	public struct RuntimeTypeHandle : ISerializable
	{
		IntPtr value;

		internal RuntimeTypeHandle (IntPtr val)
		{
			value = val;
		}

		RuntimeTypeHandle (SerializationInfo info, StreamingContext context)
		{
			if (info == null)
				throw new ArgumentNullException ("info");

			MonoType mt = ((MonoType) info.GetValue ("TypeObj", typeof (MonoType)));
			value = mt.TypeHandle.Value;
			if (value == IntPtr.Zero)
				throw new SerializationException (Locale.GetText ("Insufficient state."));
		}

		public IntPtr Value {
			get {
				return value;
			}
		}

		public void GetObjectData (SerializationInfo info, StreamingContext context)
		{
			if (info == null)
				throw new ArgumentNullException ("info");

			if (value == IntPtr.Zero)
				throw new SerializationException ("Object fields may not be properly initialized");

			info.AddValue ("TypeObj", Type.GetTypeHandle (this), typeof (MonoType));
		}

#if NET_2_0
		[ReliabilityContractAttribute (Consistency.WillNotCorruptState, Cer.Success)]
		public override bool Equals (object obj)
		{
			if (obj == null || GetType () != obj.GetType ())
				return false;

			return value == ((RuntimeTypeHandle)obj).Value;
		}

		[ReliabilityContractAttribute (Consistency.WillNotCorruptState, Cer.Success)]
		public bool Equals (RuntimeTypeHandle handle)
		{
			return value == handle.Value;
		}

		public override int GetHashCode ()
		{
			return value.GetHashCode ();
		}

		public static bool operator == (RuntimeTypeHandle left, Object right)
		{
			return (right != null) && (right is RuntimeTypeHandle) && left.Equals ((RuntimeTypeHandle)right);
		}

		public static bool operator != (RuntimeTypeHandle left, Object right)
		{
			return (right == null) || !(right is RuntimeTypeHandle) || !left.Equals ((RuntimeTypeHandle)right);
		}

		public static bool operator == (Object left, RuntimeTypeHandle right)
		{
			return (left != null) && (left is RuntimeTypeHandle) && ((RuntimeTypeHandle)left).Equals (right);
		}

		public static bool operator != (Object left, RuntimeTypeHandle right)
		{
			return (left == null) || !(left is RuntimeTypeHandle) || !((RuntimeTypeHandle)left).Equals (right);
		}

		[CLSCompliant (false)]
		[ReliabilityContractAttribute (Consistency.WillNotCorruptState, Cer.Success)]
		public ModuleHandle GetModuleHandle ()
		{
			// Although MS' runtime is crashing here, we prefer throwing an exception.
			// The check is needed because Type.GetTypeFromHandle returns null
			// for zero handles.
			if (value == IntPtr.Zero)
				throw new InvalidOperationException ("Object fields may not be properly initialized");

			return Type.GetTypeFromHandle (this).Module.ModuleHandle;
		}
#endif
	}
}
