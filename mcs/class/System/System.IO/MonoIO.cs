//
// System.IO.MonoIO.cs: static interface to native filesystem.
//
// Author:
//   Dan Lewis (dihlewis@yahoo.co.uk)
//   Dick Porter (dick@ximian.com)
//
// (C) 2002
//

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
		public extern static bool DuplicateHandle (IntPtr source_process_handle, IntPtr source_handle,
			IntPtr target_process_handle, out IntPtr target_handle, int access, int inherit, int options);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		public extern static int GetTempPath(out string path);
	}
}

