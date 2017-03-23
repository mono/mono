#if MONO_FEATURE_APPLETLS || MONO_FEATURE_APPLE_X509
using System;
using System.Runtime.InteropServices;

namespace XamMac.CoreFoundation
{
	internal static class CFHelpers
	{
		internal const string CoreFoundationLibrary = "/System/Library/Frameworks/CoreFoundation.framework/CoreFoundation";
		internal const string SecurityLibrary = "/System/Library/Frameworks/Security.framework/Security";

		[DllImport (CoreFoundationLibrary)]
		internal extern static void CFRelease (IntPtr obj);

		[DllImport (CoreFoundationLibrary)]
		internal extern static IntPtr CFRetain (IntPtr obj);

		[StructLayout (LayoutKind.Sequential)]
		struct CFRange {
			public IntPtr loc;
			public IntPtr len;

			public CFRange (int loc, int len)
				: this ((long) loc, (long) len)
			{
			}

			public CFRange (long l, long len)
			{
				this.loc = (IntPtr) l;
				this.len = (IntPtr) len;
			}
		}

		[DllImport (CoreFoundationLibrary, CharSet=CharSet.Unicode)]
		extern static IntPtr CFStringCreateWithCharacters (IntPtr allocator, string str, IntPtr count);

		[DllImport (CoreFoundationLibrary, CharSet=CharSet.Unicode)]
		extern static IntPtr CFStringGetLength (IntPtr handle);

		[DllImport (CoreFoundationLibrary, CharSet=CharSet.Unicode)]
		extern static IntPtr CFStringGetCharactersPtr (IntPtr handle);

		[DllImport (CoreFoundationLibrary, CharSet=CharSet.Unicode)]
		extern static IntPtr CFStringGetCharacters (IntPtr handle, CFRange range, IntPtr buffer);

		internal static string FetchString (IntPtr handle)
		{
			if (handle == IntPtr.Zero)
				return null;

			string str;

			int l = (int)CFStringGetLength (handle);
			IntPtr u = CFStringGetCharactersPtr (handle);
			IntPtr buffer = IntPtr.Zero;
			if (u == IntPtr.Zero){
				CFRange r = new CFRange (0, l);
				buffer = Marshal.AllocCoTaskMem (l * 2);
				CFStringGetCharacters (handle, r, buffer);
				u = buffer;
			}
			unsafe {
				str = new string ((char *) u, 0, l);
			}

			if (buffer != IntPtr.Zero)
				Marshal.FreeCoTaskMem (buffer);

			return str;
		}

		[DllImport (CoreFoundationLibrary)]
		extern static IntPtr CFDataGetLength (IntPtr handle);

		[DllImport (CoreFoundationLibrary)]
		extern static IntPtr CFDataGetBytePtr (IntPtr handle);

		internal static byte[] FetchDataBuffer (IntPtr handle)
		{
			var length = (int)CFDataGetLength (handle);
			var buffer = new byte [length];
			var ptr = CFDataGetBytePtr (handle);
			Marshal.Copy (ptr, buffer, 0, buffer.Length);
			return buffer;
		}

		[DllImport (CoreFoundationLibrary)]
		extern static IntPtr CFDataCreateWithBytesNoCopy (IntPtr allocator, IntPtr bytes, IntPtr length, IntPtr bytesDeallocator);

		[DllImport (CoreFoundationLibrary)]
		extern static IntPtr CFDataCreate (IntPtr allocator, IntPtr bytes, IntPtr length);

		[DllImport (SecurityLibrary)]
		extern static IntPtr SecCertificateCreateWithData (IntPtr allocator, IntPtr cfData);

		unsafe internal static IntPtr CreateCertificateFromData (byte[] data)
		{
			fixed (void *ptr = data) {
				var cfdata = CFDataCreate (IntPtr.Zero, (IntPtr)ptr, new IntPtr (data.Length));
				if (cfdata == IntPtr.Zero)
					return IntPtr.Zero;

				var certificate = SecCertificateCreateWithData (IntPtr.Zero, cfdata);
				if (cfdata != IntPtr.Zero)
					CFRelease (cfdata);
				return certificate;
			}
		}
	}
}
#endif
