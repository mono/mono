//
// KeyInfoEncryptedKey.cs - KeyInfoEncryptedKey implementation for XML Encryption
// http://www.w3.org/2001/04/xmlenc#sec-EncryptedKey
//
// Author:
//     Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2004
//

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

using System.Xml;

namespace System.Security.Cryptography.Xml {

	public class KeyInfoEncryptedKey : KeyInfoClause {

		#region Fields

		EncryptedKey encryptedKey;

		#endregion // Fields

		#region Constructors

		public KeyInfoEncryptedKey ()
		{
		}

		public KeyInfoEncryptedKey (EncryptedKey ek)
		{
			EncryptedKey = ek;
		}

		#endregion // Constructors

		#region Properties

		public EncryptedKey EncryptedKey {
			get { return encryptedKey; }
			set { encryptedKey = value; }
		}

		#endregion // Properties

		#region Methods

		public override XmlElement GetXml ()
		{
			return GetXml (new XmlDocument ());
		}

		internal XmlElement GetXml (XmlDocument document)
		{
			if (encryptedKey != null)
				return encryptedKey.GetXml (document);
			return null;
		}

		public override void LoadXml (XmlElement value)
		{
			EncryptedKey = new EncryptedKey ();
			EncryptedKey.LoadXml (value);
		}

		#endregion // Methods
	}
}

#endif
