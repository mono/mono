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

		unsafe public void *value;

		public static IntPtr Zero;

		unsafe static IntPtr ()
		{
			Zero.value = (void *) 0;
		}
		
		unsafe public IntPtr (int i32)
		{
			value = (void *) i32;
		}

		unsafe public IntPtr (long i64)
		{
			value = (void *) i64;
		}

		unsafe public IntPtr (void *ptr)
		{
			value = ptr;
		}

		unsafe public static int Size {
			get {
				return sizeof (void *);
			}
		}

		public void GetObjectData (SerializationInfo si, StreamingContext sc)
		{
			// FIXME: Implement me.
		}

		unsafe public override bool Equals (object o)
		{
			if (!(o is System.IntPtr))
				return false;

			return ((IntPtr) o).value == value;
		}

		unsafe public override int GetHashCode ()
		{
			return (int) value;
		}

		unsafe public int ToInt32 ()
		{
			return (int) value;
		}

		unsafe public long ToInt64 ()
		{
			return (long) value;
		}

		unsafe public void *ToPointer ()
		{
			return value;
		}

		unsafe override public string ToString ()
		{
			if (Size == 4)
				return ((int) value).ToString ();
			else
				return ((long) value).ToString ();
		}

		unsafe public static bool operator == (IntPtr a, IntPtr b)
		{
			return (a.value == b.value);
		}

		unsafe public static bool operator != (IntPtr a, IntPtr b)
		{
			return (a.value != b.value);
		}

		unsafe public static explicit operator IntPtr (int value)
		{
			return new IntPtr (value);
		}

		unsafe public static explicit operator IntPtr (long value)
		{
			return new IntPtr (value);
		}
		
		unsafe public static explicit operator IntPtr (void *value)
		{
			return new IntPtr (value);
		}

		unsafe public static explicit operator int (IntPtr value)
		{
			return (int) value.value;
		}

		unsafe public static explicit operator long (IntPtr value)
		{
			return (long) value.value;
		}

		unsafe public static explicit operator void * (IntPtr value)
		{
			return value.value;
		}
	}
}
