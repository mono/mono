//
// System.Net.SocketPermissionAttribute.cs
//
// Author:
//   Lawrence Pit (loz@cable.a2000.nl)
//

using System;
using System.Security;
using System.Security.Permissions;

namespace System.Net
{
	[AttributeUsage (AttributeTargets.Assembly 
	               | AttributeTargets.Class 
	               | AttributeTargets.Struct 
	               | AttributeTargets.Constructor 
	               | AttributeTargets.Method, AllowMultiple = true, Inherited = false)
	]	
	[Serializable]
	public sealed class SocketPermissionAttribute : CodeAccessSecurityAttribute
	{
		// Fields
		string m_access;
		string m_host;
		string m_port;
		string m_transport;
		
		// Constructors
		public SocketPermissionAttribute (SecurityAction action) : base (action)
		{
		}

		// Properties

		public string Access {
			get { return m_access; }
			set { 
				if (m_access != null)
					throw new ArgumentException ("The parameter 'Access' can be set only once.");
				if (value == null) 
					throw new ArgumentException ("The parameter 'Access' cannot be null.");
				m_access = value;
			}
		}

		public string Host {
			get { return m_host; }
			set { 
				if (m_host != null)
					throw new ArgumentException ("The parameter 'Host' can be set only once.");
				if (value == null) 
					throw new ArgumentException ("The parameter 'Host' cannot be null.");					
				m_host = value;
			}
		}

		public string Port {
			get { return m_port; }
			set { 
				if (m_port != null)
					throw new ArgumentException ("The parameter 'Port' can be set only once.");
				if (value == null) 
					throw new ArgumentException ("The parameter 'Port' cannot be null.");					
				m_port = value;
			}
		}

		public string Transport {
			get { return m_transport; }
			set { 
				if (m_transport != null)
					throw new ArgumentException ("The parameter 'Transport' can be set only once.");
				if (value == null) 
					throw new ArgumentException ("The parameter 'Transport' cannot be null.");					
				m_transport = value;
			}
		}
		
		// Methods
		
		public override IPermission CreatePermission () {
			if (this.Unrestricted)
				return new SocketPermission (PermissionState.Unrestricted);

			if (m_access == null) 
				throw new ArgumentException ("The value for 'Access' must be specified.");
			if (m_host == null) 
				throw new ArgumentException ("The value for 'Host' must be specified.");
			if (m_port == null) 
				throw new ArgumentException ("The value for 'Port' must be specified.");
			if (m_transport == null) 
				throw new ArgumentException ("The value for 'Transport' must be specified.");

			NetworkAccess access;
			TransportType transport;
			int port = SocketPermission.AllPorts;

			if (String.Compare (m_access, "Connect", true) == 0)
				access = NetworkAccess.Connect;
			else if (String.Compare (m_access, "Accept", true) == 0)
				access = NetworkAccess.Accept;
			else 
				throw new ArgumentException ("The parameter value 'Access=" + m_access + "' is invalid.");

			if (String.Compare (m_port, "All", true) != 0) {
				try {
					port = Int32.Parse (m_port);					
				} catch (Exception) {
					throw new ArgumentException ("The parameter value 'Port=" + port + "' is invalid.");
				}
				// test whether port number is valid..
				new IPEndPoint (1, port);
			}

			try {
				transport = (TransportType) Enum.Parse (typeof (TransportType), m_transport, true);
			} catch (Exception) {
				throw new ArgumentException ("The parameter value 'Transport=" + m_transport + "' is invalid.");
			}
						
			SocketPermission perm = new SocketPermission (PermissionState.None);
			perm.AddPermission (access, transport, m_host, port);
			return perm;
		}		
	}
}
