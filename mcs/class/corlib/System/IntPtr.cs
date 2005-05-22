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
// Remarks:
//   Requires '/unsafe' compiler option.  This class uses void*,
//   in overloaded constructors, conversion, and cast members in 
//   the public interface.  Using pointers is not valid CLS and 
//   the methods in question have been marked with  the 
//   CLSCompliant attribute that avoid compiler warnings.
//
// FIXME: How do you specify a native int in C#?  I am going to have to do some figuring out
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

using System.Globalization;
using System.Runtime.Serialization;

#if NET_2_0
using System.Runtime.ConstrainedExecution;
#endif

namespace System
{
	[Serializable]
	public unsafe struct IntPtr : ISerializable
	{
		private void *value;

		public static readonly IntPtr Zero;

		public IntPtr (int i32)
		{
			value = (void *) i32;
		}

		public IntPtr (long i64)
		{
			if (((i64 > Int32.MaxValue) || (i64 < Int32.MinValue)) && (IntPtr.Size < 8)) {
				throw new OverflowException (
					Locale.GetText ("This isn't a 64bits machine."));
			}

			value = (void *) i64;
		}

		[CLSCompliant (false)]
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

		void ISerializable.GetObjectData (SerializationInfo info, StreamingContext context)
		{
			if (info == null)
				throw new ArgumentNullException ("info");

			info.AddValue ("value", (long) value);
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

#if NET_2_0
		[ReliabilityContractAttribute (Consistency.WillNotCorruptState, Cer.Success)]
#endif
		public int ToInt32 ()
		{
			return (int) value;
		}

#if NET_2_0
		[ReliabilityContractAttribute (Consistency.WillNotCorruptState, Cer.Success)]
#endif
		public long ToInt64 ()
		{
			return (long) value;
		}

		[CLSCompliant (false)]
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

#if NET_2_0
		[ReliabilityContractAttribute (Consistency.WillNotCorruptState, Cer.Success)]
#endif
		public static bool operator == (IntPtr a, IntPtr b)
		{
			return (a.value == b.value);
		}

#if NET_2_0
		[ReliabilityContractAttribute (Consistency.WillNotCorruptState, Cer.Success)]
#endif
		public static bool operator != (IntPtr a, IntPtr b)
		{
			return (a.value != b.value);
		}

#if NET_2_0
		[ReliabilityContractAttribute (Consistency.WillNotCorruptState, Cer.Success)]
#endif
		public static explicit operator IntPtr (int value)
		{
			return new IntPtr (value);
		}

#if NET_2_0
		[ReliabilityContractAttribute (Consistency.WillNotCorruptState, Cer.Success)]
#endif
		public static explicit operator IntPtr (long value)
		{
			return new IntPtr (value);
		}

#if NET_2_0
		[ReliabilityContractAttribute (Consistency.WillNotCorruptState, Cer.Success)]
#endif		
		[CLSCompliant (false)]
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

		[CLSCompliant (false)]
		unsafe public static explicit operator void * (IntPtr value)
		{
			return value.value;
		}
	}
}
