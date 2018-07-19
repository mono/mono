//
// SamlAttributeStatement.cs
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
using System.Xml;
using System.IdentityModel.Claims;
using System.IdentityModel.Selectors;

namespace System.IdentityModel.Tokens
{
	public class SamlAttributeStatement : SamlSubjectStatement
	{
		bool is_readonly;
		List<SamlAttribute> attributes;

		public SamlAttributeStatement ()
		{
			attributes = new List<SamlAttribute> ();
		}

		public SamlAttributeStatement (SamlSubject samlSubject,
			IEnumerable<SamlAttribute> attributes)
			: base (samlSubject)
		{
			this.attributes = new List<SamlAttribute> (attributes);
		}

		public IList<SamlAttribute> Attributes {
			get { return attributes; }
		}

		public override bool IsReadOnly {
			get { return is_readonly; }
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
			if (SamlSubject == null)
				throw new SecurityTokenException ("Subject is null in the AttributeStatement");
			writer.WriteStartElement ("saml", "AttributeStatement", SamlConstants.Namespace);
			SamlSubject.WriteXml (writer, samlSerializer, keyInfoSerializer);
			foreach (SamlAttribute a in Attributes)
				a.WriteXml (writer, samlSerializer, keyInfoSerializer);
			writer.WriteEndElement ();
		}

		[MonoTODO]
		protected override void AddClaimsToList (IList<Claim> claims)
		{
			throw new NotImplementedException ();
		}
	}
}
