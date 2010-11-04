//
// MsmqTransportSecurity.cs
//
// Author: Atsushi Enomoto  <atsushi@ximian.com>
//
// Copyright (C) 2007 Novell, Inc (http://www.novell.com)
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
using System.Net.Security;

namespace System.ServiceModel
{
	public sealed class MsmqTransportSecurity
	{
		MsmqAuthenticationMode auth = MsmqAuthenticationMode.WindowsDomain;
		MsmqEncryptionAlgorithm enc;
		MsmqSecureHashAlgorithm hash = MsmqSecureHashAlgorithm.Sha1;
		ProtectionLevel protection_level = ProtectionLevel.Sign;

		public MsmqTransportSecurity ()
		{
		}

		public MsmqTransportSecurity (MsmqTransportSecurity other)
		{
		}

		public MsmqAuthenticationMode MsmqAuthenticationMode {
			get { return auth; }
			set { auth = value; }
		}

		public MsmqEncryptionAlgorithm MsmqEncryptionAlgorithm {
			get { return enc; }
			set { enc = value; }
		}

		public MsmqSecureHashAlgorithm MsmqSecureHashAlgorithm {
			get { return hash; }
			set { hash = value; }
		}

		public ProtectionLevel MsmqProtectionLevel {
			get { return protection_level; }
			set { protection_level = value; }
		}
	}
}
