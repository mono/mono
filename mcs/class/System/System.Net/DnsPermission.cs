//
// System.Net.DnsPermission.cs
//
// Author:
//   Lawrence Pit (loz@cable.a2000.nl)
//

using System;
using System.Collections;
using System.Security;
using System.Security.Permissions;

namespace System.Net
{
	[Serializable]
	public sealed class DnsPermission : CodeAccessPermission, IUnrestrictedPermission
	{
		// Fields
		bool m_noRestriction;
		
		// Constructors
		public DnsPermission (PermissionState state) : base () 
		{						
			m_noRestriction = (state == PermissionState.Unrestricted);
		}
		
		// Methods
				
		public override IPermission Copy ()
		{
			// this is immutable.
			return this;		
		}
		
		public override IPermission Intersect (IPermission target)
		{
			// LAMESPEC: says to throw an exception when null
			// but at same time it says to return null. We'll
			// follow MS behaviour.
			if (target == null) 
				return null;
			
			DnsPermission perm = target as DnsPermission;
			
			if (perm == null)
				throw new ArgumentException ("Argument not of type DnsPermission");
				
			if (this.m_noRestriction && perm.m_noRestriction)
				return this;
			
			return this.m_noRestriction ? perm : this;
		}
		
		public override bool IsSubsetOf (IPermission target) 
		{
			if (target == null)
				return !m_noRestriction;
			
			DnsPermission perm = target as DnsPermission;
			
			if (perm == null)
				throw new ArgumentException ("Argument not of type DnsPermission");
			
			return !this.m_noRestriction || perm.m_noRestriction;
		}

		public bool IsUnrestricted () 
		{
			return this.m_noRestriction;
		}

		/*
		
		DnsPermission dns1 = new DnsPermission (PermissionState.None);
		Console.WriteLine (dns1.ToXml ().ToString ());

		DnsPermission dns2 = new DnsPermission (PermissionState.Unrestricted);
		Console.WriteLine (dns2.ToXml ().ToString ());
		
		This is the sample xml output:

		<IPermission class="System.Net.DnsPermission, System, Version=1.0.3300.0, Cultur
		e=neutral, PublicKeyToken=b77a5c561934e089"
			     version="1"/>

		<IPermission class="System.Net.DnsPermission, System, Version=1.0.3300.0, Cultur
		e=neutral, PublicKeyToken=b77a5c561934e089"
			     version="1"
			     Unrestricted="true"/>
		*/
		public override SecurityElement ToXml ()
		{
             
			SecurityElement root = new SecurityElement ("IPermission");
			root.AddAttribute ("class", this.GetType ().AssemblyQualifiedName);
			root.AddAttribute ("version", "1");
			if (m_noRestriction)
				root.AddAttribute ("Unrestricted", "true");				

			return root;
		}
		
		public override void FromXml (SecurityElement securityElement)
		{
			if (securityElement == null)
				throw new ArgumentNullException ("securityElement");
				
			// LAMESPEC: it says to throw an ArgumentNullException in this case				
			if (securityElement.Tag != "IPermission")
				throw new ArgumentException ("securityElement");
				
			string classStr = securityElement.Attribute ("class");
			if (classStr == null || !classStr.StartsWith (this.GetType ().FullName + ","))
				throw new ArgumentException ("securityElement");
				
			string unrestricted = securityElement.Attribute ("Unrestricted");
			if (unrestricted != null) 
				this.m_noRestriction = (String.Compare (unrestricted, "true", true) == 0);
		}		
		
		public override IPermission Union (IPermission target) 
		{
			// LAMESPEC: according to spec we should throw an 
			// exception when target is null. We'll follow the
			// behaviour of MS.Net instead of the spec.
			if (target == null)
				return this;
				// throw new ArgumentNullException ("target");
				
			DnsPermission perm = target as DnsPermission;
			
			if (perm == null)
				throw new ArgumentException ("Argument not of type DnsPermission");
			
			return this.m_noRestriction ? this : perm;
		}

	}
}
