// 
// Novell.Directory.Ldap.Security.WrapPrivilegedAction.cs
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
using vmw.common;

using java.security;
using org.ietf.jgss;

namespace Novell.Directory.Ldap.Security
{
	internal class WrapPrivilegedAction : PrivilegedAction
	{
		#region Fields

		private readonly byte [] _buffer;
		private readonly int _start;
		private readonly int _len;
		private readonly GSSContext _context;
		private readonly MessageProp _messageProperties;

		#endregion // Fields

		#region Constructors

		public WrapPrivilegedAction(GSSContext context, byte [] buffer, int start, int len, MessageProp messageProperties)
		{
			_buffer = buffer;
			_start = start;
			_len = len;
			_context = context;
			_messageProperties = messageProperties;
		}

		#endregion // Constructors

		#region Methods

		public object run()
		{
			try {
				sbyte [] result = _context.wrap (TypeUtils.ToSByteArray (_buffer), _start, _len, _messageProperties);
				return (byte []) TypeUtils.ToByteArray (result);
			}
			catch (GSSException e) {
				throw new PrivilegedActionException (e);
			}
		}

		#endregion // Methods
	}
}
