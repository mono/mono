//
// SamlAttribute.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2005-2006 Novell, Inc.  http://www.novell.com
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
using System.Collections.ObjectModel;
using System.Xml;
using System.IdentityModel.Claims;
using System.IdentityModel.Selectors;

namespace System.IdentityModel.Tokens
{
	public class SamlAttribute
	{
		bool is_readonly;
		string name, ns;
		List<string> attribute_values;

		public SamlAttribute ()
		{
			attribute_values = new List<string> ();
		}

		public SamlAttribute (Claim claim)
		{
			if (claim == null)
				throw new ArgumentNullException ("claim");
			if (claim.ClaimType == null)
				throw new ArgumentException ("Claim type is null.");
			int idx = claim.ClaimType.LastIndexOf ('/');
			if (idx <= 0 || idx == claim.ClaimType.Length - 1)
				throw new ArgumentException ("Claim type does not contain '/' or it is at improper position.");
			name = claim.ClaimType.Substring (idx + 1);
			ns = claim.ClaimType.Substring (0, idx);

			if (claim.Resource != null && !(claim.Resource is string))
				throw new ArgumentException ("Claim resource is not a string.");

			attribute_values = new List<string> ();
			attribute_values.Add ((string) claim.Resource);

			if (claim.Right != Rights.PossessProperty)
				throw new ArgumentException ("Claim right is not PossessProperty");
		}

		public SamlAttribute (string attributeNamespace,
			string attributeName,
			IEnumerable<string> attributeValues)
		{
			ns = attributeNamespace;
			name = attributeName;
			attribute_values = new List<string> (attributeValues);
		}

		public IList<string> AttributeValues {
			get { return attribute_values; }
		}

		public string Name {
			get { return name; }
			set {
				CheckReadOnly ();
				name = value;
			}
		}

		public string Namespace {
			get { return ns; }
			set {
				CheckReadOnly ();
				ns = value;
			}
		}

		public bool IsReadOnly {
			get { return is_readonly; }
		}

		private void CheckReadOnly ()
		{
			if (is_readonly)
				throw new InvalidOperationException ("This SAML assertion is read-only.");
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

		public virtual void WriteXml (XmlDictionaryWriter writer,
			SamlSerializer samlSerializer,
			SecurityTokenSerializer keyInfoSerializer)
		{
			writer.WriteStartElement ("saml", "Attribute", SamlConstants.Namespace);
			writer.WriteAttributeString ("AttributeName", Name);
			writer.WriteAttributeString ("AttributeNamespace", Namespace);
			foreach (string s in AttributeValues)
				writer.WriteElementString ("saml", "AttributeValue", SamlConstants.Namespace, s);
			writer.WriteEndElement ();
		}

		[MonoTODO]
		public virtual ReadOnlyCollection<Claim> ExtractClaims ()
		{
			throw new NotImplementedException ();
		}
	}
}
