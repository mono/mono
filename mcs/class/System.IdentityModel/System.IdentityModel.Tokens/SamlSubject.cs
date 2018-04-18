//
// SamlSubject.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2006 Novell, Inc.  http://www.novell.com
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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;
using System.IdentityModel.Claims;
using System.IdentityModel.Selectors;
using System.Xml;

namespace System.IdentityModel.Tokens
{
	public class SamlSubject
	{
		public static string NameClaimType {
			get { return ClaimTypes.Name; }
		}

		bool is_readonly;
		string name_format, name_qualifier, name;
		SecurityKey crypto;
		SecurityKeyIdentifier key_identifier;
		List<string> confirmation_methods;
		string confirmation_data;

		public SamlSubject ()
		{
		}

		public SamlSubject (string nameFormat, string nameQualifier, string name)
			: this (nameFormat, nameQualifier, name, new string [0], null, null)
		{
		}

		public SamlSubject (string nameFormat, string nameQualifier, string name, IEnumerable<string> confirmations, string confirmationData, SecurityKeyIdentifier securityKeyIdentifier)
		{
			if (name == null || name.Length == 0)
				throw new ArgumentException ("non-zero length string must be specified for name of SAML Subject.");
			name_format = nameFormat;
			name_qualifier = nameQualifier;
			this.name = name;

			confirmation_methods = new List<string> (confirmations);
			confirmation_data = confirmationData;
			key_identifier = securityKeyIdentifier;
		}

		public bool IsReadOnly {
			get { return is_readonly; }
		}

		public string NameFormat {
			get { return name_format; }
			set {
				CheckReadOnly ();
				name_format = value;
			}
		}

		public string NameQualifier {
			get { return name_qualifier; }
			set {
				CheckReadOnly ();
				name_qualifier = value;
			}
		}

		public string Name {
			get { return name; }
			set {
				CheckReadOnly ();
				if (value == null || value.Length == 0)
					throw new ArgumentException ("non-zero length string must be specified for name of SAML Subject.");
				name = value;
			}
		}

		public IList<string> ConfirmationMethods {
			get { return confirmation_methods; }
		}

		public string SubjectConfirmationData {
			get { return confirmation_data; }
			set {
				CheckReadOnly ();
				confirmation_data = value;
			}
		}

		public SecurityKey Crypto {
			get { return crypto; }
			set {
				CheckReadOnly ();
				crypto = value;
			}
		}

		public SecurityKeyIdentifier KeyIdentifier {
			get { return key_identifier; }
			set {
				CheckReadOnly ();
				key_identifier = value;
			}
		}

		private void CheckReadOnly ()
		{
			if (is_readonly)
				throw new InvalidOperationException ("This SAML subject is read-only.");
		}

		public void MakeReadOnly ()
		{
			is_readonly = true;
		}

		[MonoTODO]
		public virtual ReadOnlyCollection<Claim> ExtractClaims ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual ClaimSet ExtractSubjectKeyClaimSet (
			SamlSecurityTokenAuthenticator samlAuthenticator)
		{
			throw new NotImplementedException ();
		}

		public virtual void ReadXml (XmlDictionaryReader reader,
			SamlSerializer samlSerializer,
			SecurityTokenSerializer keyInfoSerializer,
			SecurityTokenResolver outOfBandTokenResolver)
		{
			if (reader == null)
				throw new ArgumentNullException ("reader");
			if (samlSerializer == null)
				throw new ArgumentNullException ("samlSerializer");

			reader.ReadStartElement ("Subject", SamlConstants.Namespace);
			NameFormat = reader.GetAttribute ("Format");
			NameQualifier = reader.GetAttribute ("NameQualifier");
			Name = reader.ReadElementContentAsString ("NameIdentifier", SamlConstants.Namespace);
			reader.ReadEndElement ();

			if (Name == null || Name.Length == 0)
				throw new SecurityTokenException ("non-zero length string must be exist for Name.");
		}

		public virtual void WriteXml (XmlDictionaryWriter writer,
			SamlSerializer samlSerializer,
			SecurityTokenSerializer keyInfoSerializer)
		{
			if (writer == null)
				throw new ArgumentNullException ("writer");
			if (samlSerializer == null)
				throw new ArgumentNullException ("samlSerializer");

			if (Name == null || Name.Length == 0)
				throw new SecurityTokenException ("non-zero length string must be set to Name of SAML Subject before being written.");

			writer.WriteStartElement ("saml", "Subject", SamlConstants.Namespace);
			writer.WriteStartElement ("saml", "NameIdentifier", SamlConstants.Namespace);
			writer.WriteAttributeString ("Format", NameFormat);
			writer.WriteAttributeString ("NameQualifier", NameQualifier);
			writer.WriteString (Name);
			writer.WriteEndElement ();
			writer.WriteEndElement ();
		}
	}
}
