//
// SignedXml.cs - SignedXml implementation for XML Signature
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//	Atsushi Enomoto <atsushi@ximian.com>
//      Tim Coleman <tim@timcoleman.com>
//
// (C) 2002, 2003 Motus Technologies Inc. (http://www.motus.com)
// Copyright (C) Tim Coleman, 2004
// Copyright (C) 2004-2005 Novell, Inc (http://www.novell.com)
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

using System.Collections;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Security.Policy;
using System.Net;
using System.Text;
using System.Xml;

#if NET_2_0
using System.Security.Cryptography.X509Certificates;
#endif

namespace System.Security.Cryptography.Xml {

	public class SignedXml {

		public const string XmlDsigCanonicalizationUrl			= "http://www.w3.org/TR/2001/REC-xml-c14n-20010315";
		public const string XmlDsigCanonicalizationWithCommentsUrl	= XmlDsigCanonicalizationUrl + "#WithComments";
		public const string XmlDsigDSAUrl				= XmlDsigNamespaceUrl + "dsa-sha1";
		public const string XmlDsigHMACSHA1Url				= XmlDsigNamespaceUrl + "hmac-sha1";
		public const string XmlDsigMinimalCanonicalizationUrl		= XmlDsigNamespaceUrl + "minimal";
		public const string XmlDsigNamespaceUrl				= "http://www.w3.org/2000/09/xmldsig#";
		public const string XmlDsigRSASHA1Url				= XmlDsigNamespaceUrl + "rsa-sha1";
		public const string XmlDsigSHA1Url				= XmlDsigNamespaceUrl + "sha1";

#if NET_2_0
		public const string XmlDecryptionTransformUrl			= "http://www.w3.org/2002/07/decrypt#XML";
		public const string XmlDsigBase64TransformUrl			= XmlDsigNamespaceUrl + "base64";
		public const string XmlDsigC14NTransformUrl			= XmlDsigCanonicalizationUrl;
		public const string XmlDsigC14NWithCommentsTransformUrl		= XmlDsigCanonicalizationWithCommentsUrl;
		public const string XmlDsigEnvelopedSignatureTransformUrl	= XmlDsigNamespaceUrl + "enveloped-signature";
		public const string XmlDsigExcC14NTransformUrl			= "http://www.w3.org/2001/10/xml-exc-c14n#";
		public const string XmlDsigExcC14NWithCommentsTransformUrl	= XmlDsigExcC14NTransformUrl + "WithComments";
		public const string XmlDsigXPathTransformUrl			= "http://www.w3.org/TR/1999/REC-xpath-19991116";
		public const string XmlDsigXsltTransformUrl			= "http://www.w3.org/TR/1999/REC-xslt-19991116";
		public const string XmlLicenseTransformUrl			= "urn:mpeg:mpeg21:2003:01-REL-R-NS:licenseTransform";

		private EncryptedXml encryptedXml;
#endif

		protected Signature m_signature;
		private AsymmetricAlgorithm key;
		protected string m_strSigningKeyName;
		private XmlDocument envdoc;
		private IEnumerator pkEnumerator;
		private XmlElement signatureElement;
		private Hashtable hashes;
		// FIXME: enable it after CAS implementation
#if false //NET_1_1
		private XmlResolver xmlResolver = new XmlSecureResolver (new XmlUrlResolver (), new Evidence ());
#else
		private XmlResolver xmlResolver = new XmlUrlResolver ();
#endif
		private ArrayList manifests;
#if NET_2_0
		private IEnumerator _x509Enumerator;
#endif

		private static readonly char [] whitespaceChars = new char [] {' ', '\r', '\n', '\t'};

