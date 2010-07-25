// 
// Novell.Directory.Ldap.Security.Krb5Helper.cs
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
using vmw.common;

using java.security;
using javax.security.auth;
using org.ietf.jgss;


namespace Novell.Directory.Ldap.Security
{
	internal class Krb5Helper : IDisposable
	{
		enum QOP {
			NO_PROTECTION = 1,
			INTEGRITY_ONLY_PROTECTION = 2,
			PRIVACY_PROTECTION = 4
		}

		#region Fields

		internal static readonly sbyte [] EmptyToken = new sbyte [0];
		     
		private readonly bool _encryption;
		private readonly bool _signing;
		private readonly bool _delegation;

		private readonly GSSContext _context;

		#endregion // Fields

		#region Constructors

		public Krb5Helper(string name, string clientName, Subject subject, AuthenticationTypes authenticationTypes, string mech)
		{
			_encryption = (authenticationTypes & AuthenticationTypes.Sealing) != 0;
			_signing = (authenticationTypes & AuthenticationTypes.Signing) != 0;
			_delegation = (authenticationTypes & AuthenticationTypes.Delegation) != 0;

			CreateContextPrivilegedAction action = new CreateContextPrivilegedAction (name, clientName, mech,_encryption,_signing,_delegation);
			try {
				_context = (GSSContext) Subject.doAs (subject,action);
			}
			catch (PrivilegedActionException e) {
				throw new LdapException ("Problem performing token exchange with the server",LdapException.OTHER,"",e.getCause()); 
			}
		}

		#endregion // Constructors

		#region Properties

		internal GSSContext Context
		{
			get { return _context; }
		}

		#endregion // Properties

		#region Methods

		public sbyte [] ExchangeTokens(sbyte [] clientToken)
		{
			if (Context.isEstablished ()) {
				if (clientToken == null || clientToken.Length == 0)
					return Krb5Helper.EmptyToken;

				//final handshake
				byte [] challengeData = (byte []) TypeUtils.ToByteArray (clientToken);
				byte [] gssOutToken = Unwrap (challengeData, 0, challengeData.Length, new MessageProp (false));

				QOP myCop = QOP.NO_PROTECTION;

				if (_encryption)
					myCop = QOP.PRIVACY_PROTECTION;
				else if (_signing || (((QOP)gssOutToken [0] & QOP.INTEGRITY_ONLY_PROTECTION) != 0))
					myCop = QOP.INTEGRITY_ONLY_PROTECTION;

				if ((myCop & (QOP)gssOutToken [0]) == 0)
					throw new LdapException ("Server does not support the requested security level", 80, "");

				int srvMaxBufSize = SecureStream.NetworkByteOrderToInt (gssOutToken, 1, 3);

				//int rawSendSize = Context.getWrapSizeLimit(0, _encryption, srvMaxBufSize);

				byte [] gssInToken = new byte [4];
				gssInToken [0] = (byte) myCop;

				SecureStream.IntToNetworkByteOrder (srvMaxBufSize, gssInToken, 1, 3);

				gssOutToken = Wrap (gssInToken, 0, gssInToken.Length, new MessageProp (true));

				return TypeUtils.ToSByteArray (gssOutToken);
			}

			sbyte [] token = Context.initSecContext (clientToken, 0, clientToken.Length);

			if (Context.isEstablished ()) {
				
				if (Context.getConfState () != _encryption)
					throw new LdapException ("Encryption protocol was not established layer between client and server", 80, "");
					
				if (Context.getCredDelegState () != _delegation) 
					throw new LdapException ("Credential delegation was not established layer between client and server", 80, "");
					
				if (_signing && (Context.getIntegState () != _signing))
					throw new LdapException ("Signing protocol was not established layer between client and server", 80, "");
					
				if (token == null) 
					return EmptyToken;
			}
			return token;
		}

		public byte [] Wrap(byte [] outgoing, int start, int len) 
		{
			return Wrap (outgoing, start, len, new MessageProp(true));
		}

		public byte [] Wrap(byte [] outgoing, int start, int len, MessageProp messageProp)
		{
			if (!Context.isEstablished ())
				throw new LdapException ("GSSAPI authentication not completed",LdapException.OTHER,"");

			if (!(Context.getConfState () || Context.getIntegState ())) {
				// in the case no encryption and no integrity required - return the original data
				byte [] buff = new byte [len];
				Array.Copy (outgoing, start, buff, 0, len);
				return buff;
			}

			sbyte [] result = Context.wrap (TypeUtils.ToSByteArray (outgoing), start, len, messageProp);
			return (byte []) TypeUtils.ToByteArray (result);
		}

		public byte [] Unwrap(byte [] incoming, int start, int len) 
		{
			return Unwrap (incoming, start, len, new MessageProp(true));
		}

		public byte [] Unwrap(byte [] incoming, int start, int len, MessageProp messageProp)
		{
			if (!Context.isEstablished ())
				throw new LdapException ("GSSAPI authentication not completed",LdapException.OTHER,"");

			if (!(Context.getConfState () || Context.getIntegState ())) {
				// in the case no encryption and no integrity required - return the original data
				byte [] buff = new byte [len];
				Array.Copy (incoming, start, buff, 0, len);
				return buff;
			}

			sbyte [] result = Context.unwrap (TypeUtils.ToSByteArray (incoming), start, len, messageProp);
			return (byte []) TypeUtils.ToByteArray (result);
		}

		#endregion // Methods

		#region IDisposable Members

		public void Dispose() {
			Context.dispose();
		}

		#endregion
	}
}
