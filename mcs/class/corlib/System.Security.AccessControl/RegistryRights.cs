//
// System.Security.AccessControl.RegistryRights enum
//
// Author:
//	Dick Porter  <dick@ximian.com>
//
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
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
	public enum RegistryRights {
		QueryValues		= 0x00001,
		SetValue		= 0x00002,
		CreateSubKey		= 0x00004,
		EnumerateSubKeys	= 0x00008,
		Notify			= 0x00010,
		CreateLink		= 0x00020,
		Delete			= 0x10000,
		ReadPermissions		= 0x20000,
		WriteKey		= 0x20006,
		ReadKey			= 0x20019,
		ExecuteKey		= 0x20019,
		ChangePermissions	= 0x40000,
		TakeOwnership		= 0x80000,
		FullControl		= 0xF003F,
	}
}

#endif
