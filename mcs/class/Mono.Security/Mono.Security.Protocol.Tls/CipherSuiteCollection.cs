// Transport Security Layer (TLS)
// Copyright (c) 2003-2004 Carlos Guzman Alvarez

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
using System.Collections;
using System.Globalization;
using System.Security.Cryptography;

namespace Mono.Security.Protocol.Tls
{
	internal sealed class CipherSuiteCollection : ICollection, IList, IEnumerable
	{
		#region Fields

		private ArrayList				cipherSuites;
		private SecurityProtocolType	protocol;

		#endregion

		#region Indexers

		public CipherSuite this[string name] 
		{
			get { return (CipherSuite)this.cipherSuites[this.IndexOf(name)]; }
			set { this.cipherSuites[this.IndexOf(name)] = (CipherSuite)value; }
		}

		public CipherSuite this[int index] 
		{
			get { return (CipherSuite)this.cipherSuites[index]; }
			set { this.cipherSuites[index] = (CipherSuite)value; }
		}

		public CipherSuite this[short code] 
		{
			get { return (CipherSuite)this.cipherSuites[this.IndexOf(code)]; }
			set { this.cipherSuites[this.IndexOf(code)] = (CipherSuite)value; }
		}

		object IList.this[int index]
		{
			get { return this[index]; }
			set { this[index] = (CipherSuite)value; }
		}

		#endregion

		#region ICollection Properties

		bool ICollection.IsSynchronized
		{
			get { return this.cipherSuites.IsSynchronized; }
		}

		object ICollection.SyncRoot 
		{
			get { return this.cipherSuites.SyncRoot; }
		}

		public int Count 
		{
			get { return this.cipherSuites.Count; }
		}
		
		#endregion

		#region IList Properties

		public bool IsFixedSize 
		{
			get { return this.cipherSuites.IsFixedSize; }
		}

		public bool IsReadOnly
		{
			get { return this.cipherSuites.IsReadOnly; }
		}

		#endregion

		#region Constructors

		public CipherSuiteCollection(SecurityProtocolType protocol) : base()
		{
			this.protocol		= protocol;
			this.cipherSuites	= new ArrayList();
		}

		#endregion

		#region ICollection Methods

		public void CopyTo(Array array, int index)
		{
			this.cipherSuites.CopyTo(array, index);
		}
		
		#endregion

		#region IEnumerable Methods

		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.cipherSuites.GetEnumerator();
		}

		#endregion

		#region IList Methods

		public void Clear()
		{
			this.cipherSuites.Clear();
		}
			
		bool IList.Contains(object value)
		{
			return this.cipherSuites.Contains(value as CipherSuite);
		}

		public int IndexOf(string name)
		{
			int index = 0;

			foreach (CipherSuite cipherSuite in this.cipherSuites)
			{
				if (this.cultureAwareCompare(cipherSuite.Name, name))
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

			foreach (CipherSuite cipherSuite in this.cipherSuites)
			{
				if (cipherSuite.Code == code)
				{
					return index;
				}
				index++;
			}

			return -1;
		}

		int IList.IndexOf(object value)
		{
			return this.cipherSuites.IndexOf(value as CipherSuite);
		}

		void IList.Insert(int index, object value)
		{
			this.cipherSuites.Insert(index, value as CipherSuite);
		}

		void IList.Remove(object value)
		{
			this.cipherSuites.Remove(value as CipherSuite);
		}

		void IList.RemoveAt(int index)
		{
			this.cipherSuites.RemoveAt(index);
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
				case SecurityProtocolType.Default:
				case SecurityProtocolType.Tls:
					return this.add(
						new TlsCipherSuite(
						code, name, cipherType, hashType, exchangeType, exportable, 
						blockMode, keyMaterialSize, expandedKeyMaterialSize, 
						effectiveKeyBytes, ivSize, blockSize));

				case SecurityProtocolType.Ssl3:
					return this.add(
						new SslCipherSuite(
						code, name, cipherType, hashType, exchangeType, exportable, 
						blockMode, keyMaterialSize, expandedKeyMaterialSize, 
						effectiveKeyBytes, ivSize, blockSize));

				case SecurityProtocolType.Ssl2:
				default:
					throw new NotSupportedException("Unsupported security protocol type.");
			}
		}

		private TlsCipherSuite add(TlsCipherSuite cipherSuite)
		{
			this.cipherSuites.Add(cipherSuite);

			return cipherSuite;
		}

		private SslCipherSuite add(SslCipherSuite cipherSuite)
		{
			this.cipherSuites.Add(cipherSuite);

			return cipherSuite;
		}

		int IList.Add(object value)
		{
			return this.cipherSuites.Add(value as CipherSuite);
		}
		
		private bool cultureAwareCompare(string strA, string strB)
		{
			return CultureInfo.CurrentCulture.CompareInfo.Compare(
				strA, 
				strB, 
				CompareOptions.IgnoreKanaType | CompareOptions.IgnoreWidth | 
				CompareOptions.IgnoreCase) == 0 ? true : false;
		}

		#endregion
	}
}
