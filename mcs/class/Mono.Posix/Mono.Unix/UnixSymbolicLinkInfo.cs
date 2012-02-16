//
// Mono.Unix/UnixSymbolicLinkInfo.cs
//
// Authors:
//   Jonathan Pryor (jonpryor@vt.edu)
//
// (C) 2004-2006 Jonathan Pryor
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

	public sealed class UnixSymbolicLinkInfo : UnixFileSystemInfo
	{
		public UnixSymbolicLinkInfo (string path)
			: base (path)
		{
		}

		internal UnixSymbolicLinkInfo (string path, Native.Stat stat)
			: base (path, stat)
		{
		}

		public override string Name {
			get {return UnixPath.GetFileName (FullPath);}
		}

		[Obsolete ("Use GetContents()")]
		public UnixFileSystemInfo Contents {
			get {return GetContents ();}
		}

		public string ContentsPath {
			get {
				return ReadLink ();
			}
		}

		public bool HasContents {
			get {
				return TryReadLink () != null;
			}
		}

		public UnixFileSystemInfo GetContents ()
		{
			ReadLink ();
			return UnixFileSystemInfo.GetFileSystemEntry (
						UnixPath.Combine (UnixPath.GetDirectoryName (FullPath), 
							ContentsPath));
		}

		public void CreateSymbolicLinkTo (string path)
		{
			int r = Native.Syscall.symlink (path, FullName);
			UnixMarshal.ThrowExceptionForLastErrorIf (r);
		}

		public void CreateSymbolicLinkTo (UnixFileSystemInfo path)
		{
			int r = Native.Syscall.symlink (path.FullName, FullName);
			UnixMarshal.ThrowExceptionForLastErrorIf (r);
		}

		public override void Delete ()
		{
			int r = Native.Syscall.unlink (FullPath);
			UnixMarshal.ThrowExceptionForLastErrorIf (r);
			base.Refresh ();
		}

		public override void SetOwner (long owner, long group)
		{
			int r = Native.Syscall.lchown (FullPath, Convert.ToUInt32 (owner), Convert.ToUInt32 (group));
			UnixMarshal.ThrowExceptionForLastErrorIf (r);
		}

		protected override bool GetFileStatus (string path, out Native.Stat stat)
		{
			return Native.Syscall.lstat (path, out stat) == 0;
		}

		private string ReadLink ()
		{
			string r = TryReadLink ();
			if (r == null)
				UnixMarshal.ThrowExceptionForLastError ();
			return r;
		}

		private string TryReadLink ()
		{
			StringBuilder sb = new StringBuilder ((int) base.Length+1);
			int r = Native.Syscall.readlink (FullPath, sb);
			if (r == -1)
				return null;
			return sb.ToString (0, r);
		}
	}
}

// vim: noexpandtab
