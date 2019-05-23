//
// System.Net.SocketPermission.cs
//
// Author:
//   Lawrence Pit (loz@cable.a2000.nl)
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

using System.Collections;
using System.Security;
using System.Security.Permissions;

namespace System.Net {

	[Serializable]
	public sealed class SocketPermission : CodeAccessPermission, IUnrestrictedPermission
	{
		// Fields
		ArrayList m_acceptList = new ArrayList ();
		ArrayList m_connectList = new ArrayList ();
		bool m_noRestriction;
		
		// Constructors
		public SocketPermission (PermissionState state) : base () 
		{						
			m_noRestriction = (state == PermissionState.Unrestricted);
		}
		
		public SocketPermission (NetworkAccess access, TransportType transport, 
					 string hostName, int portNumber) : base () 
		{
			m_noRestriction = false;
			AddPermission (access, transport, hostName, portNumber);
		}	
		
		// Fields
		public const int AllPorts = -1;
		
		// Properties

		public IEnumerator AcceptList {
			get { return m_acceptList.GetEnumerator (); }
		}

		public IEnumerator ConnectList {
			get { return m_connectList.GetEnumerator (); }
		}
		
		// Methods
		
		public void AddPermission (NetworkAccess access, TransportType transport,
					   string hostName, int portNumber)
		{
			if (m_noRestriction)
				return;
				
			EndpointPermission permission = new EndpointPermission (hostName, portNumber, transport);

			if (access == NetworkAccess.Accept)
				m_acceptList.Add (permission);
			else
				m_connectList.Add (permission);
		}		
		
		public override IPermission Copy ()
		{
			SocketPermission permission;

			permission = new SocketPermission (m_noRestriction ? 
						PermissionState.Unrestricted : 
						PermissionState.None);

			// as EndpointPermission's are immutable it's safe to do a shallow copy.						
			permission.m_connectList = (ArrayList) this.m_connectList.Clone ();
			permission.m_acceptList = (ArrayList) this.m_acceptList.Clone ();

			return permission;		
		}
		
		public override IPermission Intersect (IPermission target)
		{
			if (target == null) 
				return null;
				
			SocketPermission perm = target as SocketPermission;
			if (perm == null) 
				throw new ArgumentException ("Argument not of type SocketPermission");
			
			if (m_noRestriction) 
				return IntersectEmpty (perm) ? null : perm.Copy ();
				
			if (perm.m_noRestriction)
				return IntersectEmpty (this) ? null : this.Copy ();
				
			SocketPermission newperm = new SocketPermission (PermissionState.None);
			Intersect (this.m_connectList, perm.m_connectList, newperm.m_connectList);
			Intersect (this.m_acceptList, perm.m_acceptList, newperm.m_acceptList);
			return IntersectEmpty (newperm) ? null : newperm;			
		}
		
		private bool IntersectEmpty (SocketPermission permission)		
		{
			return !permission.m_noRestriction && 
			       (permission.m_connectList.Count == 0) &&
			       (permission.m_acceptList.Count == 0);
		}
		
		private void Intersect (ArrayList list1, ArrayList list2, ArrayList result)
		{
			foreach (EndpointPermission perm1 in list1) {
				foreach (EndpointPermission perm2 in list2) {
					EndpointPermission perm = perm1.Intersect (perm2);
					if (perm != null) {
						// instead of the below it's also okay to simply do:
						//     result.Add (perm);
						// below is only done to avoid double entries						
						bool replaced = false;
						for (int i = 0; i < result.Count; i++) {
							EndpointPermission res = (EndpointPermission) result [i];
							EndpointPermission resperm = perm.Intersect (res);
							if (resperm != null) {
								result [i] = resperm;
								replaced = true;
								break;
							}
						}
						if (!replaced) 
							result.Add (perm);
					}
				}
			}
		}
		
