// 
// Novell.Directory.Ldap.Security.CreateContextPrivilegedAction.cs
//
// Authors:
//  Boris Kirzner <borsk@mainsoft.com>
//	Konstantin Triger <kostat@mainsoft.com>
//	
// (C) 2005 Mainsoft Corporation (http://www.mainsoft.com)
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

using java.security;
using org.ietf.jgss;

namespace Novell.Directory.Ldap.Security
{
	internal class CreateContextPrivilegedAction : PrivilegedAction
	{
		#region Fields

		private readonly bool _encryption;
		private readonly bool _signing;
		private readonly bool _delegation;
		private readonly string _name;
		private readonly string _clientName;
		private readonly string _mech;

		#endregion //Fields

		#region Constructors

		public CreateContextPrivilegedAction(string name, string clientName, string mech, bool encryption, bool signing, bool delegation)
		{
			_name = name;
			_clientName = clientName;
			_mech = mech;
			_encryption = encryption;
			_signing = signing;
			_delegation = delegation;
		}

		#endregion // Constructors

		#region Methods

		public object run()
		{
			try {				
				Oid krb5Oid = new Oid (_mech);
				GSSManager manager = GSSManager.getInstance ();
				GSSName clientName = 
					manager.createName(_clientName, GSSName__Finals.NT_USER_NAME);
				GSSCredential clientCreds =
					manager.createCredential(clientName,
					GSSContext__Finals.INDEFINITE_LIFETIME,
					krb5Oid,
					GSSCredential__Finals.INITIATE_ONLY);

//				try {
					GSSName serverName = manager.createName (_name, GSSName__Finals.NT_HOSTBASED_SERVICE, krb5Oid);
					GSSContext context = manager.createContext (serverName, krb5Oid, clientCreds, GSSContext__Finals.INDEFINITE_LIFETIME);

					context.requestMutualAuth(true);  
					context.requestConf (_encryption);
					if (!_encryption || _signing)
						context.requestInteg (!_encryption || _signing); 
					context.requestCredDeleg (_delegation);

					return context;
//				}
//				finally {
//					// Calling this throws GSSException: Operation unavailable...
//					clientCreds.dispose();
//				}
			}
			catch (GSSException e) {
				throw new PrivilegedActionException (e);
			}
		}

		#endregion // Methods
	}
}
