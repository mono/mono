//
// PublisherMembershipCondition.cs: Publisher Membership Condition
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2002 Motus Technologies Inc. (http://www.motus.com)
//

using System;
using System.Security.Cryptography.X509Certificates;

namespace System.Security.Policy {

public sealed class PublisherMembershipCondition : IMembershipCondition, ISecurityEncodable, ISecurityPolicyEncodable {

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

	public X509Certificate Certificate {
		get { return x509; }
		set { 
			if (value == null)
				throw new ArgumentNullException ("value");
			x509 = value; 
		}
	}

	[MonoTODO()]
	public bool Check (Evidence evidence) 
	{
		return true;
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

	[MonoTODO()]
	public void FromXml (SecurityElement e) 
	{
	}

	[MonoTODO()]
	public void FromXml (SecurityElement e, PolicyLevel level) 
	{
	}

	public override int GetHashCode () 
	{
		return x509.GetHashCode ();
	}

	public override string ToString () 
	{
		return "Publisher - " + x509.GetPublicKeyString ();
	}

	[MonoTODO()]
	public SecurityElement ToXml () 
	{
		return null;
	}

	[MonoTODO()]
	public SecurityElement ToXml (PolicyLevel level) 
	{
		return null;
	}
}

}