		public override bool IsSubsetOf (IPermission target) 
		{
			if (target == null)
				return (!m_noRestriction && m_connectList.Count == 0 && m_acceptList.Count ==	 0);
			
			SocketPermission perm = target as SocketPermission;

			if (perm == null) 
				throw new ArgumentException ("Parameter target must be of type SocketPermission");
			
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

		private bool IsSubsetOf (ArrayList list1, ArrayList list2)
		{
			foreach (EndpointPermission perm1 in list1) {
				bool issubset = false;
				foreach (EndpointPermission perm2 in list2) 
					if (perm1.IsSubsetOf (perm2)) {
						issubset = true;
						break;
					}
				if (!issubset) 
					return false;
			}
			return true;
		}
		
		public bool IsUnrestricted () 
		{
			return m_noRestriction;
		}

		/*
		
		SocketPermission s = new SocketPermission (NetworkAccess.Connect, TransportType.Tcp, "www.example.com", 80);
		s.AddPermission (NetworkAccess.Accept, TransportType.All, "localhost", 8080);
		s.AddPermission (NetworkAccess.Accept, TransportType.All, "localhost", SocketPermission.AllPorts);
		// s = new SocketPermission (PermissionState.None);
		SecurityElement sec = s.ToXml ();	
		Console.WriteLine (sec.ToString ());

		This is sample xml output:

		<IPermission class="System.Net.SocketPermission, System, Version=1.0.3300.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"
			     version="1">
		   <ConnectAccess>
		      <ENDPOINT host="www.example.com"
				transport="Tcp"
				port="80"/>
		   </ConnectAccess>
		   <AcceptAccess>
		      <ENDPOINT host="localhost"
				transport="All"
				port="8080"/>
		      <ENDPOINT host="localhost"
				transport="All"
				port="All"/>
		   </AcceptAccess>
		</IPermission>



		This is a sample unrestricted socketpermission, no matter how many permissions you add:			

		<IPermission class="System.Net.SocketPermission, System, Version=1.0.3300.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"
			     version="1"
			     Unrestricted="true"/>


		This is a sample constructed restricted socketpermission with no permissions added:

		<IPermission class="System.Net.SocketPermission, System, Version=1.0.3300.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"
			     version="1"/>
		*/
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
			SecurityElement child = new SecurityElement (childName);
			while (enumerator.MoveNext ()) {
				EndpointPermission perm = enumerator.Current as EndpointPermission;
				SecurityElement grandchild = new SecurityElement ("ENDPOINT");
				grandchild.AddAttribute ("host", perm.Hostname);
				grandchild.AddAttribute ("transport", perm.Transport.ToString ());
				grandchild.AddAttribute ("port", 
						perm.Port == AllPorts 
						? "All" 
						: ((Int32) perm.Port).ToString ());
				child.AddChild (grandchild);
			}
			root.AddChild (child);
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
			foreach (SecurityElement endpoint in endpoints) {
				if (endpoint.Tag != "ENDPOINT")
					continue;
				string hostname = endpoint.Attribute ("host");
				TransportType transport = 
					(TransportType) Enum.Parse (typeof (TransportType), 
							            endpoint.Attribute ("transport"), 
							            true);
				string p = endpoint.Attribute ("port");
				int port = 0;
				if (p == "All") 
					port = SocketPermission.AllPorts;
				else
					port = Int32.Parse (p);

				AddPermission (access, transport, hostname, port);
			}
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
				
			SocketPermission perm = target as SocketPermission;
			if (perm == null)
				throw new ArgumentException ("Argument not of type SocketPermission");
			
			if (this.m_noRestriction || perm.m_noRestriction) 
				return new SocketPermission (PermissionState.Unrestricted);
				
			SocketPermission copy = (SocketPermission) perm.Copy ();
			copy.m_acceptList.InsertRange (copy.m_acceptList.Count, this.m_acceptList);
			copy.m_connectList.InsertRange (copy.m_connectList.Count, this.m_connectList);				
			
			return copy;
		}
	}
}
