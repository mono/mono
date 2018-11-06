// 
// MacProxy.cs
//  
// Author: Jeffrey Stedfast <jeff@xamarin.com>
// 
// Copyright (c) 2012-2014 Xamarin Inc.
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// 

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using ObjCRuntimeInternal;

namespace Mono
{
	internal class CFType {
		[DllImport (CFObject.CoreFoundationLibrary, EntryPoint="CFGetTypeID")]
		public static extern IntPtr GetTypeID (IntPtr typeRef);
	}

	internal class CFObject : IDisposable, INativeObject
	{
		public const string CoreFoundationLibrary = "/System/Library/Frameworks/CoreFoundation.framework/CoreFoundation";
		const string SystemLibrary = "/usr/lib/libSystem.dylib";

		[DllImport (SystemLibrary)]
		public static extern IntPtr dlopen (string path, int mode);

		[DllImport (SystemLibrary)]
		static extern IntPtr dlsym (IntPtr handle, string symbol);

		[DllImport (SystemLibrary)]
		public static extern void dlclose (IntPtr handle);

		public static IntPtr GetIndirect (IntPtr handle, string symbol)
		{
			return dlsym (handle, symbol);
		}

		public static CFString GetStringConstant (IntPtr handle, string symbol)
		{
			var indirect = dlsym (handle, symbol);
			if (indirect == IntPtr.Zero)
				return null;
			var actual = Marshal.ReadIntPtr (indirect);
			if (actual == IntPtr.Zero)
				return null;
			return new CFString (actual, false);
		}

		public static IntPtr GetIntPtr (IntPtr handle, string symbol)
		{
			var indirect = dlsym (handle, symbol);
			if (indirect == IntPtr.Zero)
				return IntPtr.Zero;
			return Marshal.ReadIntPtr (indirect);
		}

		public static IntPtr GetCFObjectHandle (IntPtr handle, string symbol)
		{
			var indirect = dlsym (handle, symbol);
			if (indirect == IntPtr.Zero)
				return IntPtr.Zero;

			return Marshal.ReadIntPtr (indirect);
		}

		public CFObject (IntPtr handle, bool own)
		{
			Handle = handle;

			if (!own)
				Retain ();
		}

		~CFObject ()
		{
			Dispose (false);
		}

		public IntPtr Handle { get; private set; }

		[DllImport (CoreFoundationLibrary)]
		internal extern static IntPtr CFRetain (IntPtr handle);

		void Retain ()
		{
			CFRetain (Handle);
		}

		[DllImport (CoreFoundationLibrary)]
		internal extern static void CFRelease (IntPtr handle);

		void Release ()
		{
			CFRelease (Handle);
		}

		protected virtual void Dispose (bool disposing)
		{
			if (Handle != IntPtr.Zero) {
				Release ();
				Handle = IntPtr.Zero;
			}
		}

		public void Dispose ()
		{
			Dispose (true);
			GC.SuppressFinalize (this);
		}
	}

	internal class CFArray : CFObject
	{
		public CFArray (IntPtr handle, bool own) : base (handle, own) { }

		[DllImport (CoreFoundationLibrary)]
		extern static IntPtr CFArrayCreate (IntPtr allocator, IntPtr values, /* CFIndex */ IntPtr numValues, IntPtr callbacks);
		static readonly IntPtr kCFTypeArrayCallbacks;

		static CFArray ()
		{
			var handle = dlopen (CoreFoundationLibrary, 0);
			if (handle == IntPtr.Zero)
				return;

			try {
				kCFTypeArrayCallbacks = GetIndirect (handle, "kCFTypeArrayCallBacks");
			} finally {
				dlclose (handle);
			}
		}
		
		public static CFArray FromNativeObjects (params INativeObject[] values)
		{
			return new CFArray (Create (values), true);
		}

		public static unsafe IntPtr Create (params IntPtr[] values)
		{
			if (values == null)
				throw new ArgumentNullException ("values");
			fixed (IntPtr* pv = values) {
				return CFArrayCreate (IntPtr.Zero, (IntPtr) pv, (IntPtr)values.Length, kCFTypeArrayCallbacks);
			}
		}

		internal static unsafe CFArray CreateArray (params IntPtr[] values)
		{
			if (values == null)
				throw new ArgumentNullException ("values");

			fixed (IntPtr *pv = values) {
				IntPtr handle = CFArrayCreate (IntPtr.Zero, (IntPtr) pv, (IntPtr) values.Length, kCFTypeArrayCallbacks);

				return new CFArray (handle, false);
			}
		}
		
		public static CFArray CreateArray (params INativeObject[] values)
		{
			return new CFArray (Create (values), true);
		}

