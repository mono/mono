//
// PublisherIdentityPermission.cs: Publisher Identity Permission
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2002 Motus Technologies Inc. (http://www.motus.com)
//

using System;
using System.Security.Cryptography.X509Certificates;

namespace System.Security.Permissions {

public sealed class PublisherIdentityPermission : CodeAccessPermission {

	private X509Certificate x509;

	public PublisherIdentityPermission (PermissionState state) 
	{
		if (state == PermissionState.Unrestricted)
			throw new ArgumentException ("state");
	}

	public PublisherIdentityPermission (X509Certificate certificate) 
	{
		x509 = certificate;
	}

	public X509Certificate Certificate { 
		get { return x509; }
		set { x509 = value; }
	}

	[MonoTODO]
	public override IPermission Copy () 
	{
		throw new NotImplementedException ();
	}

	[MonoTODO]
	public override void FromXml (SecurityElement esd) 
	{
		throw new NotImplementedException ();
	}

	[MonoTODO]
	public override IPermission Intersect (IPermission target) 
	{
		throw new NotImplementedException ();
	}

	[MonoTODO]
	public override bool IsSubsetOf (IPermission target) 
	{
		throw new NotImplementedException ();
	}

	[MonoTODO]
	public override SecurityElement ToXml () 
	{
		throw new NotImplementedException ();
	}

	[MonoTODO]
	public override IPermission Union (IPermission target) 
	{
		throw new NotImplementedException ();
	}
} 

}
