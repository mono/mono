//
// StrongName.cs: Strong Name
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2002 Motus Technologies Inc. (http://www.motus.com)
//

using System;
using System.Security.Permissions;
using System.Text;

namespace System.Security.Policy {

[Serializable]
public sealed class StrongName : IIdentityPermissionFactory, IBuiltInEvidence {

	private StrongNamePublicKeyBlob publickey;
	private string name;
	private Version version;

	public StrongName (StrongNamePublicKeyBlob blob, string name, Version version) 
	{
		if (blob == null)
			throw new ArgumentNullException ("blob");
		if (name == null)
			throw new ArgumentNullException ("name");
		if (version == null)
			throw new ArgumentNullException ("version");

		publickey = blob;
		this.name = name;
		this.version = version;
	}

	public string Name { 
		get { return name; }
	}

	public StrongNamePublicKeyBlob PublicKey { 
		get { return publickey; }
	}

	public Version Version { 
		get { return version; }
	}

	public object Copy () 
	{
		return (object) new StrongName (publickey, name, version);
	}

	public IPermission CreateIdentityPermission (Evidence evidence) 
	{
		return new StrongNameIdentityPermission (publickey, name, version);
	}

	public override bool Equals (object o) 
	{
		if (!(o is StrongName))
			return false;
		StrongName sn = (o as StrongName);
		if (name != sn.Name)
			return false;
		if (!Version.Equals (sn.Version))
			return false;
		return PublicKey.Equals (sn.PublicKey);
	}

	public override int GetHashCode () 
	{
		return publickey.GetHashCode ();
	}

	public override string ToString () 
	{
		SecurityElement element = new SecurityElement (typeof (System.Security.Policy.StrongName).FullName);
		element.AddAttribute ("version", "1");
		element.AddAttribute ("Key", publickey.ToString ());
		element.AddAttribute ("Name", name);
		element.AddAttribute ("Version", version.ToString ());
		return element.ToString ();
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