		public static IntPtr Create (params INativeObject[] values)
		{
			if (values == null)
				throw new ArgumentNullException ("values");
			IntPtr[] _values = new IntPtr [values.Length];
			for (int i = 0; i < _values.Length; ++i)
				_values [i] = values [i].Handle;
			return Create (_values);
		}

		[DllImport (CoreFoundationLibrary)]
		extern static /* CFIndex */ IntPtr CFArrayGetCount (IntPtr handle);

		public int Count {
			get { return (int) CFArrayGetCount (Handle); }
		}

		[DllImport (CoreFoundationLibrary)]
		extern static IntPtr CFArrayGetValueAtIndex (IntPtr handle, /* CFIndex */ IntPtr index);

		public IntPtr this[int index] {
			get {
				return CFArrayGetValueAtIndex (Handle, (IntPtr) index);
			}
		}
		
		static public T [] ArrayFromHandle<T> (IntPtr handle, Func<IntPtr, T> creation) where T : class, INativeObject
		{
			if (handle == IntPtr.Zero)
				return null;

			var c = CFArrayGetCount (handle);
			T [] ret = new T [(int)c];

			for (uint i = 0; i < (uint)c; i++) {
				ret [i] = creation (CFArrayGetValueAtIndex (handle, (IntPtr)i));
			}
			return ret;
		}
	}

	internal class CFNumber : CFObject
	{
		public CFNumber (IntPtr handle, bool own) : base (handle, own) { }

		[DllImport (CoreFoundationLibrary)]
		[return: MarshalAs (UnmanagedType.I1)]
		extern static bool CFNumberGetValue (IntPtr handle, /* CFNumberType */ IntPtr type, [MarshalAs (UnmanagedType.I1)] out bool value);

		public static bool AsBool (IntPtr handle)
		{
			bool value;

			if (handle == IntPtr.Zero)
				return false;

			CFNumberGetValue (handle, (IntPtr) 1, out value);

			return value;
		}

		public static implicit operator bool (CFNumber number)
		{
			return AsBool (number.Handle);
		}

		[DllImport (CoreFoundationLibrary)]
		[return: MarshalAs (UnmanagedType.I1)]
		extern static bool CFNumberGetValue (IntPtr handle, /* CFNumberType */ IntPtr type, out int value);

		public static int AsInt32 (IntPtr handle)
		{
			int value;

			if (handle == IntPtr.Zero)
				return 0;

			// 9 == kCFNumberIntType == C int
			CFNumberGetValue (handle, (IntPtr) 9, out value);

			return value;
		}
		
		[DllImport (CoreFoundationLibrary)]
		extern static IntPtr CFNumberCreate (IntPtr allocator, IntPtr theType, IntPtr valuePtr);	

		public static CFNumber FromInt32 (int number)
		{
			// 9 == kCFNumberIntType == C int
			return new CFNumber (CFNumberCreate (IntPtr.Zero, (IntPtr)9, (IntPtr)number), true);
		}

		public static implicit operator int (CFNumber number)
		{
			return AsInt32 (number.Handle);
		}
	}

	internal struct CFRange {
		public IntPtr Location, Length;
		
		public CFRange (int loc, int len)
		{
			Location = (IntPtr) loc;
			Length = (IntPtr) len;
		}
	}

	internal class CFString : CFObject
	{
		string str;

		public CFString (IntPtr handle, bool own) : base (handle, own) { }

		[DllImport (CoreFoundationLibrary)]
		extern static IntPtr CFStringCreateWithCharacters (IntPtr alloc, IntPtr chars, /* CFIndex */ IntPtr length);

		public static CFString Create (string value)
		{
			IntPtr handle;

			unsafe {
				fixed (char *ptr = value) {
					handle = CFStringCreateWithCharacters (IntPtr.Zero, (IntPtr) ptr, (IntPtr) value.Length);
				}
			}

			if (handle == IntPtr.Zero)
				return null;

			return new CFString (handle, true);
		}

		[DllImport (CoreFoundationLibrary)]
		extern static /* CFIndex */ IntPtr CFStringGetLength (IntPtr handle);

		public int Length {
			get {
				if (str != null)
					return str.Length;

				return (int) CFStringGetLength (Handle);
			}
		}
		
		[DllImport (CoreFoundationLibrary)]
		extern static int CFStringCompare (IntPtr theString1, IntPtr theString2, int compareOptions);
		
		public static int Compare (IntPtr string1, IntPtr string2, int compareOptions = 0)
		{
			return CFStringCompare (string1, string2, compareOptions);
		}

		[DllImport (CoreFoundationLibrary)]
		extern static IntPtr CFStringGetCharactersPtr (IntPtr handle);

		[DllImport (CoreFoundationLibrary)]
		extern static IntPtr CFStringGetCharacters (IntPtr handle, CFRange range, IntPtr buffer);