		public SignedXml () 
		{
			m_signature = new Signature ();
			m_signature.SignedInfo = new SignedInfo ();
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

#if NET_2_0
		[ComVisible (false)]
		public EncryptedXml EncryptedXml {
			get { return encryptedXml; }
			set { encryptedXml = value; }
		}
#endif

		public KeyInfo KeyInfo {
			get {
#if NET_2_0
				if (m_signature.KeyInfo == null)
					m_signature.KeyInfo = new KeyInfo ();
#endif
				return m_signature.KeyInfo;
			}
			set { m_signature.KeyInfo = value; }
		}

		public Signature Signature {
			get { return m_signature; }
		}

		public string SignatureLength {
			get { return m_signature.SignedInfo.SignatureLength; }
		}

		public string SignatureMethod {
			get { return m_signature.SignedInfo.SignatureMethod; }
		}

		public byte[] SignatureValue {
			get { return m_signature.SignatureValue; }
		}

		public SignedInfo SignedInfo {
			get { return m_signature.SignedInfo; }
		}

		public AsymmetricAlgorithm SigningKey {
			get { return key; }
			set { key = value; }
		}

		// NOTE: CryptoAPI related ? documented as fx internal
		public string SigningKeyName {
			get { return m_strSigningKeyName; }
			set { m_strSigningKeyName = value; }
		}

		public void AddObject (DataObject dataObject) 
		{
			m_signature.AddObject (dataObject);
		}

		public void AddReference (Reference reference) 
		{
#if NET_2_0
			if (reference == null)
				throw new ArgumentNullException ("reference");
#endif
			m_signature.SignedInfo.AddReference (reference);
		}

		private Stream ApplyTransform (Transform t, XmlDocument input) 
		{
			// These transformer modify input document, which should
			// not affect to the input itself.
			if (t is XmlDsigXPathTransform 
				|| t is XmlDsigEnvelopedSignatureTransform
#if NET_2_0
				|| t is XmlDecryptionTransform
#endif
			)
				input = (XmlDocument) input.Clone ();

			t.LoadInput (input);

			if (t is XmlDsigEnvelopedSignatureTransform)
				// It returns XmlDocument for XmlDocument input.
				return CanonicalizeOutput (t.GetOutput ());

			object obj = t.GetOutput ();
			if (obj is Stream)
				return (Stream) obj;
			else if (obj is XmlDocument) {
				MemoryStream ms = new MemoryStream ();
				XmlTextWriter xtw = new XmlTextWriter (ms, Encoding.UTF8);
				((XmlDocument) obj).WriteTo (xtw);

				xtw.Flush ();

				// Rewind to the start of the stream
				ms.Position = 0;
				return ms;
			}
			else if (obj == null) {
				throw new NotImplementedException ("This should not occur. Transform is " + t + ".");
			}
			else {
				// e.g. XmlDsigXPathTransform returns XmlNodeList
				return CanonicalizeOutput (obj);
			}
		}

		private Stream CanonicalizeOutput (object obj)
		{
			Transform c14n = GetC14NMethod ();
			c14n.LoadInput (obj);
			return (Stream) c14n.GetOutput ();
		}

		private XmlDocument GetManifest (Reference r) 
		{
			XmlDocument doc = new XmlDocument ();
			doc.PreserveWhitespace = true;

			if (r.Uri [0] == '#') {
				// local manifest
				if (signatureElement != null) {
					XmlElement xel = GetIdElement (signatureElement.OwnerDocument, r.Uri.Substring (1));
					if (xel == null)
						throw new CryptographicException ("Manifest targeted by Reference was not found: " + r.Uri.Substring (1));
					doc.AppendChild (doc.ImportNode (xel, true));
					FixupNamespaceNodes (xel, doc.DocumentElement, false);
				}
			}
			else if (xmlResolver != null) {
				// TODO: need testing
				Stream s = (Stream) xmlResolver.GetEntity (new Uri (r.Uri), null, typeof (Stream));
				doc.Load (s);
			}

			if (doc.FirstChild != null) {
				// keep a copy of the manifests to check their references later
				if (manifests == null)
					manifests = new ArrayList ();
				manifests.Add (doc);

				return doc;
			}
			return null;
		}

		private void FixupNamespaceNodes (XmlElement src, XmlElement dst, bool ignoreDefault)
		{
			// add namespace nodes
			foreach (XmlAttribute attr in src.SelectNodes ("namespace::*")) {
				if (attr.LocalName == "xml")
					continue;
				if (ignoreDefault && attr.LocalName == "xmlns")
					continue;
				dst.SetAttributeNode (dst.OwnerDocument.ImportNode (attr, true) as XmlAttribute);
			}
		}

		private byte[] GetReferenceHash (Reference r, bool check_hmac) 
		{
			Stream s = null;
			XmlDocument doc = null;
			if (r.Uri == String.Empty) {
				doc = envdoc;
			}
			else if (r.Type == XmlSignature.Uri.Manifest) {
				doc = GetManifest (r);
			}
			else {
				doc = new XmlDocument ();
				doc.PreserveWhitespace = true;
				string objectName = null;

				if (r.Uri.StartsWith ("#xpointer")) {
					string uri = string.Join ("", r.Uri.Substring (9).Split (whitespaceChars));
					if (uri.Length < 2 || uri [0] != '(' || uri [uri.Length - 1] != ')')
						// FIXME: how to handle invalid xpointer?
						uri = String.Empty;
					else
						uri = uri.Substring (1, uri.Length - 2);
					if (uri == "/")
						doc = envdoc;
					else if (uri.Length > 6 && uri.StartsWith ("id(") && uri [uri.Length - 1] == ')')
						// id('foo'), id("foo")
						objectName = uri.Substring (4, uri.Length - 6);
				}
				else if (r.Uri [0] == '#') {
					objectName = r.Uri.Substring (1);
				}
				else if (xmlResolver != null) {
					// TODO: test but doc says that Resolver = null -> no access
					try {
						// no way to know if valid without throwing an exception
						Uri uri = new Uri (r.Uri);
						s = (Stream) xmlResolver.GetEntity (uri, null, typeof (Stream));
					}
					catch {
						// may still be a local file (and maybe not xml)
						s = File.OpenRead (r.Uri);
					}
				}
				if (objectName != null) {
					XmlElement found = null;
					foreach (DataObject obj in m_signature.ObjectList) {
						if (obj.Id == objectName) {
							found = obj.GetXml ();
							found.SetAttribute ("xmlns", SignedXml.XmlDsigNamespaceUrl);
							doc.AppendChild (doc.ImportNode (found, true));
							// FIXME: there should be theoretical justification of copying namespace declaration nodes this way.
							foreach (XmlNode n in found.ChildNodes)
								// Do not copy default namespace as it must be xmldsig namespace for "Object" element.
								if (n.NodeType == XmlNodeType.Element)
									FixupNamespaceNodes (n as XmlElement, doc.DocumentElement, true);
							break;
						}
					}
					if (found == null && envdoc != null) {
						found = GetIdElement (envdoc, objectName);
						if (found != null) {
							doc.AppendChild (doc.ImportNode (found, true));
							FixupNamespaceNodes (found, doc.DocumentElement, false);
						}
					}
					if (found == null)
						throw new CryptographicException (String.Format ("Malformed reference object: {0}", objectName));
				}
			}

			if (r.TransformChain.Count > 0) {		
				foreach (Transform t in r.TransformChain) {
					if (s == null) {
						s = ApplyTransform (t, doc);
					}
					else {
						t.LoadInput (s);
						object o = t.GetOutput ();
						if (o is Stream)
							s = (Stream) o;
						else
							s = CanonicalizeOutput (o);
					}
				}
			}
			else if (s == null) {
				// we must not C14N references from outside the document
				// e.g. non-xml documents
				if (r.Uri [0] != '#') {
					s = new MemoryStream ();
					doc.Save (s);
				}
				else {
					// apply default C14N transformation
					s = ApplyTransform (new XmlDsigC14NTransform (), doc);
				}
			}
			HashAlgorithm digest = GetHash (r.DigestMethod, check_hmac);
			return (digest == null) ? null : digest.ComputeHash (s);
		}

		private void DigestReferences () 
		{
			// we must tell each reference which hash algorithm to use 
			// before asking for the SignedInfo XML !
			foreach (Reference r in m_signature.SignedInfo.References) {
				// assume SHA-1 if nothing is specified
				if (r.DigestMethod == null)
					r.DigestMethod = XmlDsigSHA1Url;
				r.DigestValue = GetReferenceHash (r, false);
			}
		}

		private Transform GetC14NMethod ()
		{
			Transform t = (Transform) CryptoConfig.CreateFromName (m_signature.SignedInfo.CanonicalizationMethod);
			if (t == null)
				throw new CryptographicException ("Unknown Canonicalization Method {0}", m_signature.SignedInfo.CanonicalizationMethod);
			return t;
		}

		private Stream SignedInfoTransformed () 
		{
			Transform t = GetC14NMethod ();

			if (signatureElement == null) {
				// when creating signatures
				XmlDocument doc = new XmlDocument ();
				doc.PreserveWhitespace = true;
				doc.LoadXml (m_signature.SignedInfo.GetXml ().OuterXml);
				if (envdoc != null)
				foreach (XmlAttribute attr in envdoc.DocumentElement.SelectNodes ("namespace::*")) {
					if (attr.LocalName == "xml")
						continue;
					if (attr.Prefix == doc.DocumentElement.Prefix)
						continue;
					doc.DocumentElement.SetAttributeNode (doc.ImportNode (attr, true) as XmlAttribute);
				}
				t.LoadInput (doc);
			}
			else {
				// when verifying signatures
				// TODO - check m_signature.SignedInfo.Id
				XmlElement el = signatureElement.GetElementsByTagName (XmlSignature.ElementNames.SignedInfo, XmlSignature.NamespaceURI) [0] as XmlElement;
				StringWriter sw = new StringWriter ();
				XmlTextWriter xtw = new XmlTextWriter (sw);
				xtw.WriteStartElement (el.Prefix, el.LocalName, el.NamespaceURI);

				// context namespace nodes (except for "xmlns:xml")
				XmlNodeList nl = el.SelectNodes ("namespace::*");
				foreach (XmlAttribute attr in nl) {
					if (attr.ParentNode == el)
						continue;
					if (attr.LocalName == "xml")
						continue;
					if (attr.Prefix == el.Prefix)
						continue;
					attr.WriteTo (xtw);
				}
				foreach (XmlNode attr in el.Attributes)
					attr.WriteTo (xtw);
				foreach (XmlNode n in el.ChildNodes)
					n.WriteTo (xtw);

				xtw.WriteEndElement ();
				byte [] si = Encoding.UTF8.GetBytes (sw.ToString ());

				MemoryStream ms = new MemoryStream ();
				ms.Write (si, 0, si.Length);
				ms.Position = 0;

				t.LoadInput (ms);
			}
			// C14N and C14NWithComments always return a Stream in GetOutput
			return (Stream) t.GetOutput ();
		}

		// reuse hash - most document will always use the same hash
		private HashAlgorithm GetHash (string algorithm, bool check_hmac) 
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
			// we can sign using any hash algorith, including HMAC, but we can only verify hash (MS compatibility)
			if (check_hmac && (hash is KeyedHashAlgorithm))
				return null;
			return hash;
		}

