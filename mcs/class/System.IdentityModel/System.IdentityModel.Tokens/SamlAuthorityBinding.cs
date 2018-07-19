//
// SamlAuthorityBinding.cs
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
using System;
using System.Collections.Generic;
using System.Xml;
using System.Runtime.Serialization;
using System.IdentityModel.Claims;
using System.IdentityModel.Policy;
using System.IdentityModel.Selectors;

namespace System.IdentityModel.Tokens
{
	[DataContract]
	public class SamlAuthorityBinding
	{
		XmlQualifiedName kind;
		string binding, location;
		bool is_readonly;

		public SamlAuthorityBinding ()
		{
		}

		public SamlAuthorityBinding (XmlQualifiedName authorityKind, string binding, string location)
		{
			if (authorityKind == null)
				throw new ArgumentNullException ("authorityKind");
			AuthorityKind = authorityKind;
			Binding = binding;
			Location = location;
		}

		[DataMember]
		public XmlQualifiedName AuthorityKind {
			get { return kind; }
			set {
				CheckReadOnly ();
				if (value == null)
					throw new ArgumentNullException ("value");
				if (value.Equals (XmlQualifiedName.Empty))
					throw new ArgumentException ("non-empty XmlQualifiedName must be set to AuthorityKind of SamlAuthorityBinding.");
				kind = value;
			}
		}

		[DataMember]
		public string Binding {
			get { return binding; }
			set {
				CheckReadOnly ();
				if (value == null || value.Length == 0)
					throw new ArgumentException ("non-zero length string must be set to Binding of SamlAuthorityBinding.");
				binding = value;
			}
		}

		[DataMember]
		public string Location {
			get { return location; }
			set {
				CheckReadOnly ();
				if (value == null || value.Length == 0)
					throw new ArgumentException ("non-zero length string must be set to Location of SamlAuthorityBinding.");
				location = value;
			}
		}

		public bool IsReadOnly {
			get { return is_readonly; }
		}

		void CheckReadOnly ()
		{
			if (IsReadOnly)
				throw new InvalidOperationException ("This object is read-only.");
		}

		public void MakeReadOnly ()
		{
			is_readonly = true;
		}

		[MonoTODO]
		public virtual void ReadXml (XmlDictionaryReader reader,
			SamlSerializer samlSerializer,
			SecurityTokenSerializer keyInfoSerializer,
			SecurityTokenResolver outOfBandTokenResolver)
		{
			throw new NotImplementedException ();
		}

		public virtual void WriteXml (
			XmlDictionaryWriter writer,
			SamlSerializer samlSerializer,
			SecurityTokenSerializer keyInfoSerializer)
		{
			if (writer == null)
				throw new ArgumentNullException ("writer");
			if (samlSerializer == null)
				throw new ArgumentNullException ("samlSerializer");

			if (AuthorityKind == null)
				throw new SecurityTokenException ("AuthorityKind must be set to SAML AuthorityBinding before being written.");
			if (Binding == null)
				throw new SecurityTokenException ("non-zero length Binding must be set to SAML AuthorityBinding before being written.");
			if (Location == null)
				throw new SecurityTokenException ("non-zero length Location must be set to SAML AuthorityBinding before being written.");

			writer.WriteStartElement ("saml", "AuthorityBinding", SamlConstants.Namespace);
			writer.WriteXmlnsAttribute (String.Empty, AuthorityKind.Namespace);
			writer.WriteAttributeString ("AuthorityKind", AuthorityKind.Name);
			writer.WriteAttributeString ("Location", Location);
			writer.WriteAttributeString ("Binding", Binding);
			writer.WriteEndElement ();
		}
	}
}
