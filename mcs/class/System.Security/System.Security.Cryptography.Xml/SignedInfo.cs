//
// SignedInfo.cs - SignedInfo implementation for XML Signature
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2002, 2003 Motus Technologies Inc. (http://www.motus.com)
//

using System.Collections;
using System.Xml;

namespace System.Security.Cryptography.Xml { 

	public class SignedInfo : ICollection, IEnumerable {

		private ArrayList references;
		private string c14nMethod;
		private string id;
		private string signatureMethod;
		private string signatureLength;

		public SignedInfo() 
		{
			references = new ArrayList ();
			c14nMethod = "http://www.w3.org/TR/2001/REC-xml-c14n-20010315";
		}

		public string CanonicalizationMethod {
			get { return c14nMethod; }
			set { c14nMethod = value; }
		}

		// documented as not supported (and throwing exception)
		public int Count {
			get { throw new NotSupportedException (); }
		}

		public string Id {
			get { return id; }
			set { id = value; }
		}

		// documented as not supported (and throwing exception)
		public bool IsReadOnly {
			get { throw new NotSupportedException (); }
		}

		// documented as not supported (and throwing exception)
		public bool IsSynchronized {
			get { throw new NotSupportedException (); }
		}

		public ArrayList References {
			get { return references; }
		}

		public string SignatureLength {
			get { return signatureLength; }
			set { signatureLength = value; }
		}

		public string SignatureMethod {
			get { return signatureMethod; }
			set { signatureMethod = value; }
		}

		// documented as not supported (and throwing exception)
		public object SyncRoot {
			get { throw new NotSupportedException (); }
		}

		public void AddReference (Reference reference) 
		{
			references.Add (reference);
		}

		// documented as not supported (and throwing exception)
		public void CopyTo (Array array, int index) 
		{
			throw new NotSupportedException ();
		}

		public IEnumerator GetEnumerator () 
		{
			return references.GetEnumerator ();
		}

		public XmlElement GetXml() 
		{
			if (signatureMethod == null)
				throw new CryptographicException ("SignatureMethod");
			if (references.Count == 0)
				throw new CryptographicException ("References empty");

			XmlDocument document = new XmlDocument ();
			XmlElement xel = document.CreateElement (XmlSignature.ElementNames.SignedInfo, XmlSignature.NamespaceURI);
			if (id != null)
				xel.SetAttribute (XmlSignature.AttributeNames.Id, id);

			if (c14nMethod != null) {
				XmlElement c14n = document.CreateElement (XmlSignature.ElementNames.CanonicalizationMethod, XmlSignature.NamespaceURI);
				c14n.SetAttribute (XmlSignature.AttributeNames.Algorithm, c14nMethod);
				xel.AppendChild (c14n);
			}
			if (signatureMethod != null) {
				XmlElement sm = document.CreateElement (XmlSignature.ElementNames.SignatureMethod, XmlSignature.NamespaceURI);
				sm.SetAttribute (XmlSignature.AttributeNames.Algorithm, signatureMethod);
				if (signatureLength != null) {
					XmlElement hmac = document.CreateElement (XmlSignature.ElementNames.HMACOutputLength, XmlSignature.NamespaceURI);
					hmac.InnerText = signatureLength;
					sm.AppendChild (hmac);
				}
				xel.AppendChild (sm);
			}

			// we add References afterward so we don't end up with extraneous
			// xmlns="..." in each reference elements.
			foreach (Reference r in references) {
				XmlNode xn = r.GetXml ();
				XmlNode newNode = document.ImportNode (xn, true);
				xel.AppendChild (newNode);
			}

			return xel;
		}

		private string GetAttributeFromElement (XmlElement xel, string attribute, string element) 
		{
			string result = null;
			XmlNodeList xnl = xel.GetElementsByTagName (element);
			if ((xnl != null) && (xnl.Count > 0)) {
				XmlAttribute xa = xnl[0].Attributes [attribute];
				if (xa != null)
					result = xa.InnerText;
			}
			return result;
		}

		private string GetAttribute (XmlElement xel, string attribute) 
		{
			XmlAttribute xa = xel.Attributes [attribute];
			return ((xa != null) ? xa.InnerText : null);
		}

		[MonoTODO("signatureLength for HMAC")]
		public void LoadXml (XmlElement value) 
		{
			if (value == null)
				throw new ArgumentNullException ("value");

			if ((value.LocalName != XmlSignature.ElementNames.SignedInfo) || (value.NamespaceURI != XmlSignature.NamespaceURI))
				throw new CryptographicException ();

			id = GetAttribute (value, XmlSignature.AttributeNames.Id);
			c14nMethod = GetAttributeFromElement (value, XmlSignature.AttributeNames.Algorithm, XmlSignature.ElementNames.CanonicalizationMethod);
			signatureMethod = GetAttributeFromElement (value, XmlSignature.AttributeNames.Algorithm, XmlSignature.ElementNames.SignatureMethod);
			// TODO signatureLength for HMAC
			XmlNodeList xnl = value.GetElementsByTagName (XmlSignature.ElementNames.Reference);
			foreach (XmlNode xn in xnl) {
				Reference r = new Reference ();
				r.LoadXml ((XmlElement) xn);
				AddReference (r);
			}
		}
	}
}
