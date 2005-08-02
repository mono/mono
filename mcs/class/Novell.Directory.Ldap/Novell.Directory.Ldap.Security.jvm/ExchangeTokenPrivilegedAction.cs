// 
// Novell.Directory.Ldap.Security.ExchangeTokenPrivilegedAction.cs
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

using org.ietf.jgss;
using java.security;

namespace Novell.Directory.Ldap.Security
{
	internal class ExchangeTokenPrivilegedAction : PrivilegedAction
	{
		#region Fields

		private readonly sbyte [] _token;
		private readonly GSSContext _context;

		#endregion // Fields

		#region Constructors

		public ExchangeTokenPrivilegedAction(GSSContext context, sbyte [] token)
		{
			_token = token;
			_context = context;
		}

		#endregion // Constructors

		#region Methods

		public object run()
		{
			try {
				sbyte [] token = _context.initSecContext (_token, 0, _token.Length);
				return token;
			}
			catch (GSSException e) {
				throw new PrivilegedActionException (e);
			}
		}

		#endregion // Methods
	}
}
