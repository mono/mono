//
// StrongNameIdentityPermission.cs: Strong Name Identity Permission
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// (C) 2002, 2003 Motus Technologies Inc. (http://www.motus.com)
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

using System.Collections;
using System.Globalization;
using System.Runtime.InteropServices;

namespace System.Security.Permissions {

#if NET_2_0
	[ComVisible (true)]
#endif
	[Serializable]
	public sealed class StrongNameIdentityPermission : CodeAccessPermission, IBuiltInPermission {
	
		private const int version = 1;
		static private Version defaultVersion = new Version (0, 0);

		private struct SNIP {
			public StrongNamePublicKeyBlob PublicKey;
			public string Name;
			public Version AssemblyVersion;

			internal SNIP (StrongNamePublicKeyBlob pk, string name, Version version)
			{
				PublicKey = pk;
				Name = name;
				AssemblyVersion = version;
			}

			internal static SNIP CreateDefault ()
			{
				return new SNIP (null, String.Empty, (Version) defaultVersion.Clone ());
			}

			internal bool IsNameSubsetOf (string target) 
			{
				if (Name == null)
					return (target == null);
				if (target == null)
					return true;

				int wildcard = Name.LastIndexOf ('*');
				if (wildcard == 0)
					return true;		// *
				if (wildcard == -1)
					wildcard = Name.Length;	// exact match

				return (String.Compare (Name, 0, target, 0, wildcard, true, CultureInfo.InvariantCulture) == 0);
			}

			internal bool IsSubsetOf (SNIP target)
			{
				if ((PublicKey != null) && PublicKey.Equals (target.PublicKey))
					return true;

				if (!IsNameSubsetOf (target.Name))
					return false;
				if ((AssemblyVersion != null) && !AssemblyVersion.Equals (target.AssemblyVersion))
					return false;
				// in case PermissionState.None was used in the constructor
				if (PublicKey == null)
					return (target.PublicKey == null);
				return false;
			}
		}

#if NET_2_0
		private PermissionState _state;
		private ArrayList _list;

		public StrongNameIdentityPermission (PermissionState state) 
		{
			// Identity Permissions can be unrestricted in Fx 2.0
			_state = CheckPermissionState (state, true);
			// default values
			_list = new ArrayList ();
			_list.Add (SNIP.CreateDefault ());
		}

		public StrongNameIdentityPermission (StrongNamePublicKeyBlob blob, string name, Version version) 
		{
			if (blob == null)
				throw new ArgumentNullException ("blob");
			if ((name != null) && (name.Length == 0))
				throw new ArgumentException ("name");

			_state = PermissionState.None;
			_list = new ArrayList ();
			_list.Add (new SNIP (blob, name, version));
		}

		internal StrongNameIdentityPermission (StrongNameIdentityPermission snip) 
		{
			_state = snip._state;
			_list = new ArrayList (snip._list.Count);
			foreach (SNIP e in snip._list) {
				_list.Add (new SNIP (e.PublicKey, e.Name, e.AssemblyVersion));
			}
		}
#else
		private SNIP _single;

		public StrongNameIdentityPermission (PermissionState state) 
		{
			// false == do not allow Unrestricted for Identity Permissions
			CheckPermissionState (state, false);
			// default values
			_single = SNIP.CreateDefault ();
		}

		public StrongNameIdentityPermission (StrongNamePublicKeyBlob blob, string name, Version version) 
		{
			if (blob == null)
				throw new ArgumentNullException ("blob");

			_single = new SNIP (blob, name, version);
		}

		internal StrongNameIdentityPermission (StrongNameIdentityPermission snip) 
			: this (snip.PublicKey, snip.Name, snip.Version)
		{
		}
#endif

		// Properties

#if NET_2_0
		public string Name { 
			get {
				if (_list.Count > 1)
					throw new NotSupportedException ();
				return ((SNIP)_list [0]).Name;
			}
			set { 
				if ((value != null) && (value.Length == 0))
					throw new ArgumentException ("name");
				if (_list.Count > 1)
					ResetToDefault ();
				SNIP snip = (SNIP) _list [0];
				snip.Name = value;
				_list [0] = snip;
			}
		}

		public StrongNamePublicKeyBlob PublicKey { 
			get {
				if (_list.Count > 1)
					throw new NotSupportedException ();
				return ((SNIP)_list [0]).PublicKey;
			}
			set {
				if (value == null)
					throw new ArgumentNullException ("value");
				if (_list.Count > 1)
					ResetToDefault ();
				SNIP snip = (SNIP) _list [0];
				snip.PublicKey = value;
				_list [0] = snip;
			}
		}
	
