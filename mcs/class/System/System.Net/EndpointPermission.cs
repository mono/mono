//
// System.Net.EndpointPermission.cs
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

using System;
using System.Collections;
using System.Security;
using System.Security.Permissions;

namespace System.Net
{
	[Serializable]
	public class EndpointPermission // too bad about the lowercase p, not consistent with IPEndPoint ;)
	{
		private static char [] dot_char = new char [] { '.' };
		
		// Fields
		private string hostname;
		private int port;
		private TransportType transport;

		private bool resolved;		
		private bool hasWildcard;
		private IPAddress [] addresses;
		
		// Constructors
		internal EndpointPermission (string hostname, 
				   	     int port, 
				   	     TransportType transport) : base () 
		{			
			if (hostname == null)
				throw new ArgumentNullException ("hostname");
			this.hostname = hostname;
			this.port = port;
			this.transport = transport;
			this.resolved = false;
			this.hasWildcard = false;
			this.addresses = null;
		}		
		
		// Properties

		public string Hostname {
			get { return hostname; }
		}

		public int Port {
			get { return port; }
		}
		
		public TransportType Transport {
			get { return transport; }
		}
		
		// Methods
		
		public override bool Equals (object obj) 
		{
			EndpointPermission epp = obj as EndpointPermission;
			return ((epp != null) &&
			        (this.port == epp.port) &&
			        (this.transport == epp.transport) &&
			        (String.Compare (this.hostname, epp.hostname, true) == 0));
		}
		
		public override int GetHashCode () 
		{
			return ToString ().GetHashCode ();
		}
		
		public override string ToString () 
		{
			return hostname + "#" + port + "#" + (int) transport;
		}
		
		// Internal & Private Methods
		
		internal bool IsSubsetOf (EndpointPermission perm) 
		{
			if (perm == null)
				return false;
			
			if (perm.port != SocketPermission.AllPorts &&
			    this.port != perm.port)
			    	return false;
			
			if (perm.transport != TransportType.All &&
			    this.transport != perm.transport)
				return false;
			
			this.Resolve ();
			perm.Resolve ();
			
			if (this.hasWildcard) {
				if (perm.hasWildcard)
					return IsSubsetOf (this.hostname, perm.hostname);
				else 
					return false;
			} 
			
			if (this.addresses == null) 
				return false;
			
			if (perm.hasWildcard) 
				// a bit dubious... should they all be a subset or is one 
				// enough in this case?
				foreach (IPAddress addr in this.addresses)
					if (IsSubsetOf (addr.ToString (), perm.hostname))
						return true;
				
			if (perm.addresses == null) 
				return false;
				
			// a bit dubious... should they all be a subset or is one 
			// enough in this case?
			foreach (IPAddress addr in perm.addresses)
				if (IsSubsetOf (this.hostname, addr.ToString ())) 
					return true;	
					
			return false;		
		}		
		
		private bool IsSubsetOf (string addr1, string addr2)
		{
			string [] h1 = addr1.Split (dot_char);		
			string [] h2 = addr2.Split (dot_char);
				
			for (int i = 0; i < 4; i++) {
				int part1 = ToNumber (h1 [i]);
				if (part1 == -1) 
					return false;				

				int part2 = ToNumber (h2 [i]);
				if (part2 == -1)
					return false;				
				if (part1 == 256)
					continue;
				if (part1 != part2 && part2 != 256)
					return false;
			}
			return true;
		}
		
		internal EndpointPermission Intersect (EndpointPermission perm) 
		{
			if (perm == null)
				return null;
			
			int _port;
			if (this.port == perm.port)
				_port = this.port;
			else if (this.port == SocketPermission.AllPorts)
				_port = perm.port;
			else if (perm.port == SocketPermission.AllPorts)
				_port = this.port;
			else
				return null;

			TransportType _transport;
			if (this.transport == perm.transport)
				_transport = this.transport;
			else if (this.transport == TransportType.All)
				_transport = perm.transport;
			else if (perm.transport == TransportType.All)
				_transport = this.transport;
			else
				return null;

			string _hostname = IntersectHostname (perm);						
			
			if (_hostname == null)
				return null;

			if (!this.hasWildcard)
				return this;
				
			if (!perm.hasWildcard)
				return perm;
				
			EndpointPermission newperm = new EndpointPermission (_hostname, _port, _transport);
			newperm.hasWildcard = true;
			newperm.resolved = true;
			return newperm;
		}
		
