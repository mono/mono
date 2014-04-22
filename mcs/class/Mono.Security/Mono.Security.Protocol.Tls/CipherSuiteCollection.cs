// Transport Security Layer (TLS)
// Copyright (c) 2003-2004 Carlos Guzman Alvarez
// Copyright 2013-2014 Xamarin Inc.

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
using System.Collections.Generic;

namespace Mono.Security.Protocol.Tls {

	internal sealed class CipherSuiteCollection : List<CipherSuite> {

		#region Fields

		SecurityProtocolType protocol;

		#endregion

		#region Indexers

		public CipherSuite this [string name]  {
			get {
				int n = IndexOf (name);
				return n == -1 ? null : this [n];
			}
		}

		public CipherSuite this [short code] {
			get {
				int n = IndexOf (code);
				return n == -1 ? null : this [n];
			}
		}

		#endregion

		#region Constructors

		public CipherSuiteCollection (SecurityProtocolType protocol)
		{
			switch (protocol) {
			case SecurityProtocolType.Default:
			case SecurityProtocolType.Tls:
			case SecurityProtocolType.Ssl3:
				this.protocol = protocol;
				break;
			case SecurityProtocolType.Ssl2:
			default:
				throw new NotSupportedException ("Unsupported security protocol type.");
			}
		}

		#endregion

		#region Methods

		public int IndexOf (string name)
		{
			int index = 0;
			foreach (CipherSuite cipherSuite in this) {
				if (String.CompareOrdinal (name, cipherSuite.Name) == 0)
					return index;
				index++;
			}
			return -1;
		}

		public int IndexOf (short code)
		{
			int index = 0;
			foreach (CipherSuite cipherSuite in this) {
				if (cipherSuite.Code == code)
					return index;
				index++;
			}
			return -1;
		}

		public void Add (
			short code, string name, CipherAlgorithmType cipherType, 
			HashAlgorithmType hashType, ExchangeAlgorithmType exchangeType,
			bool exportable, bool blockMode, byte keyMaterialSize, 
			byte expandedKeyMaterialSize, short effectiveKeyBytes, 
			byte ivSize, byte blockSize)
		{
			switch (protocol) {
			case SecurityProtocolType.Default:
			case SecurityProtocolType.Tls:
				Add (new TlsCipherSuite (code, name, cipherType, hashType, exchangeType, exportable, blockMode, 
					keyMaterialSize, expandedKeyMaterialSize, effectiveKeyBytes, ivSize, blockSize));
				break;

			case SecurityProtocolType.Ssl3:
				Add (new SslCipherSuite (code, name, cipherType, hashType, exchangeType, exportable, blockMode, 
					keyMaterialSize, expandedKeyMaterialSize, effectiveKeyBytes, ivSize, blockSize));
				break;
			}
		}

		public IList<string> GetNames ()
		{
			var list = new List<string> (Count);
			foreach (CipherSuite cipherSuite in this)
				list.Add (cipherSuite.Name);
			return list;
		}

		#endregion
	}
}
