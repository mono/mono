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
public sealed class StrongName : IIdentityPermissionFactory {

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

	[MonoTODO("What should we do with the evidence ? nothing?")]
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
		StringBuilder sb = new StringBuilder ();
		sb.Append ("<StrongName version=\"1\"\r\n            Key=\"");
		sb.Append (publickey.ToString ());
		sb.Append ("\"\r\n            Name=\"");
		sb.Append (name);
		sb.Append ("\"\r\n            Version=\"");
		sb.Append (version.ToString ());
		sb.Append ("\"/>\r\n");
		return sb.ToString ();
	}
}

}
