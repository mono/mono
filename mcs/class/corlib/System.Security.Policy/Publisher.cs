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
public sealed class Publisher : IIdentityPermissionFactory, IBuiltInEvidence {
	
	private X509Certificate m_cert;

	public Publisher (X509Certificate cert) 
	{
		if (cert == null)
			throw new ArgumentNullException ("cert");
		m_cert = cert;
	}

	public X509Certificate Certificate { 
		get { 
			// needed to match MS implementation
			if (m_cert.GetRawCertData () == null)
				throw new NullReferenceException ("m_cert");
			return m_cert; 
		}
	}

	public object Copy () 
	{
		return (object) new Publisher (m_cert);
	}

	public IPermission CreateIdentityPermission (Evidence evidence) 
	{
		return new PublisherIdentityPermission (m_cert);
	}

	public override bool Equals (object o) 
	{
		if (!(o is Publisher))
			throw new ArgumentException ("not a Publisher");
		return m_cert.Equals ((o as Publisher).Certificate);
	}
	
	public override int GetHashCode () 
	{
		return m_cert.GetHashCode ();
	}

	public override string ToString ()
	{
		SecurityElement se = new SecurityElement ("System.Security.Policy.Publisher");
		se.AddAttribute ("version", "1");
		SecurityElement cert = new SecurityElement ("X509v3Certificate");
		string data = m_cert.GetRawCertDataString ();
		if (data != null)
			cert.Text = data;
		se.AddChild (cert);
		return se.ToString ();
	}

	// interface IBuiltInEvidence

	[MonoTODO]
	int IBuiltInEvidence.GetRequiredSize (bool verbose) 
	{
		return 0;
	}

	[MonoTODO]
	int IBuiltInEvidence.InitFromBuffer (char [] buffer, int position) 
	{
		return 0;
	}

	[MonoTODO]
	int IBuiltInEvidence.OutputToBuffer (char [] buffer, int position, bool verbose) 
	{
		return 0;
	}
}

}
