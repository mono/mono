//
// Pkcs9MessageDigest.cs - System.Security.Cryptography.Pkcs.Pkcs9MessageDigest
//
// Author:
//	Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2004
// Copyright (C) 2004 Novell Inc. (http://www.novell.com)
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

#if NET_2_0

using System;

namespace System.Security.Cryptography.Pkcs {

	public sealed class Pkcs9MessageDigest : Pkcs9Attribute 
	{
		#region Fields

		const string oid = "1.2.840.113549.1.9.4";
		const string name = "Message Digest";
		byte[] messageDigest;

		#endregion // Fields

		#region Constructors

		[MonoTODO ("encode for RawData using Mono.Security")]
		public Pkcs9MessageDigest () 
			: base (new Oid (oid, name), null)
		{
		}

		#endregion // Constructors

		#region Properties

		public byte[] MessageDigest {
			get { return messageDigest; }
		}

		#endregion // Properties
	}
}

#endif