		public static string AsString (IntPtr handle)
		{
			if (handle == IntPtr.Zero)
				return null;
			
			int len = (int) CFStringGetLength (handle);
			
			if (len == 0)
				return string.Empty;
			
			IntPtr chars = CFStringGetCharactersPtr (handle);
			IntPtr buffer = IntPtr.Zero;
			
			if (chars == IntPtr.Zero) {
				CFRange range = new CFRange (0, len);
				buffer = Marshal.AllocHGlobal (len * 2);
				CFStringGetCharacters (handle, range, buffer);
				chars = buffer;
			}

			string str;

			unsafe {
				str = new string ((char *) chars, 0, len);
			}
			
			if (buffer != IntPtr.Zero)
				Marshal.FreeHGlobal (buffer);

			return str;
		}

		public override string ToString ()
		{
			if (str == null)
				str = AsString (Handle);

			return str;
		}

		public static implicit operator string (CFString str)
		{
			return str.ToString ();
		}

		public static implicit operator CFString (string str)
		{
			return Create (str);
		}
	}

	
	internal class CFData : CFObject
	{
		public CFData (IntPtr handle, bool own) : base (handle, own) { }
	
		[DllImport (CoreFoundationLibrary)]
		extern static /* CFDataRef */ IntPtr CFDataCreate (/* CFAllocatorRef */ IntPtr allocator, /* UInt8* */ IntPtr bytes, /* CFIndex */ IntPtr length);
		public unsafe static CFData FromData (byte [] buffer)
		{
			fixed (byte* p = buffer)
			{
				return FromData ((IntPtr)p, (IntPtr)buffer.Length);
			}
		}

		public static CFData FromData (IntPtr buffer, IntPtr length)
		{
			return new CFData (CFDataCreate (IntPtr.Zero, buffer, length), true);
		}
		
		public IntPtr Length {
			get { return CFDataGetLength (Handle); }
		}

		[DllImport (CoreFoundationLibrary)]
		internal extern static /* CFIndex */ IntPtr CFDataGetLength (/* CFDataRef */ IntPtr theData);

		[DllImport (CoreFoundationLibrary)]
		internal extern static /* UInt8* */ IntPtr CFDataGetBytePtr (/* CFDataRef */ IntPtr theData);

		/*
		 * Exposes a read-only pointer to the underlying storage.
		 */
		public IntPtr Bytes {
			get { return CFDataGetBytePtr (Handle); }
		}

		public byte this [long idx] {
			get {
				if (idx < 0 || (ulong) idx > (ulong) Length)
					throw new ArgumentException ("idx");
				return Marshal.ReadByte (new IntPtr (Bytes.ToInt64 () + idx));
			}

			set {
				throw new NotImplementedException ("NSData arrays can not be modified, use an NSMutableData instead");
			}
		}

	}

	internal class CFDictionary : CFObject
	{
		static readonly IntPtr KeyCallbacks;
		static readonly IntPtr ValueCallbacks;
		
		static CFDictionary ()
		{
			var handle = dlopen (CoreFoundationLibrary, 0);
			if (handle == IntPtr.Zero)
				return;

			try {		
				KeyCallbacks = GetIndirect (handle, "kCFTypeDictionaryKeyCallBacks");
				ValueCallbacks = GetIndirect (handle, "kCFTypeDictionaryValueCallBacks");
			} finally {
				dlclose (handle);
			}
		}

		public CFDictionary (IntPtr handle, bool own) : base (handle, own) { }

		public static CFDictionary FromObjectAndKey (IntPtr obj, IntPtr key)
		{
			return new CFDictionary (CFDictionaryCreate (IntPtr.Zero, new IntPtr[] { key }, new IntPtr [] { obj }, (IntPtr)1, KeyCallbacks, ValueCallbacks), true);
		}

		public static CFDictionary FromKeysAndObjects (IList<Tuple<IntPtr,IntPtr>> items)
		{
			var keys = new IntPtr [items.Count];
			var values = new IntPtr [items.Count];
			for (int i = 0; i < items.Count; i++) {
				keys [i] = items [i].Item1;
				values [i] = items [i].Item2;
			}
			return new CFDictionary (CFDictionaryCreate (IntPtr.Zero, keys, values, (IntPtr)items.Count, KeyCallbacks, ValueCallbacks), true);
		}

		[DllImport (CoreFoundationLibrary)]
		extern static IntPtr CFDictionaryCreate (IntPtr allocator, IntPtr[] keys, IntPtr[] vals, IntPtr len, IntPtr keyCallbacks, IntPtr valCallbacks);

		[DllImport (CoreFoundationLibrary)]
		extern static IntPtr CFDictionaryGetValue (IntPtr handle, IntPtr key);

		[DllImport (CoreFoundationLibrary)]
		extern static IntPtr CFDictionaryCreateCopy (IntPtr allocator, IntPtr handle);

