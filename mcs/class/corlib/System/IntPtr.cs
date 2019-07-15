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
using System.Runtime.ConstrainedExecution;
using System.Diagnostics.Contracts;

namespace System
{
	[Serializable]
	[System.Runtime.InteropServices.ComVisible (true)]
	public unsafe readonly struct IntPtr : ISerializable, IEquatable<IntPtr>
	{
		private readonly void* m_value;

		public static readonly IntPtr Zero;

		[ReliabilityContract (Consistency.MayCorruptInstance, Cer.MayFail)]
		public IntPtr (int value)
		{
			m_value = (void *) value;
		}

		[ReliabilityContract (Consistency.MayCorruptInstance, Cer.MayFail)]
		public IntPtr (long value)
		{
			/* FIXME: Needs to figure the exact check which works on all architectures */
			/*
			if (((value >> 32 > 0) || (value < 0)) && (IntPtr.Size < 8)) {
				throw new OverflowException (
					Locale.GetText ("This isn't a 64bits machine."));
			}
			*/

			m_value = (void *) value;
		}

		[CLSCompliant (false)]
		[ReliabilityContract (Consistency.MayCorruptInstance, Cer.MayFail)]
		unsafe public IntPtr (void *value)
		{
			m_value = value;
		}

		private IntPtr (SerializationInfo info, StreamingContext context)
		{
			long savedValue = info.GetInt64 ("value");
			m_value = (void *) savedValue;
		}

		public static int Size {
		[ReliabilityContractAttribute (Consistency.WillNotCorruptState, Cer.Success)]
			get {
				return sizeof (void *);
			}
		}

		void ISerializable.GetObjectData (SerializationInfo info, StreamingContext context)
		{
			if (info == null)
				throw new ArgumentNullException ("info");

			info.AddValue ("value", ToInt64 ());
		}

		public override bool Equals (object obj)
		{
			if (!(obj is System.IntPtr))
				return false;

			return ((IntPtr) obj).m_value == m_value;
		}

		public override int GetHashCode ()
		{
			return (int) m_value;
		}

		[ReliabilityContractAttribute (Consistency.WillNotCorruptState, Cer.Success)]
		public int ToInt32 ()
		{
			return (int) m_value;
		}

		[ReliabilityContractAttribute (Consistency.WillNotCorruptState, Cer.Success)]
		public long ToInt64 ()
		{
			// pointer to long conversion is done using conv.u8 by the compiler
			if (Size == 4)
				return (long)(int)m_value;
			else
				return (long)m_value;
		}

		[CLSCompliant (false)]
		[ReliabilityContractAttribute (Consistency.WillNotCorruptState, Cer.Success)]
		unsafe public void *ToPointer ()
		{
			return m_value;
		}

		override public string ToString ()
		{
			return ToString (null);
		}

		public string ToString (string format)
		{
			if (Size == 4)
				return ((int) m_value).ToString (format, null);
			else
				return ((long) m_value).ToString (format, null);
		}

		[ReliabilityContractAttribute (Consistency.WillNotCorruptState, Cer.Success)]
		public static bool operator == (IntPtr value1, IntPtr value2)
		{
			return (value1.m_value == value2.m_value);
		}

		[ReliabilityContractAttribute (Consistency.WillNotCorruptState, Cer.Success)]
		public static bool operator != (IntPtr value1, IntPtr value2)
		{
			return (value1.m_value != value2.m_value);
		}

		[ReliabilityContractAttribute (Consistency.MayCorruptInstance, Cer.MayFail)]
		public static explicit operator IntPtr (int value)
		{
			return new IntPtr (value);
		}

		[ReliabilityContractAttribute (Consistency.MayCorruptInstance, Cer.MayFail)]
		public static explicit operator IntPtr (long value)
		{
			return new IntPtr (value);
		}

		[ReliabilityContractAttribute (Consistency.MayCorruptInstance, Cer.MayFail)]
		[CLSCompliant (false)]
		unsafe public static explicit operator IntPtr (void *value)
		{
			return new IntPtr (value);
		}

		public static explicit operator int (IntPtr value)
		{
			return (int) value.m_value;
		}

		public static explicit operator long (IntPtr value)
		{
			return value.ToInt64 ();
		}

		[CLSCompliant (false)]
		unsafe public static explicit operator void * (IntPtr value)
		{
			return value.m_value;
		}

		[ReliabilityContract (Consistency.MayCorruptInstance, Cer.MayFail)]
		public static IntPtr Add (IntPtr pointer, int offset)
		{
			return (IntPtr) (unchecked (((byte *) pointer) + offset));
		}

		[ReliabilityContract (Consistency.MayCorruptInstance, Cer.MayFail)]
		public static IntPtr Subtract (IntPtr pointer, int offset)
		{
			return (IntPtr) (unchecked (((byte *) pointer) - offset));
		}

		[ReliabilityContract (Consistency.MayCorruptInstance, Cer.MayFail)]
		public static IntPtr operator + (IntPtr pointer, int offset)
		{
			return (IntPtr) (unchecked (((byte *) pointer) + offset));
		}

		[ReliabilityContract (Consistency.MayCorruptInstance, Cer.MayFail)]
		public static IntPtr operator - (IntPtr pointer, int offset)
		{
			return (IntPtr) (unchecked (((byte *) pointer) - offset));
		}

		// fast way to compare IntPtr to (IntPtr)0 while IntPtr.Zero doesn't work due to slow statics access
		[Pure]
		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
		internal unsafe bool IsNull()
		{
			return m_value == null;
		}

		bool IEquatable<IntPtr>.Equals(IntPtr other)
		{
			return m_value == other.m_value;
		}
	}
}
