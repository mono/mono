//
// SignedXml.cs - SignedXml implementation for XML Signature
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//	Atsushi Enomoto <atsushi@ximian.com>
//
// (C) 2002, 2003 Motus Technologies Inc. (http://www.motus.com)
// (C) 2004 Novell (http://www.novell.com)
//

using System.Collections;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Net;
using System.Text;
using System.Xml;

namespace System.Security.Cryptography.Xml {

	public class SignedXml {

		private Signature signature;
		private AsymmetricAlgorithm key;
		private string keyName;
		private XmlDocument envdoc;
		private IEnumerator pkEnumerator;
		private XmlElement signatureElement;
		private Hashtable hashes;

		public SignedXml () 
		{
			signature = new Signature ();
			signature.SignedInfo = new SignedInfo ();
			hashes = new Hashtable (2); // 98% SHA1 for now
		}

		public SignedXml (XmlDocument document) : this ()
		{
			if (document == null)
				throw new ArgumentNullException ("document");
			envdoc = document;
		}

		public SignedXml (XmlElement elem) : this ()
		{
			if (elem == null)
				throw new ArgumentNullException ("elem");
			envdoc = new XmlDocument ();
			envdoc.LoadXml (elem.OuterXml);
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

		// NOTE: CryptoAPI related ? documented as fx internal
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

		private Stream ApplyTransform (Transform t, XmlDocument input) 
		{
			XmlDocument doc = (XmlDocument) input.Clone ();

			t.LoadInput (doc);
			if (t is XmlDsigEnvelopedSignatureTransform) {
				// It returns XmlDocument for XmlDocument input.
				doc = (XmlDocument) t.GetOutput ();
				Transform c14n = GetC14NMethod ();
				c14n.LoadInput (doc);
				return (Stream) c14n.GetOutput ();
			}

			object obj = t.GetOutput ();
			if (obj is Stream)
				return (Stream) obj;
			else {
				// e.g. XmlDsigXPathTransform returns XmlNodeList
				// TODO - fix
				return null;
			}
		}

		[MonoTODO("incomplete")]
		private byte[] GetReferenceHash (Reference r) 
		{
			Stream s = null;
			XmlDocument doc = null;
			if (r.Uri == String.Empty) {
				doc = envdoc;
			}
			else {
				doc = new XmlDocument ();
				doc.PreserveWhitespace = true;

				if (r.Uri [0] == '#') {
					foreach (DataObject obj in signature.ObjectList) {
						if ("#" + obj.Id == r.Uri) {
							doc.LoadXml (obj.GetXml ().OuterXml);
							break;
						}
					}
				}
				else {
					if (r.Uri.EndsWith (".xml")) {
#if ! NET_1_0
						doc.XmlResolver = xmlResolver;
#endif						
						doc.Load (r.Uri);
					}
					else {
						WebRequest req = WebRequest.Create (r.Uri);
						s = req.GetResponse ().GetResponseStream ();
					}
				}
			}

			if (r.TransformChain.Count > 0) {		
				foreach (Transform t in r.TransformChain) {
					if (s == null) {
						s = ApplyTransform (t, doc);
					}
					else {
						t.LoadInput (s);
						s = (Stream) t.GetOutput ();
					}
				}
			}
			else if (s == null) {
				// apply default C14N transformation
				s = ApplyTransform (new XmlDsigC14NTransform (), doc);
			}

			HashAlgorithm hash = GetHash (r.DigestMethod);
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

		private Transform GetC14NMethod ()
		{
			Transform t = (Transform) CryptoConfig.CreateFromName (signature.SignedInfo.CanonicalizationMethod);
			if (t == null)
				throw new CryptographicException ("Unknown Canonicalization Method {0}", signature.SignedInfo.CanonicalizationMethod);
			return t;
		}

		private Stream SignedInfoTransformed () 
		{
			Transform t = GetC14NMethod ();

			if (signatureElement == null) {
				// when creating signatures
				XmlDocument doc = new XmlDocument ();
				doc.PreserveWhitespace = true;
				doc.LoadXml (signature.SignedInfo.GetXml ().OuterXml);

				t.LoadInput (doc);
			}
			else {
				// when verifying signatures
				// TODO - check signature.SignedInfo.Id
				XmlNodeList xnl = signatureElement.GetElementsByTagName (XmlSignature.ElementNames.SignedInfo, XmlSignature.NamespaceURI);
				byte[] si = Encoding.UTF8.GetBytes (xnl [0].OuterXml);

				MemoryStream ms = new MemoryStream ();
				ms.Write (si, 0, si.Length);
				ms.Position = 0;

				t.LoadInput (ms);
			}
			// C14N and C14NWithComments always return a Stream in GetOutput
			return (Stream) t.GetOutput ();
		}

		// reuse hash - most document will always use the same hash
		private HashAlgorithm GetHash (string algorithm) 
		{
			HashAlgorithm hash = (HashAlgorithm) hashes [algorithm];
			if (hash == null) {
				hash = HashAlgorithm.Create (algorithm);
				if (hash == null)
					throw new CryptographicException ("Unknown hash algorithm: {0}", algorithm);
				hashes.Add (algorithm, hash);
				// now ready to be used
			}
			else {
				// important before reusing an hash object
				hash.Initialize ();
			}
			return hash;
		}

/*		private byte[] ComputeHash (string hashAlgorithm) 
		{
			HashAlgorithm hash = GetHash (hashAlgorithm);
			// get the hash of the C14N SignedInfo element
			return hash.ComputeHash (SignedInfoTransformed ());
		}
*/
		public bool CheckSignature () 
		{
			return (CheckSignatureInternal (null) != null);
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
			return (CheckSignatureInternal (key) != null);
		}

		private AsymmetricAlgorithm CheckSignatureInternal (AsymmetricAlgorithm key)
		{
			pkEnumerator = null;

			if (key != null) {
				// check with supplied key
				if (!CheckSignatureWithKey (key))
					return null;
			}
			else {
				// no supplied key, iterates all KeyInfo
				while ((key = GetPublicKey ()) != null) {
					if (CheckSignatureWithKey (key)) {
						break;
					}
				}
				if (key == null)
					throw new CryptographicException ("No public key found to verify the signature.");
			}

			// some parts may need to be downloaded
			// so where doing it last
			return (CheckReferenceIntegrity () ? key : null);
		}

		// Is the signature (over SignedInfo) valid ?
		private bool CheckSignatureWithKey (AsymmetricAlgorithm key) 
		{
			if (key == null)
				return false;

			SignatureDescription sd = (SignatureDescription) CryptoConfig.CreateFromName (signature.SignedInfo.SignatureMethod);
			if (sd == null)
				return false;

			AsymmetricSignatureDeformatter verifier = (AsymmetricSignatureDeformatter) CryptoConfig.CreateFromName (sd.DeformatterAlgorithm);
			if (verifier == null)
				return false;

			verifier.SetKey (key);
			verifier.SetHashAlgorithm (sd.DigestAlgorithm);

			HashAlgorithm hash = GetHash (sd.DigestAlgorithm);
			// get the hash of the C14N SignedInfo element
			byte[] digest = hash.ComputeHash (SignedInfoTransformed ());
			return verifier.VerifySignature (digest, signature.SignatureValue); 
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

			pkEnumerator = null;

			// Is the signature (over SignedInfo) valid ?
			Stream s = SignedInfoTransformed ();
			if (s == null)
				return false;

			byte[] actual = macAlg.ComputeHash (s);
			// HMAC signature may be partial
			if (signature.SignedInfo.SignatureLength != null) {
				int length = actual.Length;
				try {
					// SignatureLength is in bits
					length = (Int32.Parse (signature.SignedInfo.SignatureLength) >> 3);
				}
				catch {
				}

				if (length != actual.Length) {
					byte[] trunked = new byte [length];
					Buffer.BlockCopy (actual, 0, trunked, 0, length);
					actual = trunked;
				}
			}

			if (Compare (signature.SignatureValue, actual)) {
				// some parts may need to be downloaded
				// so where doing it last
				return CheckReferenceIntegrity ();
			}
			return false;
		}

		public bool CheckSignatureReturningKey (out AsymmetricAlgorithm signingKey) 
		{
			signingKey = CheckSignatureInternal (null);
			return (signingKey != null);
		}

		public void ComputeSignature () 
		{
			if (key != null) {
				// required before hashing
				signature.SignedInfo.SignatureMethod = key.SignatureAlgorithm;
				DigestReferences ();

				AsymmetricSignatureFormatter signer = null;
				// in need for a CryptoConfig factory
				if (key is DSA)
					signer = new DSASignatureFormatter (key);
				else if (key is RSA) 
					signer = new RSAPKCS1SignatureFormatter (key);

				if (signer != null) {
					SignatureDescription sd = (SignatureDescription) CryptoConfig.CreateFromName (signature.SignedInfo.SignatureMethod);

					HashAlgorithm hash = GetHash (sd.DigestAlgorithm);
					// get the hash of the C14N SignedInfo element
					byte[] digest = hash.ComputeHash (SignedInfoTransformed ());

					signer.SetHashAlgorithm ("SHA1");
					signature.SignatureValue = signer.CreateSignature (digest);
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

		public virtual XmlElement GetIdElement (XmlDocument document, string idValue) 
		{
			// this works only if there's a DTD or XSD available to define the ID
			XmlElement xel = document.GetElementById (idValue);
			if (xel == null) {
				// search an "undefined" ID
				xel = (XmlElement) document.SelectSingleNode ("//*[@Id='" + idValue + "']");
			}
			return xel;
		}

		// According to book ".NET Framework Security" this method
		// iterates all possible keys then return null
		protected virtual AsymmetricAlgorithm GetPublicKey () 
		{
			if (signature.KeyInfo == null)
				return null;

			if (pkEnumerator == null) {
				pkEnumerator = signature.KeyInfo.GetEnumerator ();
			}

			if (pkEnumerator.MoveNext ()) {
				AsymmetricAlgorithm key = null;
				KeyInfoClause kic = (KeyInfoClause) pkEnumerator.Current;

				if (kic is DSAKeyValue)
					key = DSA.Create ();
				else if (kic is RSAKeyValue) 
					key = RSA.Create ();

				if (key != null) {
					key.FromXmlString (kic.GetXml ().InnerXml);
					return key;
				}
			}
			return null;
		}

		public XmlElement GetXml () 
		{
			return signature.GetXml ();
		}

		public void LoadXml (XmlElement value) 
		{
			if (value == null)
				throw new ArgumentNullException ("value");

			signatureElement = value;
			signature.LoadXml (value);
		}

#if ! NET_1_0
		private XmlResolver xmlResolver;

		[MonoTODO("property not (yet) used in class")]
		[ComVisible(false)]
		public XmlResolver Resolver {
			set { xmlResolver = value; }
		}
#endif
	}
}
