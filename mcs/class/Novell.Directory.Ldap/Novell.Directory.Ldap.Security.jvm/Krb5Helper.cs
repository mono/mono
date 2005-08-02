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
	internal class Krb5Helper
	{
		#region Fields

		internal static readonly sbyte [] EmptyToken = new sbyte [0];
		     
		private readonly bool _encryption;
		private readonly bool _signing;
		private readonly bool _delegation;

		private readonly GSSContext _context;
		private readonly MessageProp _messageProperties; 

		private readonly string _name;
		private readonly Subject _subject;
		private readonly string _mech;

		#endregion // Fields

		#region Constructors

		public Krb5Helper(string name, Subject subject, AuthenticationTypes authenticationTypes, string mech)
		{
			_name = name;
			_subject = subject;
			_mech = mech;

			_encryption = (authenticationTypes & AuthenticationTypes.Sealing) != 0;
			_signing = (authenticationTypes & AuthenticationTypes.Signing) != 0;
			_delegation = (authenticationTypes & AuthenticationTypes.Delegation) != 0;

			CreateContextPrivilegedAction action = new CreateContextPrivilegedAction (_name,_mech,_encryption,_signing,_delegation);
			_context = (GSSContext) Subject.doAs (_subject,action);

			// 0 is a default JGSS QoP
			_messageProperties = new MessageProp (0, _encryption);
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
			if (Context.isEstablished ())
				return Krb5Helper.EmptyToken;

			sbyte [] token;
			try {
				ExchangeTokenPrivilegedAction action = new ExchangeTokenPrivilegedAction (Context, clientToken);
				token = (sbyte []) Subject.doAs (_subject, action);
			} 
			catch (PrivilegedActionException e) {
				throw new LdapException ("Problem performing token exchange with the server",LdapException.OTHER,"",e); 
			}

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
			if (!Context.isEstablished ())
				throw new LdapException ("GSSAPI authentication not completed",LdapException.OTHER,"");

			try {
				WrapPrivilegedAction action = new WrapPrivilegedAction (Context, outgoing, start, len, _messageProperties);
				return (byte []) Subject.doAs (_subject, action);				
			} 
			catch (PrivilegedActionException e) {
				throw new LdapException ("Problem performing GSS wrap",LdapException.OTHER,"",e); 
			}
		}

		public byte [] Unwrap(byte [] incoming, int start, int len)
		{
			if (!Context.isEstablished ())
				throw new LdapException ("GSSAPI authentication not completed",LdapException.OTHER,"");

			try {
				UnwrapPrivilegedAction action = new UnwrapPrivilegedAction (Context, incoming, start, len, _messageProperties);
				return (byte []) Subject.doAs (_subject, action);
			} 
			catch (PrivilegedActionException e) {
				throw new LdapException("Problems unwrapping SASL buffer",LdapException.OTHER,"",e);
			}
		}

		#endregion // Methods
	}
}
