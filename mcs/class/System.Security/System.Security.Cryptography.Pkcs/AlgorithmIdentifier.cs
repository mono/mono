//
// AlgorithmIdentifier.cs - System.Security.Cryptography.Pkcs.AlgorithmIdentifier
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
// Copyright (C) 2004-2005 Novell, Inc (http://www.novell.com)
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

#if NET_2_0 && SECURITY_DEP

namespace System.Security.Cryptography.Pkcs {

	public sealed class AlgorithmIdentifier {

		private Oid _oid;
		private int _length;
		private byte[] _params;

		// constructors

		public AlgorithmIdentifier ()
		{
			_oid = new Oid ("1.2.840.113549.3.7", "3des");
			_params = new byte [0];
		}

		public AlgorithmIdentifier (Oid algorithm)
		{
			_oid = algorithm;
			_params = new byte [0];
		}

		public AlgorithmIdentifier (Oid algorithm, int keyLength)
		{
			_oid = algorithm;
			_length = keyLength;
			_params = new byte [0];
		}

		// properties

		public int KeyLength { 
			get { return _length; }
			set { _length = value; }
		}

		public Oid Oid {
			get { return _oid; }
			set { _oid = value; }
		} 

		public byte[] Parameters { 
			get { return _params; }
			set { _params = value; }
		} 
	}
}

#endif
