//
// System.Security.Cryptography.X509Certificates.X509KeyUsageExtension
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

namespace System.Security.Cryptography.X509Certificates {

	public sealed class X509KeyUsageExtension : X509Extension 
	{
		#region Fields

		X509KeyUsageFlags keyUsages;

		#endregion // Fields

		#region Constructors

		[MonoTODO]
		public X509KeyUsageExtension ()
		{
		}

		[MonoTODO]
		public X509KeyUsageExtension (AsnEncodedData encodedKeyUsage, bool critical)
		{
		}

		[MonoTODO]
		public X509KeyUsageExtension (X509KeyUsageFlags keyUsages, bool critical)
		{
			this.keyUsages = keyUsages;
		}

		#endregion // Constructors

		#region Properties

		public X509KeyUsageFlags KeyUsages {
			get { return keyUsages; }
		}

		#endregion // Properties

		#region Methods

		[MonoTODO]
		public override void CopyFrom (AsnEncodedData encodedData)
		{
			throw new NotImplementedException ();
		}

		#endregion // Methods
	}
}

#endif
