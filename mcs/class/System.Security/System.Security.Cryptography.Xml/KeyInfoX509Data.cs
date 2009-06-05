//
// KeyInfoX509Data.cs - KeyInfoX509Data implementation for XML Signature
//
// Authors:
//	Sebastien Pouliot  <sebastien@ximian.com>
//	Atsushi Enomoto (atsushi@ximian.com)
//      Tim Coleman (tim@timcoleman.com)
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
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Xml;

namespace System.Security.Cryptography.Xml {

	public class KeyInfoX509Data : KeyInfoClause {

		private byte[] x509crl;
		private ArrayList IssuerSerialList;
		private ArrayList SubjectKeyIdList;
		private ArrayList SubjectNameList;
		private ArrayList X509CertificateList;

		public KeyInfoX509Data () 
		{
		}

		public KeyInfoX509Data (byte[] rgbCert)
		{
			AddCertificate (new X509Certificate (rgbCert));
		}

		public KeyInfoX509Data (X509Certificate cert)
		{
			AddCertificate (cert);
		}

#if NET_2_0 && SECURITY_DEP
		public KeyInfoX509Data (X509Certificate cert, X509IncludeOption includeOption)
		{
			if (cert == null)
				throw new ArgumentNullException ("cert");

			switch (includeOption) {
			case X509IncludeOption.None:
			case X509IncludeOption.EndCertOnly:
				AddCertificate (cert);
				break;
			case X509IncludeOption.ExcludeRoot:
				AddCertificatesChainFrom (cert, false);
				break;
			case X509IncludeOption.WholeChain:
				AddCertificatesChainFrom (cert, true);
				break;
			}
		}

		// this gets complicated because we must:
		// 1. build the chain using a X509Certificate2 class;
		// 2. test for root using the Mono.Security.X509.X509Certificate class;
		// 3. add the certificates as X509Certificate instances;
		private void AddCertificatesChainFrom (X509Certificate cert, bool root)
		{
			X509Chain chain = new X509Chain ();
			chain.Build (new X509Certificate2 (cert));
			foreach (X509ChainElement ce in chain.ChainElements) {
				byte[] rawdata = ce.Certificate.RawData;
				if (!root) {
					// exclude root
					Mono.Security.X509.X509Certificate mx = new Mono.Security.X509.X509Certificate (rawdata);
					if (mx.IsSelfSigned)
						rawdata = null;
				}

				if (rawdata != null)
					AddCertificate (new X509Certificate (rawdata));
			}
		}
#endif

		public ArrayList Certificates {
			get { return X509CertificateList; }
		}

		public byte[] CRL {
			get { return x509crl; }
			set { x509crl = value; }
		}

		public ArrayList IssuerSerials {
			get { return IssuerSerialList; }
		}

		public ArrayList SubjectKeyIds {
			get { return SubjectKeyIdList; }
		}

		public ArrayList SubjectNames {
			get { return SubjectNameList; }
		}

		public void AddCertificate (X509Certificate certificate) 
		{
#if NET_2_0
			if (certificate == null)
				throw new ArgumentNullException ("certificate");
#endif
			if (X509CertificateList == null)
				X509CertificateList = new ArrayList ();
			X509CertificateList.Add (certificate);
		}

		public void AddIssuerSerial (string issuerName, string serialNumber) 
		{
#if NET_2_0
			if (issuerName == null)
				throw new ArgumentException ("issuerName");
#endif
			if (IssuerSerialList == null)
				IssuerSerialList = new ArrayList ();

			X509IssuerSerial xis = new X509IssuerSerial (issuerName, serialNumber);
			IssuerSerialList.Add (xis);
		}

		public void AddSubjectKeyId (byte[] subjectKeyId) 
		{
			if (SubjectKeyIdList == null)
				SubjectKeyIdList = new ArrayList ();

			SubjectKeyIdList.Add (subjectKeyId);
		}

#if NET_2_0
		[ComVisible (false)]
		public void AddSubjectKeyId (string subjectKeyId)
		{
			if (SubjectKeyIdList == null)
				SubjectKeyIdList = new ArrayList ();

			byte[] id = null;
			if (subjectKeyId != null)
				id = Convert.FromBase64String (subjectKeyId);
			SubjectKeyIdList.Add (id);
		}
#endif

		public void AddSubjectName (string subjectName) 
		{
			if (SubjectNameList == null)
				SubjectNameList = new ArrayList ();

			SubjectNameList.Add (subjectName);
		}

