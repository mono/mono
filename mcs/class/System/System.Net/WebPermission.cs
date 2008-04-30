//
// System.Net.WebPermission.cs
//
// Author:
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//   (based on SocketPermission.cs)
//
// (C) 2003 Andreas Nahr
//

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

using System;
using System.Collections;
using System.Security;
using System.Security.Permissions;
using System.Text.RegularExpressions;

namespace System.Net {

	internal enum WebPermissionInfoType {
		InfoString,
		InfoUnexecutedRegex,
		InfoRegex,
	}

	internal class WebPermissionInfo {
		WebPermissionInfoType _type;
		object _info;

		public WebPermissionInfo (WebPermissionInfoType type, string info)
		{
			_type = type;
			_info = (string) info;
		}

		public WebPermissionInfo (Regex regex)
		{
			_type = WebPermissionInfoType.InfoRegex;
			_info = (object) regex;
		}

		public string Info {
			get {
				if (_type == WebPermissionInfoType.InfoRegex)
					return null;
				return (string) _info;
			}
		}
	}

	// (based on SocketPermission.cs - Please look there to implement missing members!)
	[MonoTODO ("Most private members that include functionallity are not implemented!")]
	[Serializable]
	public sealed class WebPermission : CodeAccessPermission, IUnrestrictedPermission
	{
		// Fields
		ArrayList m_acceptList = new ArrayList ();
		ArrayList m_connectList = new ArrayList ();
		bool m_noRestriction = false;

		// Constructors
		public WebPermission () : base () 
		{				
		}

		public WebPermission (PermissionState state) : base () 
		{						
			m_noRestriction = (state == PermissionState.Unrestricted);
		}

		public WebPermission (NetworkAccess access, string uriString) : base () 
		{
			AddPermission (access, uriString);
		}		

		public WebPermission (NetworkAccess access, Regex uriRegex) : base () 
		{
			AddPermission (access, uriRegex);
		}

		// Properties
		public IEnumerator AcceptList {
			get { return m_acceptList.GetEnumerator (); }
		}

		public IEnumerator ConnectList {
			get { return m_connectList.GetEnumerator (); }
		}

		// Methods

		public void AddPermission (NetworkAccess access, string uriString)
		{
			WebPermissionInfo info = new WebPermissionInfo (WebPermissionInfoType.InfoString, uriString); 
			AddPermission (access, info);
		}

		public void AddPermission (NetworkAccess access, Regex uriRegex)
		{
			WebPermissionInfo info = new WebPermissionInfo (uriRegex); 
			AddPermission (access, info);
		}

		internal void AddPermission (NetworkAccess access, WebPermissionInfo info)
		{
			switch (access) {
				case NetworkAccess.Accept:
					m_acceptList.Add (info);
					break;
				case NetworkAccess.Connect:
					m_connectList.Add (info);
					break;
				default:
					string msg = Locale.GetText ("Unknown NetworkAccess value {0}.");
					throw new ArgumentException (String.Format (msg, access), "access");
			}
		}

		public override IPermission Copy ()
		{
			WebPermission permission;
			permission = new WebPermission (m_noRestriction ? 
						PermissionState.Unrestricted : 
						PermissionState.None);

			// as EndpointPermission's are immutable it's safe to do a shallow copy.
			permission.m_connectList = (ArrayList) 
			this.m_connectList.Clone ();
			permission.m_acceptList = (ArrayList) this.m_acceptList.Clone ();
			return permission;
		}

		public override IPermission Intersect (IPermission target)
		{
			if (target == null) 
				return null;
			WebPermission perm = target as WebPermission;
			if (perm == null) 
				throw new ArgumentException ("Argument not of type WebPermission");
			if (m_noRestriction) 
				return IntersectEmpty (perm) ? null : perm.Copy ();
			if (perm.m_noRestriction)
				return IntersectEmpty (this) ? null : this.Copy ();
			WebPermission newperm = new WebPermission (PermissionState.None);
			Intersect (this.m_connectList, perm.m_connectList, newperm.m_connectList);
			Intersect (this.m_acceptList, perm.m_acceptList, newperm.m_acceptList);
			return IntersectEmpty (newperm) ? null : newperm;
		}

		private bool IntersectEmpty (WebPermission permission)
		{
			return !permission.m_noRestriction && 
			       (permission.m_connectList.Count == 0) &&
			       (permission.m_acceptList.Count == 0);
		}

