//
// StrongName.cs: Strong Name
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// (C) 2002 Motus Technologies Inc. (http://www.motus.com)
// Copyright (C) 2004-2005 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System.Security.Permissions;
using System.Runtime.InteropServices;

namespace System.Security.Policy {

[Serializable]
[ComVisible (true)]
public sealed class StrongName :
#if NET_4_0
		EvidenceBase,
#endif
		IIdentityPermissionFactory, IBuiltInEvidence {

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
		if (name.Length == 0)
			throw new ArgumentException (Locale.GetText ("Empty"), "name");
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
		StrongName sn = (o as StrongName);
		if (sn == null)
			return false;
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
		SecurityElement element = new SecurityElement (typeof (System.Security.Policy.StrongName).Name);
		element.AddAttribute ("version", "1");
		element.AddAttribute ("Key", publickey.ToString ());
		element.AddAttribute ("Name", name);
		element.AddAttribute ("Version", version.ToString ());
		return element.ToString ();
	}

	// interface IBuiltInEvidence

	int IBuiltInEvidence.GetRequiredSize (bool verbose) 
	{
		return (verbose ? 5 : 1) + name.Length;
	}

	[MonoTODO ("IBuiltInEvidence")]
	int IBuiltInEvidence.InitFromBuffer (char [] buffer, int position) 
	{
		return 0;
	}

	[MonoTODO ("IBuiltInEvidence")]
	int IBuiltInEvidence.OutputToBuffer (char [] buffer, int position, bool verbose) 
	{
		return 0;
	}
}

}