		public override XmlElement GetXml () 
		{
#if !NET_2_0
			// sanity check
			int count = 0;
			if (IssuerSerialList != null)
				count += IssuerSerialList.Count;
			if (SubjectKeyIdList != null)
				count += SubjectKeyIdList.Count;
			if (SubjectNameList != null)
				count += SubjectNameList.Count;
			if (X509CertificateList != null)
				count += X509CertificateList.Count;
			if ((x509crl == null) && (count == 0))
				throw new CryptographicException ("value");
#endif
			XmlDocument document = new XmlDocument ();
			XmlElement xel = document.CreateElement (XmlSignature.ElementNames.X509Data, XmlSignature.NamespaceURI);
			// FIXME: hack to match MS implementation
			xel.SetAttribute ("xmlns", XmlSignature.NamespaceURI);
			// <X509IssuerSerial>
			if ((IssuerSerialList != null) && (IssuerSerialList.Count > 0)) {
				foreach (X509IssuerSerial iser in IssuerSerialList) {
					XmlElement isl = document.CreateElement (XmlSignature.ElementNames.X509IssuerSerial, XmlSignature.NamespaceURI);
					XmlElement xin = document.CreateElement (XmlSignature.ElementNames.X509IssuerName, XmlSignature.NamespaceURI);
					xin.InnerText = iser.IssuerName;
					isl.AppendChild (xin);
 					XmlElement xsn = document.CreateElement (XmlSignature.ElementNames.X509SerialNumber, XmlSignature.NamespaceURI);
					xsn.InnerText = iser.SerialNumber;
					isl.AppendChild (xsn);
					xel.AppendChild (isl);
				}
			}
			// <X509SKI>
			if ((SubjectKeyIdList != null) && (SubjectKeyIdList.Count > 0)) {
				foreach (byte[] skid in SubjectKeyIdList) {
					XmlElement ski = document.CreateElement (XmlSignature.ElementNames.X509SKI, XmlSignature.NamespaceURI);
					ski.InnerText = Convert.ToBase64String (skid);
					xel.AppendChild (ski);
				}
			}
			// <X509SubjectName>
			if ((SubjectNameList != null) && (SubjectNameList.Count > 0)) {
				foreach (string subject in SubjectNameList) {
					XmlElement sn = document.CreateElement (XmlSignature.ElementNames.X509SubjectName, XmlSignature.NamespaceURI);
					sn.InnerText = subject;
					xel.AppendChild (sn);
				}
			}
			// <X509Certificate>
			if ((X509CertificateList != null) && (X509CertificateList.Count > 0)) {
				foreach (X509Certificate x509 in X509CertificateList) {
					XmlElement cert = document.CreateElement (XmlSignature.ElementNames.X509Certificate, XmlSignature.NamespaceURI);
					cert.InnerText = Convert.ToBase64String (x509.GetRawCertData ());
					xel.AppendChild (cert);
				}
			}
			// only one <X509CRL> 
			if (x509crl != null) {
				XmlElement crl = document.CreateElement (XmlSignature.ElementNames.X509CRL, XmlSignature.NamespaceURI);
				crl.InnerText = Convert.ToBase64String (x509crl);
				xel.AppendChild (crl);
			}
			return xel;
		}

		public override void LoadXml (XmlElement element) 
		{
			if (element == null)
				throw new ArgumentNullException ("element");

			if (IssuerSerialList != null)
				IssuerSerialList.Clear ();
			if (SubjectKeyIdList != null)
				SubjectKeyIdList.Clear ();
			if (SubjectNameList != null)
				SubjectNameList.Clear ();
			if (X509CertificateList != null)
				X509CertificateList.Clear ();
			x509crl = null;

			if ((element.LocalName != XmlSignature.ElementNames.X509Data) || (element.NamespaceURI != XmlSignature.NamespaceURI))
				throw new CryptographicException ("element");

			XmlElement [] xnl = null;
			// <X509IssuerSerial>
			xnl = XmlSignature.GetChildElements (element, XmlSignature.ElementNames.X509IssuerSerial);
			if (xnl != null) {
				for (int i=0; i < xnl.Length; i++) {
					XmlElement xel = (XmlElement) xnl[i];
					XmlElement issuer = XmlSignature.GetChildElement (xel, XmlSignature.ElementNames.X509IssuerName, XmlSignature.NamespaceURI);
					XmlElement serial = XmlSignature.GetChildElement (xel, XmlSignature.ElementNames.X509SerialNumber, XmlSignature.NamespaceURI);
					AddIssuerSerial (issuer.InnerText, serial.InnerText);
				}
			}
			// <X509SKI>
			xnl = XmlSignature.GetChildElements (element, XmlSignature.ElementNames.X509SKI);
			if (xnl != null) {
				for (int i=0; i < xnl.Length; i++) {
					byte[] skid = Convert.FromBase64String (xnl[i].InnerXml);
					AddSubjectKeyId (skid);
				}
			}
			// <X509SubjectName>
			xnl = XmlSignature.GetChildElements (element, XmlSignature.ElementNames.X509SubjectName);
			if (xnl != null) {
				for (int i=0; i < xnl.Length; i++) {
					AddSubjectName (xnl[i].InnerXml);
				}
			}
			// <X509Certificate>
			xnl = XmlSignature.GetChildElements (element, XmlSignature.ElementNames.X509Certificate);
			if (xnl != null) {
				for (int i=0; i < xnl.Length; i++) {
					byte[] cert = Convert.FromBase64String (xnl[i].InnerXml);
					AddCertificate (new X509Certificate (cert));
				}
			}
			// only one <X509CRL> 
			XmlElement x509el = XmlSignature.GetChildElement (element, XmlSignature.ElementNames.X509CRL, XmlSignature.NamespaceURI);
			if (x509el != null) {
				x509crl = Convert.FromBase64String (x509el.InnerXml);
			}
		}
	}
}
