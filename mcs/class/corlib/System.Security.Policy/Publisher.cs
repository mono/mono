//
// Publisher.cs: Publisher Policy using X509 Certificate
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2002, 2003 Motus Technologies Inc. (http://www.motus.com)
//

using System;
using System.Security;
using System.Security.Cryptography.X509Certificates;
using System.Security.Permissions;
using System.Text;

namespace System.Security.Policy {

[Serializable]
public sealed class Publisher : IIdentityPermissionFactory {
	
	private X509Certificate x509;

	public Publisher (X509Certificate cert) 
	{
		if (cert == null)
			throw new ArgumentNullException ("cert");
		x509 = cert;
	}

	~Publisher () 
	{
		// X509Certificate doesn't have a Dispose 
		// (bad design as it deal with unmanaged code in Windows)
		// not really needed but corcompare will be happier
	}

	public X509Certificate Certificate { 
		get { 
			// needed to match MS implementation
			if (x509.GetRawCertData () == null)
				throw new NullReferenceException ("x509");
			return x509; 
		}
	}

	public object Copy () 
	{
		return (object) new Publisher (x509);
	}

	public IPermission CreateIdentityPermission (Evidence evidence) 
	{
		return new PublisherIdentityPermission (x509);
	}

	public override bool Equals (object o) 
	{
		if (!(o is Publisher))
			throw new ArgumentException ("not a Publisher");
		return x509.Equals ((o as Publisher).Certificate);
	}
	
	public override int GetHashCode () 
	{
		return x509.GetHashCode ();
	}

	public override string ToString ()
	{
		SecurityElement se = new SecurityElement ("System.Security.Policy.Publisher");
		se.AddAttribute ("version", "1");
		SecurityElement cert = new SecurityElement ("X509v3Certificate");
		string data = x509.GetRawCertDataString ();
		if (data != null)
			cert.Text = data;
		se.AddChild (cert);
		return se.ToString ();
	}
}

}
