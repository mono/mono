//
// System.Security.AccessControl.FileSystemRights enum
//
// Author:
//	Dick Porter  <dick@ximian.com>
//
// Copyright (C) 2005, 2006 Novell, Inc (http://www.novell.com)
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

#if NET_2_0

namespace System.Security.AccessControl {

	[Flags]
	public enum FileSystemRights {
		ListDirectory			= 0x0000001,
		ReadData			= 0x0000001,
		CreateFiles			= 0x0000002,
		WriteData			= 0x0000002,
		AppendData			= 0x0000004,
		CreateDirectories		= 0x0000004,
		ReadExtendedAttributes		= 0x0000008,
		WriteExtendedAttributes		= 0x0000010,
		ExecuteFile			= 0x0000020,
		Traverse			= 0x0000020,
		DeleteSubdirectoriesAndFiles	= 0x0000040,
		ReadAttributes			= 0x0000080,
		WriteAttributes			= 0x0000100,
		Write				= 0x0000116,
		Delete				= 0x0010000,
		ReadPermissions			= 0x0020000,
		Read				= 0x0020089,
		ReadAndExecute			= 0x00200A9,
		Modify				= 0x00301BF,
		ChangePermissions		= 0x0040000,
		TakeOwnership			= 0x0080000,
		Synchronize			= 0x0100000,
		FullControl			= 0x01F01FF,
	}
}

#endif
