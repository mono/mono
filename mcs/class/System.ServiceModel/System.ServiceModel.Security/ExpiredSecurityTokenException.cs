//
// ExpiredSecurityTokenException.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2006 Novell, Inc.  http://www.novell.com
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
using System.IdentityModel.Tokens;
using System.Runtime.Serialization;
using System.ServiceModel.Channels;

namespace System.ServiceModel.Security
{
	[Serializable]
	public class ExpiredSecurityTokenException : MessageSecurityException
	{
		public ExpiredSecurityTokenException ()
			: this ("The security token is expired.")
		{
		}

		public ExpiredSecurityTokenException (string message)
			: base (message)
		{
		}

		public ExpiredSecurityTokenException (string message,
			Exception innerException)
			: base (message, innerException)
		{
		}

		protected ExpiredSecurityTokenException (SerializationInfo info,
			StreamingContext context)
			: base (info, context)
		{
		}

		public override void GetObjectData (SerializationInfo info,  StreamingContext ctx)
		{
			base.GetObjectData (info, ctx);
		}
	}
}
