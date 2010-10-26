//
// System.RuntimeMethodHandle.cs
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
// Copyright (C) 2004-2005 Novell, Inc (http://www.novell.com)
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
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
#if !DISABLE_SECURITY
using System.Security.Permissions;
#endif

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
	public struct RuntimeMethodHandle : ISerializable
	{
		IntPtr value;

		internal RuntimeMethodHandle (IntPtr v)
		{
			value = v;
		}

		RuntimeMethodHandle (SerializationInfo info, StreamingContext context)
		{
			if (info == null)
				throw new ArgumentNullException ("info");

			MonoMethod mm = ((MonoMethod) info.GetValue ("MethodObj", typeof (MonoMethod)));
			value = mm.MethodHandle.Value;
			if (value == IntPtr.Zero)
				throw new SerializationException (Locale.GetText ("Insufficient state."));
		}

		public IntPtr Value {
			get {
				return value;
			}
		}

		// This is from ISerializable
		public void GetObjectData (SerializationInfo info, StreamingContext context)
		{
			if (info == null)
				throw new ArgumentNullException ("info");

			if (value == IntPtr.Zero)
				throw new SerializationException ("Object fields may not be properly initialized");

			info.AddValue ("MethodObj", (MonoMethod) MethodBase.GetMethodFromHandle (this), typeof (MonoMethod));
		}

		[MethodImpl (MethodImplOptions.InternalCall)]
		static extern IntPtr GetFunctionPointer (IntPtr m);

#if !DISABLE_SECURITY
		[SecurityPermission (SecurityAction.Demand, UnmanagedCode = true)]
#endif
		public IntPtr GetFunctionPointer ()
		{
			return GetFunctionPointer (value);
		}

#if NET_2_0
		[ReliabilityContractAttribute (Consistency.WillNotCorruptState, Cer.Success)]
		public override bool Equals (object obj)
		{
			if (obj == null || GetType () != obj.GetType ())
				return false;

			return value == ((RuntimeMethodHandle)obj).Value;
		}

		[ReliabilityContractAttribute (Consistency.WillNotCorruptState, Cer.Success)]
		public bool Equals (RuntimeMethodHandle handle)
		{
			return value == handle.Value;
		}

		public override int GetHashCode ()
		{
			return value.GetHashCode ();
		}

		public static bool operator == (RuntimeMethodHandle left, RuntimeMethodHandle right)
		{
			return left.Equals (right);
		}

		public static bool operator != (RuntimeMethodHandle left, RuntimeMethodHandle right)
		{
			return !left.Equals (right);
		}
#endif
	}
}
