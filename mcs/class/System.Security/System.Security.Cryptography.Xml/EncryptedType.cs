//
// EncryptedType.cs - EncryptedType implementation for XML Encryption
// http://www.w3.org/2001/04/xmlenc#sec-EncryptedType
//
// Author:
//      Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2004

#if NET_1_2

using System.Xml;

namespace System.Security.Cryptography.Xml {
	public abstract class EncryptedType {

		#region Fields

		CipherData cipherData;
		string encoding;
		EncryptionMethod encryptionMethod;
		EncryptionProperties encryptionProperties;
		string id;
		KeyInfo keyInfo;
		string mimeType;
		string type;
	
		#endregion // Fields
	
		#region Constructors

		protected EncryptedType ()
		{
			cipherData = null;
			encoding = null;
			encryptionMethod = null;
			encryptionProperties = new EncryptionProperties ();
			id = null;
			keyInfo = null;
			mimeType = null;
			type = null;
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

		public virtual EncryptionProperties EncryptionProperties {
			get { return encryptionProperties; }
			set { encryptionProperties = value; }
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
