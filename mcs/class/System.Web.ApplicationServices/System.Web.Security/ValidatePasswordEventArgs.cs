//
// System.Web.Security.ValidatePasswordEventArgs
//
// Authors:
//	Lluis Sanchez Gual (lluis@novell.com)
//
// (C) 2005 Novell, inc.
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
using System.Runtime.CompilerServices;

namespace System.Web.Security
{
	[TypeForwardedFrom ("System.Web, Version=2.0.0.0, Culture=Neutral, PublicKeyToken=b03f5f7f11d50a3a")]
	public sealed class ValidatePasswordEventArgs: EventArgs
	{
		bool cancel;
		Exception exception;
		bool isNewUser;
		string userName;
		string password;
		
		public ValidatePasswordEventArgs (string userName, string password, bool isNewUser)
		{
			this.isNewUser = isNewUser;
			this.userName = userName;
			this.password = password;
		}
		
		public bool Cancel {
			get { return cancel; }
			set { cancel = value; }
		}
		
		public Exception FailureInformation {
			get { return exception; }
			set { exception = value; }
		}
		
		public bool IsNewUser {
			get { return isNewUser; }
		}
		
		public string UserName {
			get { return userName; }
		}
		
		public string Password {
			get { return password; }
		}
	}
}

