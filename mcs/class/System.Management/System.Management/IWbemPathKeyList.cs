//
// System.Management.AuthenticationLevel
//
// Author:
//	Bruno Lauze     (brunolauze@msn.com)
//	Atsushi Enomoto (atsushi@ximian.com)
//
// Copyright (C) 2015 Microsoft (http://www.microsoft.com)
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
using System.Runtime.InteropServices;

namespace System.Management
{
	[Guid("9AE62877-7544-4BB0-AA26-A13824659ED6")]
	[InterfaceType(1)]
	internal interface IWbemPathKeyList
	{
		int GetCount_(out uint puKeyCount);

		int GetInfo_(uint uRequestedInfo, out ulong puResponse);

		int GetKey_(uint uKeyIx, uint uFlags, out uint puNameBufSize, string pszKeyName, out uint puKeyValBufSize, IntPtr pKeyVal, out uint puApparentCimType);

		int GetKey2_(uint uKeyIx, uint uFlags, out uint puNameBufSize, string pszKeyName, out object pKeyValue, out uint puApparentCimType);

		int GetText_(int lFlags, out uint puBuffLength, string pszText);

		int MakeSingleton_(sbyte bSet);

		int RemoveAllKeys_(uint uFlags);

		int RemoveKey_(string wszName, uint uFlags);

		int SetKey_(string wszName, uint uFlags, uint uCimType, IntPtr pKeyVal);

		int SetKey2_(string wszName, uint uFlags, uint uCimType, ref object pKeyVal);
	}
}