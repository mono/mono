//
// System.IO.MonoIO.cs: static interface to native filesystem.
//
// Author:
//   Dan Lewis (dihlewis@yahoo.co.uk)
//   Dick Porter (dick@ximian.com)
//
// (C) 2002
//

using System;
using System.Runtime.CompilerServices;

// This is a heavily cut down version of the corlib class.  It's here
// because we're keeping extensions invisible, but
// System.Diagnostics.Process needs access to some of the
// functionality (and CVS can't do symlinks).

namespace System.IO
{
	internal sealed class MonoIO {

		// handle methods

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		public extern static bool Close (IntPtr handle,
						 out MonoIOError error);
		
		// console handles

		public extern static IntPtr ConsoleOutput {
			[MethodImplAttribute (MethodImplOptions.InternalCall)]
			get;
		}

		public extern static IntPtr ConsoleInput {
			[MethodImplAttribute (MethodImplOptions.InternalCall)]
			get;
		}

		public extern static IntPtr ConsoleError {
			[MethodImplAttribute (MethodImplOptions.InternalCall)]
			get;
		}

		// pipe handles

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		public extern static bool CreatePipe (out IntPtr read_handle, out IntPtr write_handle);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		public extern static int GetTempPath(out string path);
	}
}

