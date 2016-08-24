//
// FileSystemAclExtensions.cs
//
// Author:
//   Alexander Köplinger (alexander.koeplinger@xamarin.com)
//
// (C) 2016 Xamarin, Inc.
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

using System.Security.AccessControl;

namespace System.IO
{
	public static class FileSystemAclExtensions
	{
		public static DirectorySecurity GetAccessControl(this DirectoryInfo directoryInfo)
		{
			if (directoryInfo == null)
				throw new ArgumentNullException (nameof (directoryInfo));

			return directoryInfo.GetAccessControl ();
		}

		public static DirectorySecurity GetAccessControl(this DirectoryInfo directoryInfo, AccessControlSections includeSections)
		{
			if (directoryInfo == null)
				throw new ArgumentNullException (nameof (directoryInfo));

			return directoryInfo.GetAccessControl (includeSections);
		}

		public static FileSecurity GetAccessControl(this FileInfo fileInfo)
		{
			if (fileInfo == null)
				throw new ArgumentNullException (nameof (fileInfo));

			return fileInfo.GetAccessControl ();
		}

		public static FileSecurity GetAccessControl(this FileInfo fileInfo, AccessControlSections includeSections)
		{
			if (fileInfo == null)
				throw new ArgumentNullException (nameof (fileInfo));

			return fileInfo.GetAccessControl (includeSections);
		}

		public static FileSecurity GetAccessControl(this FileStream fileStream)
		{
			if (fileStream == null)
				throw new ArgumentNullException (nameof (fileStream));

			return fileStream.GetAccessControl ();
		}

		public static void SetAccessControl(this DirectoryInfo directoryInfo, DirectorySecurity directorySecurity)
		{
			if (directoryInfo == null)
				throw new ArgumentNullException (nameof (directoryInfo));

			directoryInfo.SetAccessControl (directorySecurity);
		}

		public static void SetAccessControl(this FileInfo fileInfo, FileSecurity fileSecurity)
		{
			if (fileInfo == null)
				throw new ArgumentNullException (nameof (fileInfo));

			fileInfo.SetAccessControl (fileSecurity);
		}

		public static void SetAccessControl(this FileStream fileStream, FileSecurity fileSecurity)
		{
			if (fileStream == null)
				throw new ArgumentNullException (nameof (fileStream));

			fileStream.SetAccessControl (fileSecurity);
		}
	}
}