		public Version Version { 
			get {
				if (_list.Count > 1)
					throw new NotSupportedException ();
				return ((SNIP)_list [0]).AssemblyVersion;
			}
			set {
				if (_list.Count > 1)
					ResetToDefault ();
				SNIP snip = (SNIP) _list [0];
				snip.AssemblyVersion = value;
				_list [0] = snip;
			}
		}

		internal void ResetToDefault ()
		{
			_list.Clear ();
			_list.Add (SNIP.CreateDefault ());
		}
#else
		public string Name { 
			get { return _single.Name; }
			set { _single.Name = value; }
		}

		public StrongNamePublicKeyBlob PublicKey { 
			get { return _single.PublicKey; }
			set {
				if (value == null)
					throw new ArgumentNullException ("value");
				_single.PublicKey = value;
			}
		}
	
		public Version Version { 
			get { return _single.AssemblyVersion; }
			set { _single.AssemblyVersion = value; }
		}
#endif

		// Methods
	
		public override IPermission Copy () 
		{
			if (IsEmpty ())
				return new StrongNameIdentityPermission (PermissionState.None);
			else
				return new StrongNameIdentityPermission (this);
		}
	
		public override void FromXml (SecurityElement e) 
		{
			// General validation in CodeAccessPermission
			CheckSecurityElement (e, "e", version, version);
			// Note: we do not (yet) care about the return value 
			// as we only accept version 1 (min/max values)
#if NET_2_0
			_list.Clear ();
			if ((e.Children != null) && (e.Children.Count > 0)) {
				foreach (SecurityElement se in e.Children) {
					_list.Add (FromSecurityElement (se));
				}
			} else {
				_list.Add (FromSecurityElement (e));
			}
#else
			_single = FromSecurityElement (e);
#endif
		}

		private SNIP FromSecurityElement (SecurityElement se)
		{
			string name = se.Attribute ("Name");
			StrongNamePublicKeyBlob publickey = StrongNamePublicKeyBlob.FromString (se.Attribute ("PublicKeyBlob"));
			string v = se.Attribute ("AssemblyVersion");
			Version assemblyVersion = (v == null) ? null : new Version (v);

			return new SNIP (publickey, name, assemblyVersion);
		}
#if NET_2_0
		public override IPermission Intersect (IPermission target) 
		{
			if (target == null)
				return null;
			StrongNameIdentityPermission snip = (target as StrongNameIdentityPermission);
			if (snip == null) 
				throw new ArgumentException (Locale.GetText ("Wrong permission type."));
			if (IsEmpty () || snip.IsEmpty ())
				return null;
			if (!Match (snip.Name))
				return null;

			string n = ((Name.Length < snip.Name.Length) ? Name : snip.Name);
			if (!Version.Equals (snip.Version))
				return null;
			if (!PublicKey.Equals (snip.PublicKey))
				return null;

			return new StrongNameIdentityPermission (this.PublicKey, n, this.Version);
		}

		public override bool IsSubsetOf (IPermission target) 
		{
			StrongNameIdentityPermission snip = Cast (target);
			if (snip == null)
				return IsEmpty ();

			if (IsEmpty ())
				return true;
			if (IsUnrestricted ())
				return snip.IsUnrestricted ();
			else if (snip.IsUnrestricted ())
				return true;

			foreach (SNIP e in _list) {
				foreach (SNIP t in snip._list) {
					if (!e.IsSubsetOf (t))
						return false;
				}
			}
			return true;
		}
#else
		public override IPermission Intersect (IPermission target) 
		{
			StrongNameIdentityPermission snip = (target as StrongNameIdentityPermission);
			if ((snip == null) || IsEmpty ())
				return null;
			if (snip.IsEmpty ())
				return new StrongNameIdentityPermission (PermissionState.None);
			if (!Match (snip.Name))
				return null;

			string n = ((Name.Length < snip.Name.Length) ? Name : snip.Name);
			if (!Version.Equals (snip.Version))
				return null;
			if (!PublicKey.Equals (snip.PublicKey))
				return null;

			return new StrongNameIdentityPermission (this.PublicKey, n, this.Version);
		}

		public override bool IsSubsetOf (IPermission target) 
		{
			StrongNameIdentityPermission snip = Cast (target);
			if (snip == null)
				return IsEmpty ();
			if (IsEmpty ())
				return true;

			return _single.IsSubsetOf (snip._single);
		}
#endif
	