		[MonoTODO]
		private void Intersect (ArrayList list1, ArrayList list2, ArrayList result)
		{
			throw new NotImplementedException ();
		}

		public override bool IsSubsetOf (IPermission target) 
		{
			if (target == null)
				return (!m_noRestriction && m_connectList.Count == 0 && m_acceptList.Count == 0);
			WebPermission perm = target as WebPermission;
			if (perm == null) 
				throw new ArgumentException ("Parameter target must be of type WebPermission");
			if (perm.m_noRestriction)
				return true;
			if (this.m_noRestriction)
				return false;
			if (this.m_acceptList.Count == 0 && this.m_connectList.Count == 0)
				return true;
			if (perm.m_acceptList.Count == 0 && perm.m_connectList.Count == 0)
				return false;
			return IsSubsetOf (this.m_connectList, perm.m_connectList)
			    && IsSubsetOf (this.m_acceptList, perm.m_acceptList);
		}


		[MonoTODO]
		private bool IsSubsetOf (ArrayList list1, ArrayList list2)
		{
			throw new NotImplementedException ();
		}

		public bool IsUnrestricted ()
		{
			return m_noRestriction;
		}

		public override SecurityElement ToXml ()
		{
			SecurityElement root = new SecurityElement ("IPermission");
			root.AddAttribute ("class", this.GetType ().AssemblyQualifiedName);
			root.AddAttribute ("version", "1");
			if (m_noRestriction) {
				root.AddAttribute ("Unrestricted", "true");
				return root;
			}				
			if (this.m_connectList.Count > 0)
				ToXml (root, "ConnectAccess", m_connectList.GetEnumerator ());
			if (this.m_acceptList.Count > 0) 
				ToXml (root, "AcceptAccess", m_acceptList.GetEnumerator ());
			return root;
		}

		private void ToXml (SecurityElement root, string childName, IEnumerator enumerator)
		{
			SecurityElement child = new SecurityElement (childName, null);

			root.AddChild (child);
			while (enumerator.MoveNext ()){
				WebPermissionInfo x = enumerator.Current as WebPermissionInfo;

				if (x == null) continue;

				SecurityElement uri = new SecurityElement ("URI");
				uri.AddAttribute ("uri", x.Info);
				child.AddChild (uri);
			}
		}

		public override void FromXml (SecurityElement securityElement)
		{
			if (securityElement == null)
				throw new ArgumentNullException ("securityElement");

			// LAMESPEC: it says to throw an ArgumentNullException in this case
			if (securityElement.Tag != "IPermission")
				throw new ArgumentException ("securityElement");

			string unrestricted = securityElement.Attribute ("Unrestricted");
			if (unrestricted != null) {
				this.m_noRestriction = (String.Compare (unrestricted, "true", true) == 0);
				if (this.m_noRestriction)
					return;
			}
			this.m_noRestriction = false;
			this.m_connectList = new ArrayList ();
			this.m_acceptList = new ArrayList ();
			ArrayList children = securityElement.Children;
			foreach (SecurityElement child in children) {
				if (child.Tag == "ConnectAccess") 
					FromXml (child.Children, NetworkAccess.Connect);
				else if (child.Tag == "AcceptAccess")
					FromXml (child.Children, NetworkAccess.Accept);
			}
		}

		private void FromXml (ArrayList endpoints, NetworkAccess access)
		{
			throw new NotImplementedException ();
		}

		public override IPermission Union (IPermission target) 
		{
			// LAMESPEC: according to spec we should throw an 
			// exception when target is null. We'll follow the
			// behaviour of MS.Net instead of the spec, also
			// because it matches the Intersect behaviour.

			if (target == null)
				return null;
				// throw new ArgumentNullException ("target");

			WebPermission perm = target as WebPermission;
			if (perm == null)
				throw new ArgumentException ("Argument not of type WebPermission");
			if (this.m_noRestriction || perm.m_noRestriction) 
				return new WebPermission (PermissionState.Unrestricted);

			WebPermission copy = (WebPermission) perm.Copy ();
			copy.m_acceptList.InsertRange (copy.m_acceptList.Count, this.m_acceptList);
			copy.m_connectList.InsertRange (copy.m_connectList.Count, this.m_connectList);

			return copy;
		}
	}
} 
