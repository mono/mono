//
// PublisherMembershipCondition.cs: Publisher Membership Condition
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2002, 2003 Motus Technologies Inc. (http://www.motus.com)
//

using System;
using System.Collections;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace System.Security.Policy {

	[Serializable]
	public sealed class PublisherMembershipCondition
                : IConstantMembershipCondition, IMembershipCondition, ISecurityEncodable, ISecurityPolicyEncodable {
	
		private X509Certificate x509;
	
		// LAMESPEC: Undocumented ArgumentNullException exception
		public PublisherMembershipCondition (X509Certificate certificate) 
		{
			if (certificate == null)
				throw new ArgumentNullException ("certificate");
			// needed to match MS implementation
			if (certificate.GetRawCertData() == null)
				throw new NullReferenceException ("certificate");
			x509 = certificate;
		}
	
		// LAMESPEC: Undocumented ArgumentNullException exception
		public X509Certificate Certificate {
			get { return x509; }
			set { 
				if (value == null)
					throw new ArgumentNullException ("value");
				x509 = value; 
			}
		}
	
		public bool Check (Evidence evidence) 
		{
			IEnumerator e = evidence.GetHostEnumerator ();
			while (e.MoveNext ()) {
				if (e.Current is Publisher) {
					if (x509.Equals ((e.Current as Publisher).Certificate))
						return true;
				}
			}
			return false;
		}
	
		public IMembershipCondition Copy () 
		{
			return new PublisherMembershipCondition (x509);
		}
	
		public override bool Equals (object o) 
		{
			if (!(o is PublisherMembershipCondition))
				throw new ArgumentException ("not a PublisherMembershipCondition");
			return x509.Equals ((o as PublisherMembershipCondition).Certificate);
		}
	
		public void FromXml (SecurityElement e) 
		{
			FromXml (e, null);
		}
	
		private byte FromHexChar (char c) 
		{
			if ((c >= 'A') && (c <= 'F'))
				return (byte) (c - 'A' + 10);
			if ((c >= '0') && (c <= '9'))
				return (byte) (c - '0');
			throw new ArgumentException ("invalid hex char");
		}

		public void FromXml (SecurityElement e, PolicyLevel level) 
		{
			if (e == null)
				throw new ArgumentNullException ("e");
			if (e.Tag != "IMembershipCondition")
				throw new ArgumentException ("Not IMembershipCondition", "e");
			// PolicyLevel isn't used as there's no need to resolve NamedPermissionSet references
			string cert = e.Attribute ("X509Certificate");
			if (cert != null) {
				byte[] rawcert = new byte [cert.Length >> 1];
				int n = 0;
				int i = 0;
				while (n < rawcert.Length) {
					rawcert [n] = (byte) (FromHexChar (cert[i++]) << 4);
					rawcert [n++] += FromHexChar (cert[i++]);
				}
				x509 = new X509Certificate (rawcert);
			}
		}
	
		public override int GetHashCode () 
		{
			return x509.GetHashCode ();
		}
	
		public override string ToString () 
		{
			return "Publisher - " + x509.GetPublicKeyString ();
		}

		// snippet moved from FileIOPermission (nickd) to be reused in all derived classes
		internal SecurityElement Element (object o, int version) 
		{
			SecurityElement se = new SecurityElement ("IMembershipCondition");
			Type type = this.GetType ();
			StringBuilder asmName = new StringBuilder (type.Assembly.ToString ());
			asmName.Replace ('\"', '\'');
			se.AddAttribute ("class", type.FullName + ", " + asmName);
			se.AddAttribute ("version", version.ToString ());
			return se;
		}
	
		public SecurityElement ToXml () 
		{
			return ToXml (null);
		}
	
		public SecurityElement ToXml (PolicyLevel level) 
		{
			// PolicyLevel isn't used as there's no need to resolve NamedPermissionSet references
			SecurityElement se = Element (this, 1);
			se.AddAttribute ("X509Certificate", x509.GetRawCertDataString ());
			return se;
		}
	}
}
