//
// EncryptedReference.cs - EncryptedReference implementation for XML Encryption
// http://www.w3.org/2001/04/xmlenc#sec-EncryptedReference
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
	public abstract class EncryptedReference {

		#region Fields

		string referenceType;
		string uri;
		TransformChain tc;

		#endregion // Fields

		#region Constructors

		protected EncryptedReference ()
		{
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

		[MonoTODO("Always returns false")]
		protected internal bool CacheValid {
			get { return false; }
		}

		protected string ReferenceType {
			get { return referenceType; }
			set { referenceType = value; }
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
			XmlElement xel = document.CreateElement (ReferenceType, EncryptedXml.XmlEncNamespaceUrl);

			xel.SetAttribute (XmlEncryption.AttributeNames.URI, Uri);

                        if (TransformChain != null && TransformChain.Count > 0) {
                                XmlElement xtr = document.CreateElement (XmlEncryption.ElementNames.Transforms, EncryptedXml.XmlEncNamespaceUrl);
                                foreach (Transform t in TransformChain)
                                        xtr.AppendChild (document.ImportNode (t.GetXml (), true));
                                xel.AppendChild (xtr);
                        }

			return xel;
		}

		[MonoTODO ("Make compliant.")]
		public virtual void LoadXml (XmlElement value)
		{
			if (value == null)
				throw new ArgumentNullException ("value");
			else {
				Uri = null;
				TransformChain = new TransformChain ();

				foreach (XmlNode n in value.ChildNodes) {
					if (n is XmlWhitespace)
						continue;

					switch (n.LocalName) {
					case XmlEncryption.ElementNames.Transforms:
						foreach (XmlNode xn in ((XmlElement) n).GetElementsByTagName (XmlSignature.ElementNames.Transform, XmlSignature.NamespaceURI)) {
							Transform t = null;
							switch (((XmlElement) xn).Attributes [XmlSignature.AttributeNames.Algorithm].Value) {
							case XmlSignature.AlgorithmNamespaces.XmlDsigBase64Transform:
								t = new XmlDsigBase64Transform ();
								break;
							case XmlSignature.AlgorithmNamespaces.XmlDsigC14NTransform:
								t = new XmlDsigC14NTransform ();
								break;
							case XmlSignature.AlgorithmNamespaces.XmlDsigC14NWithCommentsTransform:
								t = new XmlDsigC14NWithCommentsTransform ();
								break;
							case XmlSignature.AlgorithmNamespaces.XmlDsigEnvelopedSignatureTransform:
								t = new XmlDsigEnvelopedSignatureTransform ();
								break;
							case XmlSignature.AlgorithmNamespaces.XmlDsigXPathTransform:
								t = new XmlDsigXPathTransform ();
								break;
							case XmlSignature.AlgorithmNamespaces.XmlDsigXsltTransform:
								t = new XmlDsigXsltTransform ();
								break;
#if NET_2_0
							case XmlSignature.AlgorithmNamespaces.XmlDsigExcC14NTransform:
								t = new XmlDsigExcC14NTransform ();
								break;
							case XmlSignature.AlgorithmNamespaces.XmlDsigExcC14NWithCommentsTransform:
								t = new XmlDsigExcC14NWithCommentsTransform ();
								break;
							case XmlSignature.AlgorithmNamespaces.XmlDecryptionTransform:
								t = new XmlDecryptionTransform ();
								break;
#endif
							default:
								continue;
							}

							t.LoadInnerXml (((XmlElement) xn).ChildNodes);
							TransformChain.Add (t);
						}
						break;
					}
				}

				if (value.HasAttribute (XmlEncryption.AttributeNames.URI))
					Uri = value.Attributes [XmlEncryption.AttributeNames.URI].Value;
			}
		}

		#endregion // Methods
	}
}

#endif
