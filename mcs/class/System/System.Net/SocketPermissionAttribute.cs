//
// System.Net.SocketPermissionAttribute.cs
//
// Author:
//	Lawrence Pit (loz@cable.a2000.nl)
//	Sebastien Pouliot  <sebastien@ximian.com>
//
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

using System.Security;
using System.Security.Permissions;

namespace System.Net {

	[AttributeUsage (AttributeTargets.Assembly 
	               | AttributeTargets.Class 
	               | AttributeTargets.Struct 
	               | AttributeTargets.Constructor 
	               | AttributeTargets.Method, AllowMultiple = true, Inherited = false)
	]	
	[Serializable]
	public sealed class SocketPermissionAttribute : CodeAccessSecurityAttribute {

		// Fields
		string m_access;
		string m_host;
		string m_port;
		string m_transport;
		
		// Constructors
		public SocketPermissionAttribute (SecurityAction action)
			: base (action)
		{
		}

		// Properties

		public string Access {
			get { return m_access; }
			set { 
				if (m_access != null)
					AlreadySet ("Access");

				m_access = value;
			}
		}

		public string Host {
			get { return m_host; }
			set { 
				if (m_host != null)
					AlreadySet ("Host");

				m_host = value;
			}
		}

		public string Port {
			get { return m_port; }
			set { 
				if (m_port != null)
					AlreadySet ("Port");

				m_port = value;
			}
		}

		public string Transport {
			get { return m_transport; }
			set { 
				if (m_transport != null)
					AlreadySet ("Transport");

				m_transport = value;
			}
		}
		
		// Methods
		
		public override IPermission CreatePermission () 
		{
			if (this.Unrestricted)
				return new SocketPermission (PermissionState.Unrestricted);

			string missing = String.Empty;
			if (m_access == null) 
				missing += "Access, ";
			if (m_host == null) 
				missing += "Host, ";
			if (m_port == null) 
				missing += "Port, ";
			if (m_transport == null) 
				missing += "Transport, ";
			if (missing.Length > 0) {
				string msg = Locale.GetText ("The value(s) for {0} must be specified.");
				missing = missing.Substring (0, missing.Length - 2); // remove last separator
				throw new ArgumentException (String.Format (msg, missing));
			}

			NetworkAccess access;
			TransportType transport;
			int port = SocketPermission.AllPorts;

			if (String.Compare (m_access, "Connect", true) == 0)
				access = NetworkAccess.Connect;
			else if (String.Compare (m_access, "Accept", true) == 0)
				access = NetworkAccess.Accept;
			else {
				string msg = Locale.GetText ("The parameter value for 'Access', '{1}, is invalid.");
				throw new ArgumentException (String.Format (msg, m_access));
			}

			if (String.Compare (m_port, "All", true) != 0) {
				try {
					port = Int32.Parse (m_port);					
				} 
				catch {
					string msg = Locale.GetText ("The parameter value for 'Port', '{1}, is invalid.");
					throw new ArgumentException (String.Format (msg, m_port));
				}
				// test whether port number is valid..
				new IPEndPoint (1, port);
			}

			try {
				transport = (TransportType) Enum.Parse (typeof (TransportType), m_transport, true);
			}
			catch {
				string msg = Locale.GetText ("The parameter value for 'Transport', '{1}, is invalid.");
				throw new ArgumentException (String.Format (msg, m_transport));
			}
						
			SocketPermission perm = new SocketPermission (PermissionState.None);
			perm.AddPermission (access, transport, m_host, port);
			return perm;
		}

		// helpers

		internal void AlreadySet (string property)
		{
			string msg = Locale.GetText ("The parameter '{0}' can be set only once.");
			throw new ArgumentException (String.Format (msg, property), property);
		}
	}
}
