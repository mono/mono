//
// SignedXml.cs - SignedXml implementation for XML Signature
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2002, 2003 Motus Technologies Inc. (http://www.motus.com)
//

using System.Collections;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Xml;

namespace System.Security.Cryptography.Xml {

	public class SignedXml {

		private Signature signature;
		private AsymmetricAlgorithm key;
		private string keyName;
		private XmlDocument envdoc;

		public SignedXml () 
		{
			signature = new Signature ();
			signature.SignedInfo = new SignedInfo ();
		}

		public SignedXml (XmlDocument document)
		{
			signature = new Signature ();
			signature.SignedInfo = new SignedInfo ();
			envdoc = document;
		}

		public SignedXml (XmlElement elem) : this ()
		{
			if (elem == null)
				throw new ArgumentNullException ("elem");
			signature = new Signature ();
			signature.SignedInfo = new SignedInfo ();
		}

		public const string XmlDsigCanonicalizationUrl = "http://www.w3.org/TR/2001/REC-xml-c14n-20010315";
		public const string XmlDsigCanonicalizationWithCommentsUrl = XmlDsigCanonicalizationUrl + "#WithComments";
		public const string XmlDsigNamespaceUrl = "http://www.w3.org/2000/09/xmldsig#";
		public const string XmlDsigDSAUrl = XmlDsigNamespaceUrl + "dsa-sha1";
		public const string XmlDsigHMACSHA1Url = XmlDsigNamespaceUrl + "hmac-sha1";
		public const string XmlDsigMinimalCanonicalizationUrl = XmlDsigNamespaceUrl + "minimal";
		public const string XmlDsigRSASHA1Url = XmlDsigNamespaceUrl + "rsa-sha1";
		public const string XmlDsigSHA1Url = XmlDsigNamespaceUrl + "sha1";

		public KeyInfo KeyInfo {
			get { return signature.KeyInfo; }
			set { signature.KeyInfo = value; }
		}

		public Signature Signature {
			get { return signature; }
		}

		public string SignatureLength {
			get { return signature.SignedInfo.SignatureLength; }
		}

		public string SignatureMethod {
			get { return signature.SignedInfo.SignatureMethod; }
		}

		public byte[] SignatureValue {
			get { return signature.SignatureValue; }
		}

		public SignedInfo SignedInfo {
			get { return signature.SignedInfo; }
		}

		public AsymmetricAlgorithm SigningKey {
			get { return key; }
			set { key = value; }
		}

		public string SigningKeyName {
			get { return keyName; }
			set { keyName = value; }
		}

		public void AddObject (DataObject dataObject) 
		{
			signature.AddObject (dataObject);
		}

		public void AddReference (Reference reference) 
		{
			signature.SignedInfo.AddReference (reference);
		}

		private Stream ApplyTransform (Transform t, XmlDocument doc) 
		{
			t.LoadInput (doc);
			if (t is XmlDsigEnvelopedSignatureTransform) {
				XmlDocument d = (XmlDocument) t.GetOutput ();
				MemoryStream ms = new MemoryStream ();
				d.Save (ms);
				return ms;
			}
			else
				return (Stream) t.GetOutput ();
		}

		private Stream ApplyTransform (Transform t, Stream s) 
		{
			try {
				t.LoadInput (s);
				s = (Stream) t.GetOutput ();
			}
			catch (Exception e) {
				string temp = e.ToString (); // stop debugger
			}
			return s;
		}

		[MonoTODO("incomplete")]
		private byte[] GetReferenceHash (Reference r) 
		{
			XmlDocument doc = new XmlDocument ();
			doc.PreserveWhitespace = true;
			if (r.Uri == "")
				doc = envdoc;
			else {
				foreach (DataObject obj in signature.ObjectList) {
					if ("#" + obj.Id == r.Uri) {
						doc.LoadXml (obj.GetXml ().OuterXml);
						break;
					}
				}
			}

			Stream s = null;
			if (r.TransformChain.Count > 0) {		
				foreach (Transform t in r.TransformChain) {
					if (s == null)
						s = ApplyTransform (t, doc);
					else
						s = ApplyTransform (t, s);
				}
			}
			else
				s = ApplyTransform (new XmlDsigC14NTransform (), doc);

			// TODO: We should reuse the same hash object (when possible)
			HashAlgorithm hash = (HashAlgorithm) CryptoConfig.CreateFromName (r.DigestMethod);
			return hash.ComputeHash (s);
		}

		private void DigestReferences () 
		{
			// we must tell each reference which hash algorithm to use 
			// before asking for the SignedInfo XML !
			foreach (Reference r in signature.SignedInfo.References) {
				// assume SHA-1 if nothing is specified
				if (r.DigestMethod == null)
					r.DigestMethod = XmlDsigSHA1Url;
				r.DigestValue = GetReferenceHash (r);
			}
		}
		
		private Stream SignedInfoTransformed () 
		{
			Transform t = (Transform) CryptoConfig.CreateFromName (signature.SignedInfo.CanonicalizationMethod);
			if (t == null)
				return null;

			XmlDocument doc = new XmlDocument ();
			doc.LoadXml (signature.SignedInfo.GetXml ().OuterXml);
			return ApplyTransform (t, doc); 
		}

		private byte[] Hash (string hashAlgorithm) 
		{
			HashAlgorithm hash = HashAlgorithm.Create (hashAlgorithm);
			// get the hash of the C14N SignedInfo element
			return hash.ComputeHash (SignedInfoTransformed ());
		}

		public bool CheckSignature () 
		{
			// CryptographicException
			if (key == null)
				key = GetPublicKey ();
			return CheckSignature (key);
		}

		private bool CheckReferenceIntegrity () 
		{
			// check digest (hash) for every reference
			foreach (Reference r in signature.SignedInfo.References) {
				// stop at first broken reference
				if (! Compare (r.DigestValue, GetReferenceHash (r)))
					return false;
			}
			return true;
		}

		public bool CheckSignature (AsymmetricAlgorithm key) 
		{
			if (key == null)
				throw new ArgumentNullException ("key");

			// Part 1: Are all references digest valid ?
			bool result = CheckReferenceIntegrity ();
			if (result) {
				// Part 2: Is the signature (over SignedInfo) valid ?
				SignatureDescription sd = (SignatureDescription) CryptoConfig.CreateFromName (signature.SignedInfo.SignatureMethod);

				byte[] hash = Hash (sd.DigestAlgorithm);
				AsymmetricSignatureDeformatter verifier = (AsymmetricSignatureDeformatter) CryptoConfig.CreateFromName (sd.DeformatterAlgorithm);

				if (verifier != null) {
					verifier.SetHashAlgorithm (sd.DigestAlgorithm);
					result = verifier.VerifySignature (hash, signature.SignatureValue); 
				}
				else
					result = false;
			}

			return result;
		}

		private bool Compare (byte[] expected, byte[] actual) 
		{
			bool result = ((expected != null) && (actual != null));
			if (result) {
				int l = expected.Length;
				result = (l == actual.Length);
				if (result) {
					for (int i=0; i < l; i++) {
						if (expected[i] != actual[i])
							return false;
					}
				}
			}
			return result;
		}

		public bool CheckSignature (KeyedHashAlgorithm macAlg) 
		{
			if (macAlg == null)
				throw new ArgumentNullException ("macAlg");

			// Part 1: Are all references digest valid ?
			bool result = CheckReferenceIntegrity ();
			if (result) {
				// Part 2: Is the signature (over SignedInfo) valid ?
				byte[] actual = macAlg.ComputeHash (SignedInfoTransformed ());
				result = Compare (signature.SignatureValue, actual);
			}
			return result;
		}

		public bool CheckSignatureReturningKey (out AsymmetricAlgorithm signingKey) 
		{
			// here's the key used for verifying the signature
			if (key == null)
				key = GetPublicKey ();
			signingKey = key;
			// we'll find the key if we haven't already
			return CheckSignature (key);
		}

		public void ComputeSignature () 
		{
			if (key != null) {
				// required before hashing
				signature.SignedInfo.SignatureMethod = key.SignatureAlgorithm;
				DigestReferences ();

				SignatureDescription sd = (SignatureDescription) CryptoConfig.CreateFromName (signature.SignedInfo.SignatureMethod);

				// the hard part - C14Ning the KeyInfo
				byte[] hash = Hash (sd.DigestAlgorithm);
				AsymmetricSignatureFormatter signer = null;

				// in need for a CryptoConfig factory
				if (key is DSA)
					signer = new DSASignatureFormatter (key);
				else if (key is RSA) 
					signer = new RSAPKCS1SignatureFormatter (key);

				if (signer != null) {
					signer.SetHashAlgorithm ("SHA1");
					signature.SignatureValue = signer.CreateSignature (hash);
				}
			}
		}

		public void ComputeSignature (KeyedHashAlgorithm macAlg) 
		{
			if (macAlg == null)
				throw new ArgumentNullException ("macAlg");

			if (macAlg is HMACSHA1) {
				DigestReferences ();

				signature.SignedInfo.SignatureMethod = XmlDsigHMACSHA1Url;
				signature.SignatureValue = macAlg.ComputeHash (SignedInfoTransformed ());
			}
			else 
				throw new CryptographicException ("unsupported algorithm");
		}

		// is that all ?
		public virtual XmlElement GetIdElement (XmlDocument document, string idValue) 
		{
			return document.GetElementById (idValue);
		}

		protected virtual AsymmetricAlgorithm GetPublicKey () 
		{
			AsymmetricAlgorithm key = null;
			if (signature.KeyInfo != null) {
				foreach (KeyInfoClause kic in signature.KeyInfo) {
					if (kic is DSAKeyValue)
						key = DSA.Create ();
					else if (kic is RSAKeyValue) 
						key = RSA.Create ();

					if (key != null) {
						key.FromXmlString (kic.GetXml ().InnerXml);
						break;
					}
				}
			}
			return key;
		}

		public XmlElement GetXml () 
		{
			return signature.GetXml ();
		}

		public void LoadXml (XmlElement value) 
		{
			signature.LoadXml (value);
		}

#if ! NET_1_0
		private XmlResolver xmlResolver;

		[MonoTODO("property not (yet) used in class")]
		[ComVisible(false)]
		XmlResolver Resolver {
			set { xmlResolver = value; }
		}
#endif
	}
}