		public bool CheckSignature () 
		{
			return (CheckSignatureInternal (null) != null);
		}

		private bool CheckReferenceIntegrity (ArrayList referenceList) 
		{
			if (referenceList == null)
				return false;

			// check digest (hash) for every reference
			foreach (Reference r in referenceList) {
				// stop at first broken reference
				byte[] hash = GetReferenceHash (r, true);
				if (! Compare (r.DigestValue, hash))
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
			} else {
#if NET_2_0
				if (Signature.KeyInfo == null)
					return null;
#else
				if (Signature.KeyInfo == null)
					throw new CryptographicException ("At least one KeyInfo is required.");
#endif
				// no supplied key, iterates all KeyInfo
				while ((key = GetPublicKey ()) != null) {
					if (CheckSignatureWithKey (key)) {
						break;
					}
				}
				pkEnumerator = null;
				if (key == null)
					return null;
			}

			// some parts may need to be downloaded
			// so where doing it last
			if (!CheckReferenceIntegrity (m_signature.SignedInfo.References))
				return null;

			if (manifests != null) {
				// do not use foreach as a manifest could contain manifests...
				for (int i=0; i < manifests.Count; i++) {
					Manifest manifest = new Manifest ((manifests [i] as XmlDocument).DocumentElement);
					if (! CheckReferenceIntegrity (manifest.References))
						return null;
				}
			}
			return key;
		}

