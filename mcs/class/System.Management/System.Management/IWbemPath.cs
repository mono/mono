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
	[Guid("3BC15AF2-736C-477E-9E51-238AF8667DCC")]
	[InterfaceType(1)]
	internal interface IWbemPath
	{
		int CreateClassPart_(int lFlags, string Name);

		int DeleteClassPart_(int lFlags);

		int GetClassName_(out int puBuffLength, string pszName);

		int GetInfo_(uint uRequestedInfo, out ulong puResponse);

		int GetKeyList_(out IWbemPathKeyList pOut);

		int GetNamespaceAt_(uint uIndex, out int puNameBufLength, string pName);

		int GetNamespaceCount_(out uint puCount);

		int GetScope_(uint uIndex, out uint puClassNameBufSize, string pszClass, out IWbemPathKeyList pKeyList);

		int GetScopeAsText_(uint uIndex, out uint puTextBufSize, string pszText);

		int GetScopeCount_(out uint puCount);

		int GetServer_(out int puNameBufLength, string pName);

		int GetText_(int lFlags, out int puBuffLength, string pszText);

		int IsLocal_(string wszMachine);

		int IsRelative_(string wszMachine, string wszNamespace);

		int IsRelativeOrChild_(string wszMachine, string wszNamespace, int lFlags);

		int IsSameClassName_(string wszClass);

		int RemoveAllNamespaces_();

		int RemoveAllScopes_();

		int RemoveNamespaceAt_(uint uIndex);

		int RemoveScope_(uint uIndex);

		int SetClassName_(string Name);

		int SetNamespaceAt_(uint uIndex, string pszName);

		int SetScope_(uint uIndex, string pszClass);

		int SetScopeFromText_(uint uIndex, string pszText);

		int SetServer_(string Name);

		int SetText_(uint uMode, string pszPath);
	}
}