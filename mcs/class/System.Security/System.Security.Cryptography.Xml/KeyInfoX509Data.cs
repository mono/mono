//
// KeyInfoX509Data.cs - KeyInfoX509Data implementation for XML Signature
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2002, 2003 Motus Technologies Inc. (http://www.motus.com)
//

using System.Collections;
using System.Security.Cryptography.X509Certificates;
using System.Xml;

namespace System.Security.Cryptography.Xml {

	// FIXME: framework class isn't documented so compatibility isn't assured!
	internal class IssuerSerial {
		public string Issuer;
		public string Serial;

		public IssuerSerial (string issuer, string serial) 
		{
			Issuer = issuer;
			Serial = serial;
		}
	}

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
			X509CertificateList.Add (certificate);
		}

		public void AddIssuerSerial (string issuerName, string serialNumber) 
		{
			IssuerSerial isser = new IssuerSerial (issuerName, serialNumber);
			IssuerSerialList.Add (isser);
		}

		public void AddSubjectKeyId (byte[] subjectKeyId) 
		{
			SubjectKeyIdList.Add (subjectKeyId);
		}

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
				foreach (IssuerSerial iser in IssuerSerialList) {
					XmlElement isl = document.CreateElement (XmlSignature.ElementNames.X509IssuerSerial, XmlSignature.NamespaceURI);
					XmlElement xin = document.CreateElement (XmlSignature.ElementNames.X509IssuerName, XmlSignature.NamespaceURI);
					xin.InnerText = iser.Issuer;
					isl.AppendChild (xin);
 					XmlElement xsn = document.CreateElement (XmlSignature.ElementNames.X509SerialNumber, XmlSignature.NamespaceURI);
					xsn.InnerText = iser.Serial;
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

			XmlNodeList xnl = null;
			// <X509IssuerSerial>
			xnl = element.GetElementsByTagName (XmlSignature.ElementNames.X509IssuerSerial, XmlSignature.NamespaceURI);
			if (xnl != null) {
				for (int i=0; i < xnl.Count; i++) {
					XmlElement xel = (XmlElement) xnl[i];
					XmlNodeList issuer = xel.GetElementsByTagName (XmlSignature.ElementNames.X509IssuerName, XmlSignature.NamespaceURI);
					XmlNodeList serial = xel.GetElementsByTagName (XmlSignature.ElementNames.X509SerialNumber, XmlSignature.NamespaceURI);
					AddIssuerSerial (issuer[0].InnerText, serial[0].InnerText);
				}
			}
			// <X509SKI>
			xnl = element.GetElementsByTagName (XmlSignature.ElementNames.X509SKI, XmlSignature.NamespaceURI);
			if (xnl != null) {
				for (int i=0; i < xnl.Count; i++) {
					byte[] skid = Convert.FromBase64String (xnl[i].InnerXml);
					AddSubjectKeyId (skid);
				}
			}
			// <X509SubjectName>
			xnl = element.GetElementsByTagName (XmlSignature.ElementNames.X509SubjectName, XmlSignature.NamespaceURI);
			if (xnl != null) {
				for (int i=0; i < xnl.Count; i++) {
					AddSubjectName (xnl[i].InnerXml);
				}
			}
			// <X509Certificate>
			xnl = element.GetElementsByTagName (XmlSignature.ElementNames.X509Certificate, XmlSignature.NamespaceURI);
			if (xnl != null) {
				for (int i=0; i < xnl.Count; i++) {
					byte[] cert = Convert.FromBase64String (xnl[i].InnerXml);
					AddCertificate (new X509Certificate (cert));
				}
			}
			// only one <X509CRL> 
			xnl = element.GetElementsByTagName (XmlSignature.ElementNames.X509CRL, XmlSignature.NamespaceURI);
			if ((xnl != null) && (xnl.Count > 0)) {
				x509crl = Convert.FromBase64String (xnl[0].InnerXml);
			}
		}
	}
}