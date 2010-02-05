//
// SecurityPolicyVersion.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2010 Novell, Inc.  http://www.novell.com
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

namespace System.ServiceModel.Security
{
	public abstract class SecurityPolicyVersion
	{
		static SecurityPolicyVersion ()
		{
			WSSecurityPolicy11 = new SecurityPolicyVersionImpl () { Prefix = "wsp", Namespace = "http://schemas.xmlsoap.org/ws/2004/09/policy" };
			WSSecurityPolicy12 = new SecurityPolicyVersionImpl () { Prefix = "wsp", Namespace = "http://docs.oasis-open.org/ws-sx/ws-securitypolicy/200702" };
		}

		public static SecurityPolicyVersion WSSecurityPolicy11 { get; private set; }
		public static SecurityPolicyVersion WSSecurityPolicy12 { get; private set; }

		public string Namespace { get; internal set; }
		public string Prefix { get; internal set; }
	}

	class SecurityPolicyVersionImpl : SecurityPolicyVersion
	{
	}
}