		public override SecurityElement ToXml () 
		{
			SecurityElement se = Element (version);
#if NET_2_0
			if (_list.Count > 1) {
				foreach (SNIP snip in _list) {
					SecurityElement child = new SecurityElement ("StrongName");
					ToSecurityElement (child, snip);
					se.AddChild (child);
				}
			} else if (_list.Count == 1) {
				SNIP snip = (SNIP)_list [0];
				if (!IsEmpty (snip))
					ToSecurityElement (se, snip);
			}
#else
			ToSecurityElement (se, _single);
#endif
			return se;
		}

		private void ToSecurityElement (SecurityElement se, SNIP snip)
		{
			if (snip.PublicKey != null)
				se.AddAttribute ("PublicKeyBlob", snip.PublicKey.ToString ());
			if (snip.Name != null)
				se.AddAttribute ("Name", snip.Name);
			if (snip.AssemblyVersion != null)
				se.AddAttribute ("AssemblyVersion", snip.AssemblyVersion.ToString ());
		}

#if NET_2_0
		public override IPermission Union (IPermission target) 
		{
			StrongNameIdentityPermission snip = Cast (target);
			if ((snip == null) || snip.IsEmpty ())
				return Copy ();

			if (IsEmpty ())
				return snip.Copy ();

			StrongNameIdentityPermission union = (StrongNameIdentityPermission) Copy ();
			foreach (SNIP e in snip._list) {
				if (!IsEmpty (e) && !Contains (e)) {
					union._list.Add (e);
				}
			}
			return union;
		}
#else
		public override IPermission Union (IPermission target) 
		{
			StrongNameIdentityPermission snip = Cast (target);
			if ((snip == null) || snip.IsEmpty ())
				return Copy ();

			if (IsEmpty ())
				return snip.Copy ();

			if (!PublicKey.Equals (snip.PublicKey)) {
				return null;
			}

			string n = Name;
			if ((n == null) || (n.Length == 0)) {
				n = snip.Name;
			}
			else if (Match (snip.Name)) {
				n = ((Name.Length > snip.Name.Length) ? Name : snip.Name);
			}
			else if ((snip.Name != null) && (snip.Name.Length > 0) && (n != snip.Name)) {
				return null;
			}

			Version v = Version;
			if (v == null) {
				v = snip.Version;
			}
			else if ((snip.Version != null) && (v != snip.Version)) {
				return null;
			}

			return new StrongNameIdentityPermission (PublicKey, n, v);
		}
#endif
	
		// IBuiltInPermission
		int IBuiltInPermission.GetTokenIndex ()
		{
			return (int) BuiltInToken.StrongNameIdentity;
		}

		// helpers

#if NET_2_0
		private bool IsUnrestricted ()
		{
			return (_state == PermissionState.Unrestricted);
		}

		private bool Contains (SNIP snip)
		{
			foreach (SNIP e in _list) {
				bool pk = (((e.PublicKey == null) && (snip.PublicKey == null)) ||
					((e.PublicKey != null) && e.PublicKey.Equals (snip.PublicKey)));
				bool name = e.IsNameSubsetOf (snip.Name);
				bool version = (((e.AssemblyVersion == null) && (snip.AssemblyVersion == null)) ||
					((e.AssemblyVersion != null) && e.AssemblyVersion.Equals (snip.AssemblyVersion)));

				if (pk && name && version)
					return true;
			}
			return false;
		}

		private bool IsEmpty (SNIP snip)
		{
			if (PublicKey != null)
				return false;
			if ((Name != null) && (Name.Length > 0))
				return false;
			return ((Version == null) || defaultVersion.Equals (Version));
		}

#endif
		private bool IsEmpty ()
		{
#if NET_2_0
			if (IsUnrestricted () || (_list.Count > 1))
				return false;
#endif
			if (PublicKey != null)
				return false;
			if ((Name != null) && (Name.Length > 0))
				return false;
			return ((Version == null) || defaultVersion.Equals (Version));
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

		private bool Match (string target) 
		{
			if ((Name == null) || (target == null))
				return false;

			int wcu = Name.LastIndexOf ('*');
			int wct = target.LastIndexOf ('*');
			int length = Int32.MaxValue;

			if ((wcu == -1) && (wct == -1)) {
				// no wildcard, this is an exact match
				length = Math.Max (Name.Length, target.Length);
			}
			else if (wcu == -1) {
				// only "target" has a wildcard, use it
				length = wct;
			}
			else if (wct == -1) {
				// only "this" has a wildcard, use it
				length = wcu;
			}
			else {
				// both have wildcards, partial match with the smallest
				length = Math.Min (wcu, wct);
			}

			return (String.Compare (Name, 0, target, 0, length, true, CultureInfo.InvariantCulture) == 0);
		}
	} 
}
