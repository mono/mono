//
// Publisher.cs: Publisher Policy using X509 Certificate
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2002 Motus Technologies Inc. (http://www.motus.com)
//

using System;
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
			if (x509.GetRawCertData() == null)
				throw new NullReferenceException ("x509");
			return x509; 
		}
	}

	public object Copy () 
	{
		return (object) new Publisher (x509);
	}

	[MonoTODO("What should we do with the evidence ? nothing?")]
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
		StringBuilder sb = new StringBuilder ();
		sb.Append ("<System.Security.Policy.Publisher version=\"1\">\r\n   <X509v3Certificate");
		string cert = x509.GetRawCertDataString ();
		if (cert == null)
			sb.Append ("/>\r\n");
		else {
			sb.Append (">");
			sb.Append (cert);
			sb.Append ("</X509v3Certificate>\r\n");
		}
		sb.Append ("</System.Security.Policy.Publisher>\r\n");
		return sb.ToString ();
	}
}

}