		private string IntersectHostname (EndpointPermission perm)
		{
			if (this.hostname == perm.hostname)
				return this.hostname;
				
			this.Resolve ();
			perm.Resolve ();
			
			string _hostname = null;
			
			if (this.hasWildcard) {
				if (perm.hasWildcard) {
					_hostname = Intersect (this.hostname, perm.hostname);
				} else if (perm.addresses != null) {
					for (int j = 0; j < perm.addresses.Length; j++) {
						_hostname = Intersect (this.hostname, perm.addresses [j].ToString ());
						if (_hostname != null) 
							break;
					}
				}
			} else if (this.addresses != null) {
				for (int i = 0; i < this.addresses.Length; i++) {
					string thisaddr = this.addresses [i].ToString ();
					if (perm.hasWildcard) {
						_hostname = Intersect (thisaddr, perm.hostname);
					} else if (perm.addresses != null) {
						for (int j = 0; j < perm.addresses.Length; j++) {
							_hostname = Intersect (thisaddr, perm.addresses [j].ToString ());
							if (_hostname != null) 
								break;
						}
					}
				}
			}
			
			return _hostname;
		}
		
		// alas, currently we'll only support IPv4 as that's MS.Net behaviour
		// returns null when both host strings do not intersect
		private string Intersect (string addr1, string addr2)
		{
			string [] h1 = addr1.Split (dot_char);		
			string [] h2 = addr2.Split (dot_char);
				
			string [] s = new string [7];
			for (int i = 0; i < 4; i++) {
				int part1 = ToNumber (h1 [i]);
				if (part1 == -1) 
					return null;				

				int part2 = ToNumber (h2 [i]);
				if (part2 == -1)
					return null;				

				if (part1 == 256) 
					s [i << 1] = (part2 == 256) ? "*" : String.Empty + part2;
				else if (part2 == 256)
					s [i << 1] = (part1 == 256) ? "*" : String.Empty + part1;				
				else if (part1 == part2)
					s [i << 1] = String.Empty + part1;
				else
					return null;
			}
			
			s [1] = s [3] = s [5] = ".";
			return String.Concat (s);
		}
		
		// returns 256 if value is a '*' character
		// returns -1 if value isn't a number between 0 and 255		
		private int ToNumber (string value)
		{
			if (value == "*")
				return 256;
				
			int len = value.Length;
			if (len < 1 || len > 3)
				return -1;
				
			int val = 0;				
			for (int i = 0; i < len; i++) {
				char c = value [i];
				if ('0' <= c && c <= '9') 
					val = checked (val * 10 + (c - '0'));
				else
					return -1;
			}
			
			return val <= 255 ? val : -1;
		}

		internal void Resolve ()
		{
			if (resolved) 	
				return;
				
			bool isHostname = false;				
			bool hasWildcard = false;
			this.addresses = null;
			
			string [] s = hostname.Split (dot_char);

			if (s.Length != 4) {
				isHostname = true;
			} else {
				for (int i = 0; i < 4; i++) {
					int quad = ToNumber (s [i]);
					if (quad == -1) {
						isHostname = true;
						break;
					}
					if (quad == 256)
						hasWildcard = true;
				}
			}
			
			if (isHostname) {
				this.hasWildcard = false;
				try {
					this.addresses = Dns.GetHostAddresses (hostname);
				} catch (System.Net.Sockets.SocketException) {					
				}
			} else {
				this.hasWildcard = hasWildcard;
				if (!hasWildcard) {
					addresses = new IPAddress [1];
					addresses [0] = IPAddress.Parse (hostname);
				}
			}
			
			this.resolved = true;				
		}
		
		internal void UndoResolve ()
		{
			resolved = false;
		}
	}
}
