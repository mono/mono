//
// EncryptedReference.cs - EncryptedReference implementation for XML Encryption
// http://www.w3.org/2001/04/xmlenc#sec-EncryptedReference
//
// Author:
//      Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2004

#if NET_1_2

using System.Xml;

namespace System.Security.Cryptography.Xml {
	public abstract class EncryptedReference {

		#region Fields

		bool cacheValid;
		string referenceType;
		string uri;
		TransformChain tc;

		#endregion // Fields

		#region Constructors

		protected EncryptedReference ()
		{
			uri = null;
			TransformChain = new TransformChain ();
		}
	
		protected EncryptedReference (string uri)
		{
			Uri = uri;
			TransformChain = new TransformChain ();
		}
	
		protected EncryptedReference (string uri, TransformChain tc)
			: this ()
		{
			Uri = uri;
			TransformChain = tc;
		}
	
		#endregion // Constructors

		#region Properties

		[MonoTODO()]
		protected internal bool CacheValid {
			get { return cacheValid; }
		}

		[MonoTODO]
		protected string ReferenceType {
			get { return referenceType; }
		}

		public TransformChain TransformChain {
			get { return tc; }
			set { tc = value; }
		}

		public string Uri {
			get { return uri; }
			set { uri = value; }
		}

		#endregion // Properties
	
		#region Methods

		public void AddTransform (Transform transform)
		{
			TransformChain.Add (transform);
		}

		public virtual XmlElement GetXml ()
		{
			return GetXml (new XmlDocument ());
		}

		internal virtual XmlElement GetXml (XmlDocument document)
		{
			return document.CreateElement ("", "");
		}

		public virtual void LoadXml (XmlElement value)
		{
		}

		#endregion // Methods
	}
}

#endif
