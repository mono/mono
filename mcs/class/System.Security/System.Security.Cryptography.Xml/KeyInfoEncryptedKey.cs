//
// KeyInfoEncryptedKey.cs - KeyInfoEncryptedKey implementation for XML Encryption
// http://www.w3.org/2001/04/xmlenc#sec-EncryptedKey
//
// Author:
//     Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2004
//

#if NET_1_2

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

		[MonoTODO]
		internal XmlElement GetXml (XmlDocument document)
		{
			if (encryptedKey != null)
				return encryptedKey.GetXml (document);
			return null;
		}

		[MonoTODO]
		public override void LoadXml (XmlElement value)
		{
			EncryptedKey = new EncryptedKey ();
			EncryptedKey.LoadXml (value);
		}

		#endregion // Methods
	}
}

#endif
