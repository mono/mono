// 
// Novell.Directory.Ldap.Security.AuthenticationCallbackHandler.cs
//
// Authors:
//  Boris Kirzner <borsk@mainsoft.com>
//	Konstantin Triger <kostat@mainsoft.com>
//	
// (C) 2005 Mainsoft Corporation (http://www.mainsoft.com)
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

using javax.security.auth.callback;
using java.io;

namespace Novell.Directory.Ldap.Security
{
	internal class AuthenticationCallbackHandler : CallbackHandler
	{

		#region Fields

		private readonly string _username;
		private readonly string _password;

		#endregion //Fields

		#region Constructors

		public AuthenticationCallbackHandler(string username, string password)
		{
			_username = username;
			_password = password;
		}

		#endregion // Constructors

		#region Methods

		public void handle(Callback [] callbacks)
		{
			for (int i = 0; i < callbacks.Length; i++) {
				if (callbacks [i] is NameCallback) {
					NameCallback nc = (NameCallback) callbacks [i];
					nc.setName (_username);
				}
				else if (callbacks [i] is PasswordCallback) {
					PasswordCallback pc = (PasswordCallback) callbacks [i];
					pc.setPassword (_password.ToCharArray ());
				}
				else {
					throw new UnsupportedCallbackException (callbacks [i], "Unrecognized Callback");
				}
			}
		}

		#endregion // Methods
	}
}
