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
	[Guid("3AE0080A-7E3A-4366-BF89-0FEEDC931659")]
	[InterfaceType(1)]
	[TypeLibType(0x200)]
	internal interface IWbemEventSink
	{
		int GetRestrictedSink_(int lNumQueries, ref string awszQueries, object pCallback, out IWbemEventSink ppSink);

		int Indicate_(int lObjectCount, ref IWbemClassObject_DoNotMarshal apObjArray);

		int IndicateWithSD_(int lNumObjects, ref object apObjects, int lSDLength, ref byte pSD);

		int IsActive_();

		int SetBatchingParameters_(int lFlags, uint dwMaxBufferSize, uint dwMaxSendLatency);

		int SetSinkSecurity_(int lSDLength, ref byte pSD);

		int SetStatus_(int lFlags, int hResult, string strParam, IWbemClassObject_DoNotMarshal pObjParam);
	}
}