using System;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

namespace Mono {
	internal struct StringMarshalHelper : IDisposable {
		string str;
		IntPtr value;
		
		internal StringMarshalHelper (string str)
		{
			this.str = str;
			this.value = IntPtr.Zero;
		}

		public void Dispose ()
		{
			try {}
			finally {
				if (value != IntPtr.Zero) {
					var tmp = value;
					value = IntPtr.Zero;
					Marshal.FreeHGlobal (tmp); //FIXME don't use hglobal memory
				}
			}
		}

		public IntPtr Value {
			get {
				if (value == IntPtr.Zero)
					value = Marshal.StringToHGlobalAnsi (str); //FIXME we want to use mono_string_to_utf8
				return value;
			}
		}
	}

	internal static class RuntimeMarshal {
		internal static string PtrToUtf8String (IntPtr ptr)
		{
			unsafe {
				return new String ((sbyte*)ptr);
			}
		}

		internal static StringMarshalHelper MarshalString (string str)
		{
			return new StringMarshalHelper (str);
		}

		static int DecodeBlobSize (IntPtr in_ptr, out IntPtr out_ptr)
		{
			uint size;
			unsafe {
				byte *ptr = (byte*)in_ptr;
	
				if ((*ptr & 0x80) == 0) {
					size = (uint)(ptr [0] & 0x7f);
					ptr++;
				} else if ((*ptr & 0x40) == 0){
					size = (uint)(((ptr [0] & 0x3f) << 8) + ptr [1]);
					ptr += 2;
				} else {
					size = (uint)(((ptr [0] & 0x1f) << 24) +
						(ptr [1] << 16) +
						(ptr [2] << 8) +
						ptr [3]);
					ptr += 4;
				}
				out_ptr = (IntPtr)ptr;
			}

			return (int)size;
		}

		internal static byte[] DecodeBlobArray (IntPtr ptr)
		{
			IntPtr out_ptr;
			int size = DecodeBlobSize (ptr, out out_ptr);
			byte[] res = new byte [size];
			Marshal.Copy (out_ptr, res, 0, size);
			return res;
		}

		internal static int AsciHexDigitValue (int c)
		{
			if (c >= '0' && c <= '9')
				return c - '0';
			if (c >= 'a' && c <= 'f')
				return c - 'a' + 10;
			return c - 'A' + 10;
		}

		[MethodImpl (MethodImplOptions.InternalCall)]
		internal static extern void FreeAssemblyName (ref MonoAssemblyName name);
	}
}
