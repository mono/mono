//
// Mono.Unix.Catalog.cs: Wrappers for the libintl library.
//
// Authors:
//   Edd Dumbill (edd@usefulinc.com)
//   Jonathan Pryor (jonpryor@vt.edu)
//
// (C) 2004 Edd Dumbill
// (C) 2005 Jonathan Pryor
//
// This file implements the low-level syscall interface to the POSIX
// subsystem.
//
// This file tries to stay close to the low-level API as much as possible
// using enumerations, structures and in a few cases, using existing .NET
// data types.
//
// Implementation notes:
//
//    Since the values for the various constants on the API changes
//    from system to system (even Linux on different architectures will
//    have different values), we define our own set of values, and we
//    use a set of C helper routines to map from the constants we define
//    to the values of the native OS.
//
//    Bitfields are flagged with the [Map] attribute, and a helper program
//    generates a set of map_XXXX routines that we can call to convert
//    from our value definitions to the value definitions expected by the
//    OS.
//
//    Methods that require tuning are bound as `internal syscal_NAME' methods
//    and then a `NAME' method is exposed.
//

using System;
using System.Runtime.InteropServices;

namespace Mono.Unix {

	public class Catalog {
		[Obsolete ("Don't create Catalog instances.")]
		public Catalog () {}

		[DllImport("libintl")]
		static extern IntPtr bindtextdomain (IntPtr domainname, IntPtr dirname);
		[DllImport("libintl")]
		static extern IntPtr bind_textdomain_codeset (IntPtr domainname,
			IntPtr codeset);
		[DllImport("libintl")]
		static extern IntPtr textdomain (IntPtr domainname);
		
		public static void Init (String package, String localedir)
		{
			IntPtr ipackage, ilocaledir, iutf8;
			MarshalStrings (package, out ipackage, localedir, out ilocaledir, 
					"UTF-8", out iutf8);
			try {
				if (bindtextdomain (ipackage, ilocaledir) == IntPtr.Zero)
					throw new OutOfMemoryException ("bindtextdomain");
				if (bind_textdomain_codeset (ipackage, iutf8) == IntPtr.Zero)
					throw new OutOfMemoryException ("bind_textdomain_codeset");
				if (textdomain (ipackage) == IntPtr.Zero)
					throw new OutOfMemoryException ("textdomain");
			}
			finally {
				Marshal.FreeHGlobal (ipackage);
				Marshal.FreeHGlobal (ilocaledir);
				Marshal.FreeHGlobal (iutf8);
			}
		}

		private static void MarshalStrings (string s1, out IntPtr p1, 
				string s2, out IntPtr p2, string s3, out IntPtr p3)
		{
			p1 = p2 = p3 = IntPtr.Zero;

			bool cleanup = true;

			try {
				p1 = Marshal.StringToHGlobalAuto (s1);
				p2 = Marshal.StringToHGlobalAuto (s2);
				if (s3 != null)
					p3 = Marshal.StringToHGlobalAuto (s3);
				cleanup = false;
			}
			finally {
				if (cleanup) {
					Marshal.FreeHGlobal (p1);
					Marshal.FreeHGlobal (p2);
					Marshal.FreeHGlobal (p3);
				}
			}
		}
	
		[DllImport("libintl")]
		static extern IntPtr gettext (IntPtr instring);
		
		public static String GetString (String s)
		{
			IntPtr ints = Marshal.StringToHGlobalAuto (s);
			try {
				// gettext(3) returns the input pointer if no translation is found
				IntPtr r = gettext (ints);
				if (r != ints)
					return Marshal.PtrToStringAuto (r);
				return s;
			}
			finally {
				Marshal.FreeHGlobal (ints);
			}
		}
	
		[DllImport("libintl")]
		static extern IntPtr ngettext (IntPtr singular, IntPtr plural, Int32 n);
		
		public static String GetPluralString (String s, String p, Int32 n)
		{
			IntPtr ints, intp, _ignore;
			MarshalStrings (s, out ints, p, out intp, null, out _ignore);

			try {
				// ngettext(3) returns an input pointer if no translation is found
				IntPtr r = ngettext (ints, intp, n);
				if (r == ints)
					return s;
				if (r == intp)
					return p;
				return Marshal.PtrToStringAnsi (r); 
			}
			finally {
				Marshal.FreeHGlobal (ints);
				Marshal.FreeHGlobal (intp);
			}
		}
	}
}