		public CFDictionary Copy ()
		{
			return new CFDictionary (CFDictionaryCreateCopy (IntPtr.Zero, Handle), true);
		}
		
		public CFMutableDictionary MutableCopy ()
		{
			return new CFMutableDictionary (CFDictionaryCreateMutableCopy (IntPtr.Zero, IntPtr.Zero, Handle), true);
		}

		[DllImport (CoreFoundationLibrary)]
		extern static IntPtr CFDictionaryCreateMutableCopy (IntPtr allocator, IntPtr capacity, IntPtr theDict);

		public IntPtr GetValue (IntPtr key)
		{
			return CFDictionaryGetValue (Handle, key);
		}

		public IntPtr this[IntPtr key] {
			get {
				return GetValue (key);
			}
		}
	}
	
	internal class CFMutableDictionary : CFDictionary
	{
		public CFMutableDictionary (IntPtr handle, bool own) : base (handle, own) { }

		public void SetValue (IntPtr key, IntPtr val)
		{
			CFDictionarySetValue (Handle, key, val);
		}

		public static CFMutableDictionary Create ()
		{
			var handle = CFDictionaryCreateMutable (IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);
			if (handle == IntPtr.Zero)
				throw new InvalidOperationException ();
			return new CFMutableDictionary (handle, true);
		}

		[DllImport (CoreFoundationLibrary)]
		extern static void CFDictionarySetValue (IntPtr handle, IntPtr key, IntPtr val);

		[DllImport (CoreFoundationLibrary)]
		extern static IntPtr CFDictionaryCreateMutable (IntPtr allocator, IntPtr capacity, IntPtr keyCallback, IntPtr valueCallbacks);

	}

	class CFBoolean : INativeObject, IDisposable {
		IntPtr handle;

		public static readonly CFBoolean True;
		public static readonly CFBoolean False;

		static CFBoolean ()
		{
			var handle = CFObject.dlopen (CFObject.CoreFoundationLibrary, 0);
			if (handle == IntPtr.Zero)
				return;
			try {
				True  = new CFBoolean (CFObject.GetCFObjectHandle (handle, "kCFBooleanTrue"), false);
				False = new CFBoolean (CFObject.GetCFObjectHandle (handle, "kCFBooleanFalse"), false);
			}
			finally {
				CFObject.dlclose (handle);
			}
		}

		internal CFBoolean (IntPtr handle, bool owns)
		{
			this.handle = handle;
			if (!owns)
				CFObject.CFRetain (handle);
		}

		~CFBoolean ()
		{
			Dispose (false);
		}

		public IntPtr Handle {
			get {
				return handle;
			}
		}

		public void Dispose ()
		{
			Dispose (true);
			GC.SuppressFinalize (this);
		}

		protected virtual void Dispose (bool disposing)
		{
			if (handle != IntPtr.Zero){
				CFObject.CFRelease (handle);
				handle = IntPtr.Zero;
			}
		}

		public static implicit operator bool (CFBoolean value)
		{
			return value.Value;
		}

		public static explicit operator CFBoolean (bool value)
		{
			return FromBoolean (value);
		}

		public static CFBoolean FromBoolean (bool value)
		{
			return value ? True : False;
		}

		[DllImport (CFObject.CoreFoundationLibrary)]
		[return: MarshalAs (UnmanagedType.I1)]
		extern static /* Boolean */ bool CFBooleanGetValue (/* CFBooleanRef */ IntPtr boolean);

		public bool Value {
			get {return CFBooleanGetValue (handle);}
		}

		public static bool GetValue (IntPtr boolean)
		{
			return CFBooleanGetValue (boolean);
		}
	}

	internal class CFDate : INativeObject, IDisposable {
		IntPtr handle;

		internal CFDate (IntPtr handle, bool owns)
		{
			this.handle = handle;
			if (!owns)
				CFObject.CFRetain (handle);
		}

		~CFDate ()
		{
			Dispose (false);
		}

		[DllImport (CFObject.CoreFoundationLibrary)]
		extern static IntPtr CFDateCreate (IntPtr allocator, /* CFAbsoluteTime */ double at);

		public static CFDate Create (DateTime date)
		{
			var referenceTime = new DateTime (2001, 1, 1);
			var difference = (date - referenceTime).TotalSeconds;
			var handle = CFDateCreate (IntPtr.Zero, difference);
			if (handle == IntPtr.Zero)
				throw new NotSupportedException ();
			return new CFDate (handle, true);
		}

		public IntPtr Handle {
			get {
				return handle;
			}
		}

		public void Dispose ()
		{
			Dispose (true);
			GC.SuppressFinalize (this);
		}

		protected virtual void Dispose (bool disposing)
		{
			if (handle != IntPtr.Zero) {
				CFObject.CFRelease (handle);
				handle = IntPtr.Zero;
			}
		}

	}

}
