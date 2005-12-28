//
// Mono.Unix/FileHandleOperations.cs
//
// Authors:
//   Jonathan Pryor (jonpryor@vt.edu)
//
// (C) 2005 Jonathan Pryor
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
using System.IO;
using System.Text;
using Mono.Unix;

namespace Mono.Unix {

	public sealed /* static */ class FileHandleOperations
	{
		private FileHandleOperations () {}

		public static void AdviseFileAccessPattern (int fd, FileAccessPattern pattern, long offset, long len)
		{
			int r = Native.Syscall.posix_fadvise (fd, offset, len,
				(Native.PosixFadviseAdvice) pattern);
			UnixMarshal.ThrowExceptionForLastErrorIf (r);
		}

		public static void AdviseFileAccessPattern (int fd, FileAccessPattern pattern)
		{
			AdviseFileAccessPattern (fd, pattern, 0, 0);
		}

		public static void AdviseFileAccessPattern (FileStream file, FileAccessPattern pattern, long offset, long len)
		{
			if (file == null)
				throw new ArgumentNullException ("file");
			int r = Native.Syscall.posix_fadvise (file.Handle.ToInt32(), offset, len,
				(Native.PosixFadviseAdvice) pattern);
			UnixMarshal.ThrowExceptionForLastErrorIf (r);
		}

		public static void AdviseFileAccessPattern (FileStream file, FileAccessPattern pattern)
		{
			AdviseFileAccessPattern (file, pattern, 0, 0);
		}

		public static void AdviseFileAccessPattern (UnixStream stream, FileAccessPattern pattern, long offset, long len)
		{
			if (stream == null)
				throw new ArgumentNullException ("stream");
			int r = Native.Syscall.posix_fadvise (stream.Handle, offset, len,
				(Native.PosixFadviseAdvice) pattern);
			UnixMarshal.ThrowExceptionForLastErrorIf (r);
		}

		public static void AdviseFileAccessPattern (UnixStream stream, FileAccessPattern pattern)
		{
			AdviseFileAccessPattern (stream, pattern, 0, 0);
		}
	}
}

// vim: noexpandtab
