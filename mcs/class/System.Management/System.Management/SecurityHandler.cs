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
	internal class SecurityHandler
	{
		private bool needToReset;

		private IntPtr handle;

		private ManagementScope scope;

		internal SecurityHandler(ManagementScope theScope)
		{
			this.scope = theScope;
			if (this.scope != null && this.scope.Options.EnablePrivileges)
			{
				WmiNetUtilsHelper.SetSecurity_f(out this.needToReset, out this.handle);
			}
		}

		internal void Reset()
		{
			if (this.needToReset)
			{
				this.needToReset = false;
				if (this.scope != null)
				{
					WmiNetUtilsHelper.ResetSecurity_f(this.handle);
				}
			}
		}

		internal void Secure(IWbemServices services)
		{
			if (this.scope != null)
			{
				IntPtr password = this.scope.Options.GetPassword();
				int num = WmiNetUtilsHelper.BlessIWbemServices_f(services, this.scope.Options.Username, password, this.scope.Options.Authority, (int)this.scope.Options.Impersonation, (int)this.scope.Options.Authentication);
				Marshal.ZeroFreeBSTR(password);
				if (num < 0)
				{
					Marshal.ThrowExceptionForHR(num);
				}
			}
		}

		internal void SecureIUnknown(object unknown)
		{
			if (this.scope != null)
			{
				IntPtr password = this.scope.Options.GetPassword();
				int num = WmiNetUtilsHelper.BlessIWbemServicesObject_f(unknown, this.scope.Options.Username, password, this.scope.Options.Authority, (int)this.scope.Options.Impersonation, (int)this.scope.Options.Authentication);
				Marshal.ZeroFreeBSTR(password);
				if (num < 0)
				{
					Marshal.ThrowExceptionForHR(num);
				}
			}
		}
	}
}