//
// XmlDecryptionTransform.cs - XmlDecryptionTransform implementation for XML Encryption
//
// Author:
//      Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2004

#if NET_1_2

using System.Xml;

namespace System.Security.Cryptography.Xml {
	public class XmlDecryptionTransform : Transform {

		#region Fields

		EncryptedXml encryptedXml;
		Type[] inputTypes;
		Type[] outputTypes;

		#endregion // Fields

		#region Constructors
	
		public XmlDecryptionTransform ()
			: base ()
		{
		}
	
		#endregion // Constructors

		#region Properties

		public EncryptedXml EncryptedXml {
			get { return encryptedXml; }
			set { encryptedXml = value; }
		}

		public override Type[] InputTypes {
			get { 
				if (inputTypes == null) {
					lock (this) {
						inputTypes = new Type [3] {typeof (System.IO.Stream), typeof (System.Xml.XmlNodeList), typeof (System.Xml.XmlDocument)}; 
					}
				}
				return inputTypes;
			}
		}

		public override Type[] OutputTypes {
			get { 
				if (outputTypes == null) {
					lock (this) {
						outputTypes = new Type [2] {typeof (System.Xml.XmlDocument), typeof (System.Xml.XmlNodeList)};
					}
				}
				return outputTypes;
			}
		}

		#endregion // Properties

		#region Methods

		[MonoTODO]
		protected override XmlNodeList GetInnerXml ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override object GetOutput ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override object GetOutput (Type type)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected virtual bool IsTargetElement (XmlElement inputElement, string idValue)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override void LoadInnerXml (XmlNodeList nodeList)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override void LoadInput (object obj)
		{
			throw new NotImplementedException ();
		}

		#endregion // Methods
	}
}

#endif
