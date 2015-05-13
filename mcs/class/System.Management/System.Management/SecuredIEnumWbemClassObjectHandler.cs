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

namespace System.Management
{
	internal class SecuredIEnumWbemClassObjectHandler
	{
		private IEnumWbemClassObject pEnumWbemClassObjectsecurityHelper;

		private ManagementScope scope;

		internal SecuredIEnumWbemClassObjectHandler(ManagementScope theScope, IEnumWbemClassObject pEnumWbemClassObject)
		{
			this.scope = theScope;
			this.pEnumWbemClassObjectsecurityHelper = pEnumWbemClassObject;
		}

		internal int Clone_(ref IEnumWbemClassObject ppEnum)
		{
			int num = -2147217407;
			if (this.scope != null)
			{
				num = WmiNetUtilsHelper.CloneEnumWbemClassObject_f(out ppEnum, (int)this.scope.Options.Authentication, (int)this.scope.Options.Impersonation, this.pEnumWbemClassObjectsecurityHelper, this.scope.Options.Username, this.scope.Options.GetPassword(), this.scope.Options.Authority);
			}
			return num;
		}

		internal int Next_(int lTimeout, int uCount, IWbemClassObject_DoNotMarshal[] ppOutParams, ref uint puReturned)
		{
			int num = this.pEnumWbemClassObjectsecurityHelper.Next_(lTimeout, uCount, ppOutParams, out puReturned);
			return num;
		}

		internal int NextAsync_(uint uCount, IWbemObjectSink pSink)
		{
			int num = this.pEnumWbemClassObjectsecurityHelper.NextAsync_(uCount, pSink);
			return num;
		}

		internal int Reset_()
		{
			int num = this.pEnumWbemClassObjectsecurityHelper.Reset_();
			return num;
		}

		internal int Skip_(int lTimeout, uint nCount)
		{
			int num = this.pEnumWbemClassObjectsecurityHelper.Skip_(lTimeout, nCount);
			return num;
		}
	}
}