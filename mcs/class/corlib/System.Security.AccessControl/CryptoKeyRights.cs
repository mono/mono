//
// System.Security.AccessControl.CryptoKeyRights enum
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
	public enum CryptoKeyRights {
		ReadData		= 0x00000001,
		WriteData		= 0x00000002,
		ReadExtendedAttributes	= 0x00000008,
		WriteExtendedAttributes	= 0x00000010,
		ReadAttributes		= 0x00000080,
		WriteAttributes		= 0x00000100,
		Delete			= 0x00010000,
		ReadPermissions		= 0x00020000,
		ChangePermissions	= 0x00040000,
		TakeOwnership		= 0x00080000,
		Synchronize		= 0x00100000,
		FullControl		= 0x001F019B,
		GenericAll		= 0x10000000,
		GenericExecute		= 0x20000000,
		GenericWrite		= 0x40000000,
		GenericRead		= unchecked((int)0x80000000),	/* overflow */
	}
}

#endif
