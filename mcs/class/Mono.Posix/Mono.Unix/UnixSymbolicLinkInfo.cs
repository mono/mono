//
// Mono.Unix/UnixSymbolicLinkInfo.cs
//
// Authors:
//   Jonathan Pryor (jonpryor@vt.edu)
//
// (C) 2004 Jonathan Pryor
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

		internal UnixSymbolicLinkInfo (string path, Stat stat)
			: base (path, stat)
		{
		}

		public override string Name {
			get {return UnixPath.GetFileName (FullPath);}
		}

		public UnixFileSystemInfo Contents {
			get {
				return UnixFileSystemInfo.Create (ContentsPath);
			}
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

		public void CreateSymbolicLinkTo (string path)
		{
			int r = Syscall.symlink (path, OriginalPath);
			UnixMarshal.ThrowExceptionForLastErrorIf (r);
		}

		public void CreateSymbolicLinkTo (UnixFileSystemInfo path)
		{
			int r = Syscall.symlink (path.FullName, OriginalPath);
			UnixMarshal.ThrowExceptionForLastErrorIf (r);
		}

		public override void Delete ()
		{
			int r = Syscall.unlink (FullPath);
			UnixMarshal.ThrowExceptionForLastErrorIf (r);
			base.Refresh ();
		}

		public override void SetOwner (uint owner, uint group)
		{
			int r = Syscall.lchown (FullPath, owner, group);
			UnixMarshal.ThrowExceptionForLastErrorIf (r);
		}

		// TODO: Should ReadLink be in UnixSymbolicLinkInfo?
		private string ReadLink ()
		{
			string r = TryReadLink ();
			if (r == null)
				UnixMarshal.ThrowExceptionForLastError ();
			return r;
		}

		private string TryReadLink ()
		{
			return UnixPath.ReadSymbolicLink (FullPath);
		}
	}
}

// vim: noexpandtab