		// Is the signature (over SignedInfo) valid ?
		private bool CheckSignatureWithKey (AsymmetricAlgorithm key) 
		{
			if (key == null)
				return false;

			SignatureDescription sd = (SignatureDescription) CryptoConfig.CreateFromName (m_signature.SignedInfo.SignatureMethod);
			if (sd == null)
				return false;

			AsymmetricSignatureDeformatter verifier = (AsymmetricSignatureDeformatter) CryptoConfig.CreateFromName (sd.DeformatterAlgorithm);
			if (verifier == null)
				return false;

			try {
				verifier.SetKey (key);
				verifier.SetHashAlgorithm (sd.DigestAlgorithm);

				HashAlgorithm hash = GetHash (sd.DigestAlgorithm, true);
				// get the hash of the C14N SignedInfo element
				MemoryStream ms = (MemoryStream) SignedInfoTransformed ();

				byte[] digest = hash.ComputeHash (ms);
				return verifier.VerifySignature (digest, m_signature.SignatureValue);
			}
			catch {
				// e.g. SignatureMethod != AsymmetricAlgorithm type
				return false;
			} 
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
			// HMAC signature may be partial and specified by <HMACOutputLength>
			if (m_signature.SignedInfo.SignatureLength != null) {
				int length = Int32.Parse (m_signature.SignedInfo.SignatureLength);
				// we only support signatures with a multiple of 8 bits
				// and the value must match the signature length
				if ((length & 7) != 0)
					throw new CryptographicException ("Signature length must be a multiple of 8 bits.");

				// SignatureLength is in bits (and we works on bytes, only in multiple of 8 bits)
				// and both values must match for a signature to be valid
				length >>= 3;
				if (length != m_signature.SignatureValue.Length)
					throw new CryptographicException ("Invalid signature length.");

				// is the length "big" enough to make the signature meaningful ? 
				// we use a minimum of 80 bits (10 bytes) or half the HMAC normal output length
				// e.g. HMACMD5 output 128 bits but our minimum is 80 bits (not 64 bits)
				int minimum = Math.Max (10, actual.Length / 2);
				if (length < minimum)
					throw new CryptographicException ("HMAC signature is too small");

				if (length < actual.Length) {
					byte[] trunked = new byte [length];
					Buffer.BlockCopy (actual, 0, trunked, 0, length);
					actual = trunked;
				}
			}

			if (Compare (m_signature.SignatureValue, actual)) {
				// some parts may need to be downloaded
				// so where doing it last
				return CheckReferenceIntegrity (m_signature.SignedInfo.References);
			}
			return false;
		}

#if NET_2_0
		[MonoTODO]
		[ComVisible (false)]
		public bool CheckSignature (X509Certificate2 certificate, bool verifySignatureOnly)
		{
			throw new NotImplementedException ();
		}
#endif

