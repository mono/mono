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
// (C) 2004 Novell Inc.
//

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
			IssuerSerialList = new ArrayList ();
			SubjectKeyIdList = new ArrayList ();
			SubjectNameList = new ArrayList ();
			X509CertificateList = new ArrayList ();
		}

		public KeyInfoX509Data (byte[] rgbCert) : this ()
		{
			AddCertificate (new X509Certificate (rgbCert));
		}

		public KeyInfoX509Data (X509Certificate cert) : this ()
		{
			AddCertificate (cert);
		}

#if NET_2_0
		public KeyInfoX509Data (X509Certificate cert, X509IncludeOption includeOption)
		{
			
		}
#endif

		public ArrayList Certificates {
			get { return X509CertificateList.Count != 0 ? X509CertificateList : null; }
		}

		public byte[] CRL {
			get { return x509crl; }
			set { x509crl = value; }
		}

		public ArrayList IssuerSerials {
			get { return IssuerSerialList.Count != 0 ? IssuerSerialList : null; }
		}

		public ArrayList SubjectKeyIds {
			get { return SubjectKeyIdList.Count != 0 ? SubjectKeyIdList : null; }
		}

		public ArrayList SubjectNames {
			get { return SubjectNameList.Count != 0 ? SubjectNameList : null; }
		}

		public void AddCertificate (X509Certificate certificate) 
		{
			X509CertificateList.Add (certificate);
		}

		public void AddIssuerSerial (string issuerName, string serialNumber) 
		{
			X509IssuerSerial xis = new X509IssuerSerial (issuerName, serialNumber);
			IssuerSerialList.Add (xis);
		}

		public void AddSubjectKeyId (byte[] subjectKeyId) 
		{
			SubjectKeyIdList.Add (subjectKeyId);
		}

#if NET_2_0
		[MonoTODO]
		public void AddSubjectKeyId (string subjectKeyId)
		{
			throw new NotImplementedException ();
		}
#endif

		public void AddSubjectName (string subjectName) 
		{
			SubjectNameList.Add (subjectName);
		}

		public override XmlElement GetXml () 
		{
			// sanity check
			int count = IssuerSerialList.Count + SubjectKeyIdList.Count + SubjectNameList.Count + X509CertificateList.Count;
			if ((x509crl == null) && (count == 0))
				throw new CryptographicException ("value");

			XmlDocument document = new XmlDocument ();
			XmlElement xel = document.CreateElement (XmlSignature.ElementNames.X509Data, XmlSignature.NamespaceURI);
			// FIXME: hack to match MS implementation
			xel.SetAttribute ("xmlns", XmlSignature.NamespaceURI);
			// <X509IssuerSerial>
			if (IssuerSerialList.Count > 0) {
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
			if (SubjectKeyIdList.Count > 0) {
				foreach (byte[] skid in SubjectKeyIdList) {
					XmlElement ski = document.CreateElement (XmlSignature.ElementNames.X509SKI, XmlSignature.NamespaceURI);
					ski.InnerText = Convert.ToBase64String (skid);
					xel.AppendChild (ski);
				}
			}
			// <X509SubjectName>
			if (SubjectNameList.Count > 0) {
				foreach (string subject in SubjectNameList) {
					XmlElement sn = document.CreateElement (XmlSignature.ElementNames.X509SubjectName, XmlSignature.NamespaceURI);
					sn.InnerText = subject;
					xel.AppendChild (sn);
				}
			}
			// <X509Certificate>
			if (X509CertificateList.Count > 0) {
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

			IssuerSerialList.Clear ();
			SubjectKeyIdList.Clear ();
			SubjectNameList.Clear ();
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
