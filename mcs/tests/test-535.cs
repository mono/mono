//
// Test for conversions that were supposed to be explicit operator
// conversions on UIntPtr and IntPtr, but due to historical reasons
// ended up in the CSC compiler.
//
// See bug http://bugzilla.ximian.com/show_bug.cgi?id=59800 for details
//
// The conversions are:
//   UIntPtr->SByte
//   UIntPtr->Int16
//   UIntPtr->Int32
//   IntPtr->UInt64
//   UInt64->IntPtr
//   SByte->UIntPtr
//   Int16->UIntPtr
//   Int32->UIntPtr
	
using System;
class X {
	public static void Main ()
	{
		UIntPtr a = (UIntPtr) 1;

		// from uintptr
		sbyte _sbyte = (sbyte) a;
		short _short = (short) a;
		int   _int   = (int) a;

		// uint64 to intptr
		IntPtr _intptr = (IntPtr) 1;
		ulong _ulong = (ulong) _intptr;

		// to intptr
		UIntPtr _uptr = (UIntPtr) _sbyte;
		_uptr = (UIntPtr) _short;
		_uptr = (UIntPtr) _int;
	}

	static void Compile ()
	{
		IntPtr a = (IntPtr) 1;
		M (a);
	}
	
	static void M (long l){}
	static void M (UInt64 l){}
	static void M (object o){}
	
}
