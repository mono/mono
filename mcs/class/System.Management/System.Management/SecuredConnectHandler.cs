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
	internal class SecuredConnectHandler
	{
		private ManagementScope scope;

		internal SecuredConnectHandler(ManagementScope theScope)
		{
			this.scope = theScope;
		}

		internal int ConnectNSecureIWbemServices(string path, ref IWbemServices pServices)
		{
			int num = -2147217407;
			if (this.scope != null)
			{
				num = WmiNetUtilsHelper.ConnectServerWmi_f(path, this.scope.Options.Username, this.scope.Options.GetPassword(), this.scope.Options.Locale, this.scope.Options.Flags, this.scope.Options.Authority, this.scope.Options.GetContext(), out pServices, (int)this.scope.Options.Impersonation, (int)this.scope.Options.Authentication);
			}
			return num;
		}
	}
}