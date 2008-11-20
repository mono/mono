//
// System.Web.ClientServices.ClientFormsIdentity
//
// Authors:
//   Marek Habersack (mhabersack@novell.com)
//
// (C) 2008 Novell, Inc
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

#if NET_3_5
using System;
using System.ComponentModel;
using System.Net;
using System.Security;
using System.Security.Principal;
using System.Web;
using System.Web.Security;
using System.Web.UI;

namespace System.Web.ClientServices
{
	public class ClientFormsIdentity : IDisposable, IIdentity
	{
		string Password {
			get;
			set;
		}
		
		public CookieContainer AuthenticationCookies {
			get;
			private set;
		}
		
		public string AuthenticationType {
			get;
			private set;
		}
		
		public bool IsAuthenticated {
			get;
			private set;
		}
		
		public string Name {
			get;
			private set;
		}
		
		public MembershipProvider Provider {
			get;
			private set;
		}
		
		public ClientFormsIdentity (string name, string password, MembershipProvider provider, string authenticationType, bool isAuthenticated, CookieContainer authenticationCookies)
		{
			Password = password;
			Name = name;
			Provider = provider;
			AuthenticationType = authenticationType;
			IsAuthenticated = isAuthenticated;
			AuthenticationCookies = authenticationCookies;
		}
		
		public void Dispose ()
		{
			throw new NotImplementedException ();
		}
		
		public void RevalidateUser ()
		{
			throw new NotImplementedException ();
		}		
	}
}
#endif
