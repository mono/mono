//
// StrongNameIdentityPermission.cs: Strong Name Identity Permission
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// (C) 2002, 2003 Motus Technologies Inc. (http://www.motus.com)
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
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

using System.Globalization;

namespace System.Security.Permissions {

	[Serializable]
	public sealed class StrongNameIdentityPermission : CodeAccessPermission, IBuiltInPermission {
	
		private const int version = 1;
		static private Version defaultVersion = new Version (0, 0);

		private StrongNamePublicKeyBlob publickey;
		private string name;
		private Version assemblyVersion;
	
		public StrongNameIdentityPermission (PermissionState state) 
		{
			// false == do not allow Unrestricted for Identity Permissions
			CheckPermissionState (state, false);
			// default values
			name = String.Empty;
			assemblyVersion = (Version) defaultVersion.Clone ();
		}
	
		public StrongNameIdentityPermission (StrongNamePublicKeyBlob blob, string name, Version version) 
		{
			if (blob == null)
				throw new ArgumentNullException ("blob");

			Name = name;
			publickey = blob;
			assemblyVersion = version;
		}
	
		public string Name { 
			get { return name; }
			set { 
#if NET_2_0
				if ((value != null) && (value.Length == 0))
					throw new ArgumentException ("name");
#endif
				name = value;
			}
		}
	
		public StrongNamePublicKeyBlob PublicKey { 
			get { return publickey; }
			set {
				if (value == null)
					throw new ArgumentNullException ("value");
				publickey = value;
			}
		}
	
		public Version Version { 
			get { return assemblyVersion; }
			set { assemblyVersion = value; }
		}
	
		public override IPermission Copy () 
		{
			if (IsEmpty ())
				return new StrongNameIdentityPermission (PermissionState.None);
			else
				return new StrongNameIdentityPermission (publickey, name, assemblyVersion);
			// Note: this will throw an ArgumentException if Name is still equals to String.Empty
			// but MS implementation has the same bug/design issue
		}
	
		public override void FromXml (SecurityElement e) 
		{
			// General validation in CodeAccessPermission
			CheckSecurityElement (e, "e", version, version);
			// Note: we do not (yet) care about the return value 
			// as we only accept version 1 (min/max values)

			name = e.Attribute ("Name");
			publickey = StrongNamePublicKeyBlob.FromString (e.Attribute ("PublicKeyBlob"));
			string v = e.Attribute ("AssemblyVersion");
			assemblyVersion = (v == null) ? null : new Version (v);
		}
	
		public override IPermission Intersect (IPermission target) 
		{
			StrongNameIdentityPermission snip = (target as StrongNameIdentityPermission);
			if (snip == null)
				return null;

			if (IsEmpty () || snip.IsEmpty ())
				return new StrongNameIdentityPermission (PermissionState.None);

			if (name != snip.name)
				return null;
			if (!assemblyVersion.Equals (snip.assemblyVersion))
				return null;
			if (!publickey.Equals (snip.publickey))
				return null;

			return Copy ();
		}
	
		public override bool IsSubsetOf (IPermission target) 
		{
			StrongNameIdentityPermission snip = Cast (target);
			if (snip == null)
				return IsEmpty ();

			if (((name != null) && (name.Length > 0)) && (name != snip.Name))
				return false;
			if ((assemblyVersion != null) && !assemblyVersion.Equals (snip.assemblyVersion))
				return false;
			return publickey.Equals (snip.publickey);
		}
	
		public override SecurityElement ToXml () 
		{
			SecurityElement se = Element (version);
			if (publickey != null)
				se.AddAttribute ("PublicKeyBlob", publickey.ToString ());
			if (name != null)
				se.AddAttribute ("Name", name);
			if (assemblyVersion != null)
				se.AddAttribute ("AssemblyVersion", assemblyVersion.ToString ());
			return se;
		}
	
		public override IPermission Union (IPermission target) 
		{
			StrongNameIdentityPermission snip = Cast (target);
			if ((snip == null) || snip.IsEmpty ())
				return Copy ();

			if (!publickey.Equals (snip.publickey)) {
#if NET_2_0
				string msg = Locale.GetText ("Permissions have different public keys.");
				throw new ArgumentException (msg, "target");
#else
				return null;
#endif
			}

			string n = name;
			if ((n == null) || (n.Length == 0)) {
				n = snip.name;
			}
			else if ((snip.name != null) && (snip.name.Length > 0) && (n != snip.name)) {
#if NET_2_0
				string msg = String.Format (Locale.GetText ("Name mismatch: '{0}' versus '{1}'"), n, snip.Name);
				throw new ArgumentException (msg, "target");
#else
				return null;
#endif
			}

			Version v = assemblyVersion;
			if (v == null) {
				v = snip.assemblyVersion;
			}
			else if ((snip.assemblyVersion != null) && (v != snip.assemblyVersion)) {
#if NET_2_0
				string msg = String.Format (Locale.GetText ("Version mismatch: '{0}' versus '{1}'"), v, snip.assemblyVersion);
				throw new ArgumentException (msg, "target");
#else
				return null;
#endif
			}

			return new StrongNameIdentityPermission (publickey, n, v);
		}
	
		// IBuiltInPermission
		int IBuiltInPermission.GetTokenIndex ()
		{
			return (int) BuiltInToken.StrongNameIdentity;
		}

		// helpers

		private bool IsEmpty ()
		{
			if (publickey != null)
				return false;
			if ((name != null) && (name.Length > 0))
				return false;
			return ((assemblyVersion == null) || defaultVersion.Equals (assemblyVersion));
		}

		private StrongNameIdentityPermission Cast (IPermission target)
		{
			if (target == null)
				return null;

			StrongNameIdentityPermission snip = (target as StrongNameIdentityPermission);
			if (snip == null) {
				ThrowInvalidPermission (target, typeof (StrongNameIdentityPermission));
			}

			return snip;
		}
	} 
}
