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
		#region PROPERTIES

		public TlsCipherSuite this[string name] 
		{
			get { return (TlsCipherSuite)this[IndexOf(name)]; }
			set { this[IndexOf(name)] = (TlsCipherSuite)value; }
		}

		public TlsCipherSuite this[short code] 
		{
			get { return (TlsCipherSuite)base[IndexOf(code)]; }
			set { base[IndexOf(code)] = (TlsCipherSuite)value; }
		}

		public new TlsCipherSuite this[int code] 
		{
			get { return (TlsCipherSuite)base[code]; }
			set { base[code] = (TlsCipherSuite)value; }
		}

		#endregion

		#region STATIC_METHODS

		public static TlsCipherSuiteCollection GetSupportedCipherSuiteCollection()
		{
			TlsCipherSuiteCollection scs = new TlsCipherSuiteCollection();

			// Supported ciphers
			scs.Add((0x00 << 0x08) | 0x0A, "TLS_RSA_WITH_3DES_EDE_CBC_SHA", "3DES", "SHA", false, true, 24, 24, 168, 8, 8);
			scs.Add((0x00 << 0x08) | 0x09, "TLS_RSA_WITH_DES_CBC_SHA", "DES", "SHA", false, true, 8, 8, 56, 8, 8);
			scs.Add((0x00 << 0x08) | 0x05, "TLS_RSA_WITH_RC4_128_SHA", "RC4", "SHA", false, false, 16, 16, 128, 0, 0);
			scs.Add((0x00 << 0x08) | 0x04, "TLS_RSA_WITH_RC4_128_MD5", "RC4", "MD5", false, false, 16, 16, 128, 0, 0);			
			
			// Default CipherSuite
			// scs.Add(0, "TLS_NULL_WITH_NULL_NULL", "", "", true, false, 0, 0, 0, 0, 0);
			
			// RSA Cipher Suites
			// scs.Add((0x00 << 0x08) | 0x01, "TLS_RSA_WITH_NULL_MD5", "", "MD5", true, false, 0, 0, 0, 0, 0);
			// scs.Add((0x00 << 0x08) | 0x02, "TLS_RSA_WITH_NULL_SHA", "", "SHA", true, false, 0, 0, 0, 0, 0);
			// scs.Add((0x00 << 0x08) | 0x03, "TLS_RSA_EXPORT_WITH_RC4_40_MD5", "RC4", "MD5", true, false, 5, 16, 40, 0, 0);
			// scs.Add((0x00 << 0x08) | 0x05, "TLS_RSA_WITH_RC4_128_SHA", "RC4", "SHA", false, false, 16, 16, 128, 0, 0);
			// scs.Add((0x00 << 0x08) | 0x04, "TLS_RSA_WITH_RC4_128_MD5", "RC4", "MD5", false, false, 16, 16, 128, 0, 0);			
			// scs.Add((0x00 << 0x08) | 0x06, "TLS_RSA_EXPORT_WITH_RC2_CBC_40_MD5", "RC2", "MD5", true, true, 5, 16, 40, 8, 8);
			// scs.Add((0x00 << 0x08) | 0x07, "TLS_RSA_WITH_IDEA_CBC_SHA", "IDEA", "SHA", false, true, 16, 16, 128, 8, 8);
			// scs.Add((0x00 << 0x08) | 0x08, "TLS_RSA_EXPORT_WITH_DES40_CBC_SHA", "DES", "SHA", true, true, 5, 8, 40, 8, 8);
			// scs.Add((0x00 << 0x08) | 0x09, "TLS_RSA_WITH_DES_CBC_SHA", "DES", "SHA", false, true, 8, 8, 56, 8, 8);
			// scs.Add((0x00 << 0x08) | 0x0A, "TLS_RSA_WITH_3DES_EDE_CBC_SHA", "3DES", "SHA", false, true, 24, 24, 168, 8, 8);
			
			// Diffie-Hellman Cipher Suites
			// scs.Add((0x00 << 0x08) | 0x0B, "TLS_DH_DSS_EXPORT_WITH_DES40_CBC_SHA", "DES", "SHA", true, true, 5, 8, 40, 8, 8);
			// scs.Add((0x00 << 0x08) | 0x0C, "TLS_DH_DSS_WITH_DES_CBC_SHA", "DES", "SHA", false, true, 8, 8, 56, 8, 8);
			// scs.Add((0x00 << 0x08) | 0x0D, "TLS_DH_DSS_WITH_3DES_EDE_CBC_SHA", "3DES", "SHA", false, true, 24, 24, 168, 8, 8);
			// scs.Add((0x00 << 0x08) | 0x0E, "TLS_DH_RSA_EXPORT_WITH_DES40_CBC_SHA", "DES", "SHA", true, true, 5, 8, 40, 8, 8);
			// scs.Add((0x00 << 0x08) | 0x0F, "TLS_DH_RSA_WITH_DES_CBC_SHA", "DES", "SHA", false, true, 8, 8, 56, 8, 8);
			// scs.Add((0x00 << 0x08) | 0x10, "TLS_DH_RSA_WITH_3DES_EDE_CBC_SHA", "3DES", "SHA", false, true, 24, 24, 168, 8, 8);
			// scs.Add((0x00 << 0x08) | 0x11, "TLS_DHE_DSS_EXPORT_WITH_DES40_CBC_SHA", "DES", "SHA", true, true, 5, 8, 40, 8, 8);
			// scs.Add((0x00 << 0x08) | 0x12, "TLS_DHE_DSS_WITH_DES_CBC_SHA", "DES", "SHA", false, true, 8, 8, 56, 8, 8);
			// scs.Add((0x00 << 0x08) | 0x13, "TLS_DHE_DSS_WITH_3DES_EDE_CBC_SHA", "3DES", "SHA", false, true, 24, 24, 168, 8, 8);
			// scs.Add((0x00 << 0x08) | 0x14, "TLS_DHE_RSA_EXPORT_WITH_DES40_CBC_SHA", "DES", "SHA", true, true, 5, 8, 40, 8, 8);
			// scs.Add((0x00 << 0x08) | 0x15, "TLS_DHE_RSA_WITH_DES_CBC_SHA", "SHA", "DES", false, true, 8, 8, 56, 8, 8);
			// scs.Add((0x00 << 0x08) | 0x16, "TLS_DHE_RSA_WITH_3DES_EDE_CBC_SHA", "3DES", "SHA", false, true, 24, 24, 168, 8, 8);

			// Anonymous Diffie-Hellman Cipher Suites
			// scs.Add((0x00 << 0x08) | 0x17, "TLS_DH_anon_EXPORT_WITH_RC4_40_MD5", "RC4", "MD5", true, false, 5, 16, 40, 0, 0);
			// scs.Add((0x00 << 0x08) | 0x18, "TLS_DH_anon_WITH_RC4_128_MD5", "RC4", "MD5", false, false, 16, 16, 128, 0, 0);
			// scs.Add((0x00 << 0x08) | 0x19, "TLS_DH_anon_EXPORT_WITH_DES40_CBC_SHA", "DES", "SHA", false, true, 5, 8, 40, 8, 8);
			// scs.Add((0x00 << 0x08) | 0x1A, "TLS_DH_anon_WITH_DES_CBC_SHA", "DES4", "SHA", false, true, 8, 8, 56, 8, 8);
			// scs.Add((0x00 << 0x08) | 0x1B, "TLS_DH_anon_WITH_3DES_EDE_CBC_SHA", "3DES", "SHA", false, true, 24, 24, 168, 8, 8);

			return scs;
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
			foreach(TlsCipherSuite suite in this)
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
			foreach(TlsCipherSuite suite in this)
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

		public TlsCipherSuite Add(TlsCipherSuite cipherSuite)
		{
			base.Add(cipherSuite);

			return cipherSuite;
		}

		public TlsCipherSuite Add(short code, string name, string algName, string hashName, bool exportable, bool blockMode, byte keyMaterialSize, byte expandedKeyMaterialSize, byte effectiveKeyBytes, byte ivSize, byte blockSize)
		{
			TlsCipherSuite cipherSuite = new TlsCipherSuite(code, name, algName, hashName, exportable, blockMode, keyMaterialSize, expandedKeyMaterialSize, effectiveKeyBytes, ivSize, blockSize);

			return Add(cipherSuite);
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
