//
// SamlAuthenticationStatement.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2005 Novell, Inc.  http://www.novell.com
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
using System.Collections.Generic;
using System.Globalization;
using System.Xml;
using System.IdentityModel.Claims;
using System.IdentityModel.Selectors;

namespace System.IdentityModel.Tokens
{
	public class SamlAuthenticationStatement : SamlSubjectStatement
	{
		public static string ClaimType {
			get { return "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/authentication"; }
		}

		bool is_readonly;
		string auth_method = "urn:oasis:names:tc:SAML:1.0:am:unspecified";
		string dns, ip;
		new IList<SamlAuthorityBinding> bindings;
		DateTime instant;

		public SamlAuthenticationStatement ()
		{
			bindings = new List<SamlAuthorityBinding> ();
		}

		public SamlAuthenticationStatement (SamlSubject samlSubject,
			string authenticationMethod,
			DateTime authenticationInstant,
			string dnsAddress, string ipAddress,
			IEnumerable<SamlAuthorityBinding> authorityBindings)
			: base (samlSubject)
		{
			AuthenticationMethod = authenticationMethod;
			instant = authenticationInstant;
			dns = dnsAddress;
			ip = ipAddress;
			if (authorityBindings != null)
				bindings = new List<SamlAuthorityBinding> (authorityBindings);
			else
				bindings = new List<SamlAuthorityBinding> ();
		}

		public DateTime AuthenticationInstant {
			get { return instant; }
			set {
				CheckReadOnly ();
				instant = value;
			}
		}

		public string AuthenticationMethod {
			get { return auth_method; }
			set {
				CheckReadOnly ();
				if (value == null || value.Length == 0)
					throw new ArgumentException ("Authentication method must be non-zero length string.");
				auth_method = value;
			}
		}

		public IList<SamlAuthorityBinding> AuthorityBindings {
			get { return bindings; }
		}

		public string DnsAddress {
			get { return dns; }
			set {
				CheckReadOnly ();
				dns = value;
			}
		}

		public string IPAddress {
			get { return ip; }
			set {
				CheckReadOnly ();
				ip= value;
			}
		}

		public override bool IsReadOnly {
			get { return is_readonly; }
		}

		private void CheckReadOnly ()
		{
			if (is_readonly)
				throw new InvalidOperationException ("This SAML assertion is read-only.");
		}

		public override void MakeReadOnly ()
		{
			is_readonly = true;
		}

		[MonoTODO]
		public override void ReadXml (XmlDictionaryReader reader,
			SamlSerializer samlSerializer,
			SecurityTokenSerializer keyInfoSerializer,
			SecurityTokenResolver outOfBandTokenResolver)
		{
			throw new NotImplementedException ();
		}

		public override void WriteXml (XmlDictionaryWriter writer,
			SamlSerializer samlSerializer,
			SecurityTokenSerializer keyInfoSerializer)
		{
			if (writer == null)
				throw new ArgumentNullException ("writer");
			if (samlSerializer == null)
				throw new ArgumentNullException ("samlSerializer");
			if (SamlSubject == null)
				throw new SecurityTokenException ("SAML Subject must be set to AuthenticationStatement before it is written.");

			writer.WriteStartElement ("saml", "AuthenticationStatement", SamlConstants.Namespace);
			writer.WriteAttributeString ("AuthenticationMethod", AuthenticationMethod);
			writer.WriteAttributeString ("AuthenticationInstant", 
				AuthenticationInstant.ToString (SamlConstants.DateFormat, CultureInfo.InvariantCulture));
			SamlSubject.WriteXml (writer, samlSerializer, keyInfoSerializer);
			if (DnsAddress != null || IPAddress != null) {
				writer.WriteStartElement ("saml", "SubjectLocality", SamlConstants.Namespace);
				if (IPAddress != null)
					writer.WriteAttributeString ("IPAddress", IPAddress);
				if (DnsAddress != null)
					writer.WriteAttributeString ("DNSAddress", DnsAddress);
				writer.WriteEndElement ();
			}
			writer.WriteEndElement ();
		}

		[MonoTODO]
		protected override void AddClaimsToList (IList<Claim> claims)
		{
			throw new NotImplementedException ();
		}
	}
}
