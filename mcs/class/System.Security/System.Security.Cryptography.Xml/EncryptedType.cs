//
// EncryptedType.cs - EncryptedType implementation for XML Encryption
// http://www.w3.org/2001/04/xmlenc#sec-EncryptedType
//
// Author:
//      Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2004

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
	public abstract class EncryptedType {

		#region Fields

		CipherData cipherData;
		string encoding;
		EncryptionMethod encryptionMethod;
		EncryptionPropertyCollection encryptionProperties;
		string id;
		KeyInfo keyInfo;
		string mimeType;
		string type;
	
		#endregion // Fields
	
		#region Constructors

		protected EncryptedType ()
		{
			cipherData = new CipherData ();
			encryptionProperties = new EncryptionPropertyCollection ();
			keyInfo = new KeyInfo ();
		}
	
		#endregion // Constructors
	
		#region Properties

		public virtual CipherData CipherData {
			get { return cipherData; }
			set { cipherData = value; }
		}

		public virtual string Encoding {
			get { return encoding; }
			set { encoding = value; }
		}

		public virtual EncryptionMethod EncryptionMethod {
			get { return encryptionMethod; }
			set { encryptionMethod = value; }
		}

		public virtual EncryptionPropertyCollection EncryptionProperties {
			get { return encryptionProperties; }
		}

		public virtual string Id {
			get { return id; }
			set { id = value; }
		}

		public KeyInfo KeyInfo {
			get { return keyInfo; }
			set { keyInfo = value; }
		}

		public virtual string MimeType {
			get { return mimeType; }
			set { mimeType = value; }
		}

		public virtual string Type {
			get { return type; }
			set { type = value; }
		}

		#endregion // Properties

		#region Methods

		public void AddProperty (EncryptionProperty ep)
		{
			EncryptionProperties.Add (ep);
		}

		public abstract XmlElement GetXml ();
		public abstract void LoadXml (XmlElement value);

		#endregion // Methods
	}
}

#endif
