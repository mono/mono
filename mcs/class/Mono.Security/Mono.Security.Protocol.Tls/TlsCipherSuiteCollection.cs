/* Transport Security Layer (TLS)
 * Copyright (c) 2003 Carlos Guzmán Álvarez
 * 
 * Permission is hereby granted, free of charge, to any person 
 * obtaining a copy of this software and associated documentation 
 * files (the "Software"), to deal in the Software without restriction, 
 * including without limitation the rights to use, copy, modify, merge, 
 * publish, distribute, sublicense, and/or sell copies of the Software, 
 * and to permit persons to whom the Software is furnished to do so, 
 * subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included 
 * in all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, 
 * EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES 
 * OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND 
 * NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT 
 * HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, 
 * WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, 
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
 * DEALINGS IN THE SOFTWARE.
 */

using System;
using System.Collections;
using System.Globalization;
using System.Security.Cryptography;

namespace Mono.Security.Protocol.Tls
{
	internal sealed class TlsCipherSuiteCollection : ArrayList
	{
		#region FIELDS

		private SecurityProtocolType protocol;

		#endregion

		#region PROPERTIES

		public CipherSuite this[string name] 
		{
			get { return (CipherSuite)this[IndexOf(name)]; }
			set { this[IndexOf(name)] = (CipherSuite)value; }
		}

		public CipherSuite this[short code] 
		{
			get { return (CipherSuite)base[IndexOf(code)]; }
			set { base[IndexOf(code)] = (CipherSuite)value; }
		}

		public new CipherSuite this[int code] 
		{
			get { return (CipherSuite)base[code]; }
			set { base[code] = (CipherSuite)value; }
		}

		#endregion

		#region CONSTRUCTORS

		public TlsCipherSuiteCollection(SecurityProtocolType protocol) : base()
		{
			this.protocol = protocol;
		}

		#endregion

		#region METHODS
	
		public bool Contains(string name)
		{
			return(-1 != IndexOf(name));
		}
		
		public int IndexOf(string name)
		{
			int index = 0;
			foreach (CipherSuite suite in this)
			{
				if (cultureAwareCompare(suite.Name, name))
				{
					return index;
				}
				index++;
			}
			return -1;
		}

		public int IndexOf(short code)
		{
			int index = 0;
			foreach (CipherSuite suite in this)
			{
				if (suite.Code == code)
				{
					return index;
				}
				index++;
			}
			return -1;
		}

		public void RemoveAt(string errorMessage)
		{
			RemoveAt(IndexOf(errorMessage));
		}

		public CipherSuite Add(
			short code, string name, CipherAlgorithmType cipherType, 
			HashAlgorithmType hashType, ExchangeAlgorithmType exchangeType,
			bool exportable, bool blockMode, byte keyMaterialSize, 
			byte expandedKeyMaterialSize, short effectiveKeyBytes, 
			byte ivSize, byte blockSize)
		{
			switch (this.protocol)
			{
				case SecurityProtocolType.Ssl3:
					return this.add(
						new TlsSslCipherSuite(
						code, name, cipherType, hashType, exchangeType, exportable, 
						blockMode, keyMaterialSize, expandedKeyMaterialSize, 
						effectiveKeyBytes, ivSize, blockSize));

				case SecurityProtocolType.Tls:
					return this.add(
						new TlsCipherSuite(
						code, name, cipherType, hashType, exchangeType, exportable, 
						blockMode, keyMaterialSize, expandedKeyMaterialSize, 
						effectiveKeyBytes, ivSize, blockSize));

				default:
					throw new NotSupportedException();
			}
		}

		private TlsCipherSuite add(TlsCipherSuite cipherSuite)
		{
			base.Add(cipherSuite);

			return cipherSuite;
		}

		private TlsSslCipherSuite add(TlsSslCipherSuite cipherSuite)
		{
			base.Add(cipherSuite);

			return cipherSuite;
		}

		private bool cultureAwareCompare(string strA, string strB)
		{
			try
			{
				return CultureInfo.CurrentCulture.CompareInfo.Compare(strA, strB, CompareOptions.IgnoreKanaType | CompareOptions.IgnoreWidth | CompareOptions.IgnoreCase) == 0 ? true : false;
			}
			catch (NotSupportedException)
			{
				return strA.ToUpper() == strB.ToUpper() ? true : false;
			}
		}

		#endregion
	}
}
