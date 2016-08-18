//
// MessageSecurityOverTcp.cs
//
// Author: Atsushi Enomoto (atsushi@ximian.com)
//
// Copyright (C) 2006 Novell, Inc (http://www.novell.com)
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
using System.ServiceModel.Security;

namespace System.ServiceModel
{
	public sealed class MessageSecurityOverTcp
	{
#if !MOBILE && !XAMMAC_4_5
		SecurityAlgorithmSuite alg_suite;
#endif
		MessageCredentialType client_credential_type;

		public MessageSecurityOverTcp ()
		{
#if !MOBILE && !XAMMAC_4_5
			alg_suite = SecurityAlgorithmSuite.Default;
#endif
			// This default value is *silly* but anyways
			// such code that does not change this ClientCredentialType 
			// won't work on Mono.
			client_credential_type = MessageCredentialType.Windows;
		}

#if !MOBILE && !XAMMAC_4_5
		public SecurityAlgorithmSuite AlgorithmSuite {
			get { return alg_suite; }
			set { alg_suite = value; }
		}
#endif

		public MessageCredentialType ClientCredentialType {
			get { return client_credential_type; }
			set { client_credential_type = value; }
		}
	}
}
