//
// System.IntPtr.cs
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// Maintainer:
//	 Michael Lambert, michaellambert@email.com
//
// (C) Ximian, Inc.  http://www.ximian.com
//
// Remarks:			Requires '/unsafe' compiler option.  This class uses void*,
//					in overloaded constructors, conversion, and cast members in 
//					the public interface.  Using pointers is not valid CLS and 
//					the methods in question have been marked with  the 
//					CLSCompliant attribute that avoid compiler warnings.
//
// FIXME: How do you specify a native int in C#?  I am going to have to do some figuring out
//

using System;
using System.Runtime.Serialization;

#if __MonoCS__
#else
[
    assembly: System.CLSCompliant(true)
]
#endif

namespace System {

	[
		CLSCompliant(true)
	]
	[Serializable]
	public unsafe struct IntPtr : ISerializable {

		private void *value;

		public static readonly IntPtr Zero;

		static IntPtr ()
		{
			Zero.value = (void *) 0;
		}
		
		public IntPtr (int i32)
		{
			value = (void *) i32;
		}

		public IntPtr (long i64)
		{
			value = (void *) i64;
		}

		[
			CLSCompliant(false)
		]
		unsafe public IntPtr (void *ptr)
		{
			value = ptr;
		}

		private IntPtr (SerializationInfo info, StreamingContext context)
		{
			long savedValue = info.GetInt64 ("value");
			value = (void *) savedValue;
		}

		public static int Size {
			get {
				return sizeof (void *);
			}
		}

		void ISerializable.GetObjectData (SerializationInfo si, StreamingContext sc)
		{
			if( si == null )
				throw new ArgumentNullException( "si" );
        
			si.AddValue("value", (long) value);
		}

		public override bool Equals (object o)
		{
			if (!(o is System.IntPtr))
				return false;

			return ((IntPtr) o).value == value;
		}

		public override int GetHashCode ()
		{
			return (int) value;
		}

		public int ToInt32 ()
		{
			return (int) value;
		}

		public long ToInt64 ()
		{
			return (long) value;
		}

		[
			CLSCompliant(false)
		]
		unsafe public void *ToPointer ()
		{
			return value;
		}

		override public string ToString ()
		{
			if (Size == 4)
				return ((int) value).ToString ();
			else
				return ((long) value).ToString ();
		}

		public static bool operator == (IntPtr a, IntPtr b)
		{
			return (a.value == b.value);
		}

		public static bool operator != (IntPtr a, IntPtr b)
		{
			return (a.value != b.value);
		}

		public static explicit operator IntPtr (int value)
		{
			return new IntPtr (value);
		}

		public static explicit operator IntPtr (long value)
		{
			return new IntPtr (value);
		}
		
		[
			CLSCompliant(false)
		]
		unsafe public static explicit operator IntPtr (void *value)
		{
			return new IntPtr (value);
		}

		public static explicit operator int (IntPtr value)
		{
			return (int) value.value;
		}

		public static explicit operator long (IntPtr value)
		{
			return (long) value.value;
		}

		[
			CLSCompliant(false)
		]
		unsafe public static explicit operator void * (IntPtr value)
		{
			return value.value;
		}
	}
}
