//
// PublisherIdentityPermission.cs: Publisher Identity Permission
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2002, 2003 Motus Technologies Inc. (http://www.motus.com)
//

using System;
using System.Security.Cryptography.X509Certificates;

namespace System.Security.Permissions {

	[Serializable]
	public sealed class PublisherIdentityPermission : CodeAccessPermission, IBuiltInPermission {
	
		private X509Certificate x509;
	
		public PublisherIdentityPermission (PermissionState state) 
		{
			switch (state) {
				case PermissionState.None:
					break;
				case PermissionState.Unrestricted:
					throw new ArgumentException ("Invalid state for identity permission", "state");
				default:
					throw new ArgumentException ("Unknown state", "state");
			}
		}
	
		public PublisherIdentityPermission (X509Certificate certificate) 
		{
			Certificate = certificate;
		}
	
		public X509Certificate Certificate { 
			get { return x509; }
			set { 
				if (value == null)
					throw new ArgumentNullException ("value");
				x509 = value; 
			}
		}
	
		public override IPermission Copy () 
		{
			PublisherIdentityPermission p = new PublisherIdentityPermission (PermissionState.None);
			if (x509 != null)
				p.Certificate = x509;
			return p;
		}

		private byte FromHexChar (char c) 
		{
			if ((c >= 'A') && (c <= 'F'))
				return (byte) (c - 'A' + 10);
			if ((c >= '0') && (c <= '9'))
				return (byte) (c - '0');
			throw new ArgumentException ("invalid hex char");
		}
	
		public override void FromXml (SecurityElement esd) 
		{
			if (esd == null)
				throw new ArgumentNullException ("esd");
			if (esd.Tag != "IPermission")
				throw new ArgumentException ("not IPermission");
			if (!(esd.Attributes ["class"] as string).StartsWith ("System.Security.Permissions.PublisherIdentityPermission"))
				throw new ArgumentException ("not PublisherIdentityPermission");
			if ((esd.Attributes ["version"] as string) != "1")
				throw new ArgumentException ("wrong version");

			string cert = (esd.Attributes ["X509v3Certificate"] as string);
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
	
		public override IPermission Intersect (IPermission target) 
		{
			if (target == null)
				return null;
			if (! (target is PublisherIdentityPermission))
				throw new ArgumentException ("wrong type");

			PublisherIdentityPermission o = (PublisherIdentityPermission) target;

			if ((x509 != null) && (o.x509 != null)) {
				if (x509.GetRawCertDataString () == o.x509.GetRawCertDataString ())
					return new PublisherIdentityPermission (o.x509);
			}
			return null;
		}
	
		public override bool IsSubsetOf (IPermission target) 
		{
			if (target == null)
				return false;

			if (! (target is PublisherIdentityPermission))
				throw new ArgumentException ("wrong type");

			PublisherIdentityPermission o = (PublisherIdentityPermission) target;
			if (x509 == null)
				return true;
			if (o.x509 == null)
				return false;
			return (x509.GetRawCertDataString () == o.x509.GetRawCertDataString ());
		}
	
		public override SecurityElement ToXml () 
		{
			SecurityElement se = Element (this, 1);
			if (x509 != null)
				se.AddAttribute ("X509v3Certificate", x509.GetRawCertDataString ());
			return se;
		}
	
		public override IPermission Union (IPermission target) 
		{
			if (target == null)
				return Copy ();
			if (! (target is PublisherIdentityPermission))
				throw new ArgumentException ("wrong type");

			PublisherIdentityPermission o = (PublisherIdentityPermission) target;

			if ((x509 != null) && (o.x509 != null)) {
				if (x509.GetRawCertDataString () == o.x509.GetRawCertDataString ())
					return new PublisherIdentityPermission (x509); // any cert would do
			}
			else if ((x509 == null) && (o.x509 != null))
				return new PublisherIdentityPermission (o.x509);
			else if ((x509 != null) && (o.x509 == null))
				return new PublisherIdentityPermission (x509);
			return null;
		}

		// IBuiltInPermission
		int IBuiltInPermission.GetTokenIndex ()
		{
			return 9;
		}
	} 
}
