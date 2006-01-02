//
// Mono.Unix/FileAccessPermissions.cs
//
// Authors:
//   Jonathan Pryor (jonpryor@vt.edu)
//
// (C) 2005-2006 Jonathan Pryor
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
using Mono.Unix;

namespace Mono.Unix {

	[Flags]
	public enum FileAccessPermissions {
		UserReadWriteExecute  = (int) Native.FilePermissions.S_IRWXU,
		UserRead              = (int) Native.FilePermissions.S_IRUSR,
		UserWrite             = (int) Native.FilePermissions.S_IWUSR,
		UserExecute           = (int) Native.FilePermissions.S_IXUSR,
		GroupReadWriteExecute = (int) Native.FilePermissions.S_IRWXG,
		GroupRead             = (int) Native.FilePermissions.S_IRGRP,
		GroupWrite            = (int) Native.FilePermissions.S_IWGRP,
		GroupExecute          = (int) Native.FilePermissions.S_IXGRP,
		OtherReadWriteExecute = (int) Native.FilePermissions.S_IRWXO,
		OtherRead             = (int) Native.FilePermissions.S_IROTH,
		OtherWrite            = (int) Native.FilePermissions.S_IWOTH,
		OtherExecute          = (int) Native.FilePermissions.S_IXOTH,

		DefaultPermissions    = (int) Native.FilePermissions.DEFFILEMODE,
		AllPermissions        = (int) Native.FilePermissions.ACCESSPERMS,
	}
}

