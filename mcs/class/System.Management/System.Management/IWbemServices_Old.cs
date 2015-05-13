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
	[Guid("9556DC99-828C-11CF-A37E-00AA003240C7")]
	[InterfaceType(1)]
	[TypeLibType(0x200)]
	internal interface IWbemServices_Old
	{
		int CancelAsyncCall_(IWbemObjectSink pSink);

		int CreateClassEnum_(string strSuperclass, int lFlags, IWbemContext pCtx, out IEnumWbemClassObject ppEnum);

		int CreateClassEnumAsync_(string strSuperclass, int lFlags, IWbemContext pCtx, IWbemObjectSink pResponseHandler);

		int CreateInstanceEnum_(string strFilter, int lFlags, IWbemContext pCtx, out IEnumWbemClassObject ppEnum);

		int CreateInstanceEnumAsync_(string strFilter, int lFlags, IWbemContext pCtx, IWbemObjectSink pResponseHandler);

		int DeleteClass_(string strClass, int lFlags, IWbemContext pCtx, IntPtr ppCallResult);

		int DeleteClassAsync_(string strClass, int lFlags, IWbemContext pCtx, IWbemObjectSink pResponseHandler);

		int DeleteInstance_(string strObjectPath, int lFlags, IWbemContext pCtx, IntPtr ppCallResult);

		int DeleteInstanceAsync_(string strObjectPath, int lFlags, IWbemContext pCtx, IWbemObjectSink pResponseHandler);

		int ExecMethod_(string strObjectPath, string strMethodName, int lFlags, IWbemContext pCtx, IWbemClassObject_DoNotMarshal pInParams, out IWbemClassObject_DoNotMarshal ppOutParams, IntPtr ppCallResult);

		int ExecMethodAsync_(string strObjectPath, string strMethodName, int lFlags, IWbemContext pCtx, IWbemClassObject_DoNotMarshal pInParams, IWbemObjectSink pResponseHandler);

		int ExecNotificationQuery_(string strQueryLanguage, string strQuery, int lFlags, IWbemContext pCtx, out IEnumWbemClassObject ppEnum);

		int ExecNotificationQueryAsync_(string strQueryLanguage, string strQuery, int lFlags, IWbemContext pCtx, IWbemObjectSink pResponseHandler);

		int ExecQuery_(string strQueryLanguage, string strQuery, int lFlags, IWbemContext pCtx, out IEnumWbemClassObject ppEnum);

		int ExecQueryAsync_(string strQueryLanguage, string strQuery, int lFlags, IWbemContext pCtx, IWbemObjectSink pResponseHandler);

		int GetObject_(string strObjectPath, int lFlags, IWbemContext pCtx, out IWbemClassObject_DoNotMarshal ppObject, IntPtr ppCallResult);

		int GetObjectAsync_(string strObjectPath, int lFlags, IWbemContext pCtx, IWbemObjectSink pResponseHandler);

		int OpenNamespace_(string strNamespace, int lFlags, IWbemContext pCtx, out IWbemServices ppWorkingNamespace, IntPtr ppCallResult);

		int PutClass_(IWbemClassObject_DoNotMarshal pObject, int lFlags, IWbemContext pCtx, IntPtr ppCallResult);

		int PutClassAsync_(IWbemClassObject_DoNotMarshal pObject, int lFlags, IWbemContext pCtx, IWbemObjectSink pResponseHandler);

		int PutInstance_(IWbemClassObject_DoNotMarshal pInst, int lFlags, IWbemContext pCtx, IntPtr ppCallResult);

		int PutInstanceAsync_(IWbemClassObject_DoNotMarshal pInst, int lFlags, IWbemContext pCtx, IWbemObjectSink pResponseHandler);

		int QueryObjectSink_(int lFlags, out IWbemObjectSink ppResponseHandler);
	}
}