		public bool CheckSignatureReturningKey (out AsymmetricAlgorithm signingKey) 
		{
			signingKey = CheckSignatureInternal (null);
			return (signingKey != null);
		}

		public void ComputeSignature () 
		{
			if (key != null) {
				if (m_signature.SignedInfo.SignatureMethod == null)
					// required before hashing
					m_signature.SignedInfo.SignatureMethod = key.SignatureAlgorithm;
				else if (m_signature.SignedInfo.SignatureMethod != key.SignatureAlgorithm)
					throw new CryptographicException ("Specified SignatureAlgorithm is not supported by the signing key.");
				DigestReferences ();

				AsymmetricSignatureFormatter signer = null;
				// in need for a CryptoConfig factory
				if (key is DSA)
					signer = new DSASignatureFormatter (key);
				else if (key is RSA) 
					signer = new RSAPKCS1SignatureFormatter (key);

				if (signer != null) {
					SignatureDescription sd = (SignatureDescription) CryptoConfig.CreateFromName (m_signature.SignedInfo.SignatureMethod);

					HashAlgorithm hash = GetHash (sd.DigestAlgorithm, false);
					// get the hash of the C14N SignedInfo element
					byte[] digest = hash.ComputeHash (SignedInfoTransformed ());

					signer.SetHashAlgorithm ("SHA1");
					m_signature.SignatureValue = signer.CreateSignature (digest);
				}
			}
			else
				throw new CryptographicException ("signing key is not specified");
		}

