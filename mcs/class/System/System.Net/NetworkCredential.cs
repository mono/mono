//
// System.Net.NetworkCredential.cs
//
// Author: Duncan Mak (duncan@ximian.com)
// Author: Rolf Bjarne KVinge (rolf@xamarin.com)
//
// (C) Ximian, Inc.
// Copyright (C) 2010 Novell, Inc (http://www.novell.com)
// Copyright (C) 2011 Xamarin Inc (http://www.xamarin.com)
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

using System.Security;

namespace System.Net
{
	public class NetworkCredential : ICredentials
					, ICredentialsByHost
	{
		// Fields
		string userName;
		string password;
		string domain;
		
#if NET_4_0
		SecureString securePassword;
#endif

		// Constructors
		public NetworkCredential ()
		{
		}

		public NetworkCredential (string userName, string password)
		{
			this.userName = userName;
			this.password = password;
		}

		public NetworkCredential (string userName, string password, string domain)
		{
			this.userName = userName;
			this.password = password;
			this.domain = domain;
		}

		// Properties

		public string Domain {
			get { return domain ?? String.Empty; }
			set { domain = value; }
		}

		public string UserName {
			get { return userName ?? String.Empty; }
			set { userName = value; }			
		}

		public string Password {
			get { return password ?? String.Empty; }
			set { password = value; }
		}

#if NET_4_0
		public SecureString SecurePassword {
			get { return securePassword; }
			set {
				if (value == null) {
					securePassword = new SecureString ();
				} else {
					securePassword = value;
				}
			}
		}
#endif

		public NetworkCredential GetCredential (Uri uri, string authType)
		{
			return this;
		}

		public NetworkCredential GetCredential (string host, int port, string authenticationType)
		{
			return this;
		}
	}
}
