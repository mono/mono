//
// System.IntPtr.cs
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//
// FIXME: How do you specify a native int in C#?  I am going to have to do some figuring out
//
using System;
using System.Runtime.Serialization;

namespace System {
	
	public struct IntPtr : ISerializable {

		public int value;

		public static int Zero = 0;
		
		public IntPtr (int i32)
		{
			value = i32;
		}

		public IntPtr (long i64)
		{
			value = (int) i64;
		}

		unsafe public IntPtr (void *ptr)
		{
			value = (int) ptr;
		}

		unsafe public static int Size {
			get {
				return sizeof (int);
			}
		}

		public void GetObjectData (SerializationInfo si, StreamingContext sc)
		{
			// FIXME: Implement me.
		}
	}
}