		public void ComputeSignature (KeyedHashAlgorithm macAlg) 
		{
			if (macAlg == null)
				throw new ArgumentNullException ("macAlg");

			string method = null;

			if (macAlg is HMACSHA1) {
				method = XmlDsigHMACSHA1Url;
#if NET_2_0
			} else if (macAlg is HMACSHA256) {
				method = "http://www.w3.org/2001/04/xmldsig-more#hmac-sha256";
			} else if (macAlg is HMACSHA384) {
				method = "http://www.w3.org/2001/04/xmldsig-more#hmac-sha384";
			} else if (macAlg is HMACSHA512) {
				method = "http://www.w3.org/2001/04/xmldsig-more#hmac-sha512";
			} else if (macAlg is HMACRIPEMD160) {
				method = "http://www.w3.org/2001/04/xmldsig-more#hmac-ripemd160";
#endif
			}

			if (method == null)
				throw new CryptographicException ("unsupported algorithm");

			DigestReferences ();
			m_signature.SignedInfo.SignatureMethod = method;
			m_signature.SignatureValue = macAlg.ComputeHash (SignedInfoTransformed ());
		}

		public virtual XmlElement GetIdElement (XmlDocument document, string idValue) 
		{
			if ((document == null) || (idValue == null))
				return null;

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
			if (m_signature.KeyInfo == null)
				return null;

			if (pkEnumerator == null) {
				pkEnumerator = m_signature.KeyInfo.GetEnumerator ();
			}
			
#if NET_2_0 && SECURITY_DEP
			if (_x509Enumerator != null) {
				if (_x509Enumerator.MoveNext ()) {
					X509Certificate cert = (X509Certificate) _x509Enumerator.Current;
					return new X509Certificate2 (cert.GetRawCertData ()).PublicKey.Key;
				} else {
					_x509Enumerator = null;
				}
			}
#endif
			while (pkEnumerator.MoveNext ()) {
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

#if NET_2_0 && SECURITY_DEP
				if (kic is KeyInfoX509Data) {
					_x509Enumerator = ((KeyInfoX509Data) kic).Certificates.GetEnumerator ();
					if (_x509Enumerator.MoveNext ()) {
						X509Certificate cert = (X509Certificate) _x509Enumerator.Current;
						return new X509Certificate2 (cert.GetRawCertData ()).PublicKey.Key;
					}
				}
#endif
			}
			return null;
		}

		public XmlElement GetXml () 
		{
			return m_signature.GetXml (envdoc);
		}

		public void LoadXml (XmlElement value) 
		{
			if (value == null)
				throw new ArgumentNullException ("value");

			signatureElement = value;
			m_signature.LoadXml (value);
#if NET_2_0
			// Need to give the EncryptedXml object to the 
			// XmlDecryptionTransform to give it a fighting 
			// chance at decrypting the document.
			foreach (Reference r in m_signature.SignedInfo.References) {
				foreach (Transform t in r.TransformChain) {
					if (t is XmlDecryptionTransform) 
						((XmlDecryptionTransform) t).EncryptedXml = EncryptedXml;
				}
			}
#endif
		}

#if NET_1_1
		[ComVisible (false)]
		public XmlResolver Resolver {
			set { xmlResolver = value; }
		}
#endif
	}
}
