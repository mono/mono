//
// SamlEvidence.cs
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
using System.IdentityModel.Claims;
using System.IdentityModel.Policy;
using System.IdentityModel.Selectors;

namespace System.IdentityModel.Tokens
{
	public class SamlEvidence
	{
		public SamlEvidence ()
		{
		}

		public SamlEvidence (IEnumerable<string> assertionIdReferences)
			: this (assertionIdReferences, new SamlAssertion [0])
		{
		}

		public SamlEvidence (IEnumerable<SamlAssertion> assertions)
			: this (new string [0], assertions)
		{
		}

		public SamlEvidence (
			IEnumerable<string> assertionIdReferences,
			IEnumerable<SamlAssertion> assertions)
		{
			if (assertionIdReferences == null)
				throw new ArgumentException ("assertionIdReferences are null.");
			if (assertions == null)
				throw new ArgumentException ("assertions are null.");
			foreach (string r in assertionIdReferences) {
				if (r == null)
					throw new ArgumentException ("assertionIdReferences contain null item.");
				references.Add (r);
			}
			foreach (SamlAssertion a in assertions) {
				if (a == null)
					throw new ArgumentException ("assertions contain null item.");
				this.assertions.Add (a);
			}
		}

		bool is_readonly;
		List<string> references = new List<string> ();
		List<SamlAssertion> assertions = new List<SamlAssertion> ();

		public IList<string> AssertionIdReferences {
			get { return references; }
		}

		public IList<SamlAssertion> Assertions {
			get { return assertions; }
		}

		public bool IsReadOnly {
			get { return is_readonly; }
		}

		private void CheckReadOnly ()
		{
			if (IsReadOnly)
				throw new InvalidOperationException ("This SAML assertion is read-only.");
		}

		public void MakeReadOnly ()
		{
			is_readonly = true;
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
			references.Clear ();
			assertions.Clear ();

			reader.ReadStartElement ("Evidence", SamlConstants.Namespace);
			for (reader.MoveToContent ();
			     reader.NodeType == XmlNodeType.Element;
			     reader.MoveToContent ()) {
				if (reader.NamespaceURI != SamlConstants.Namespace)
					throw new SecurityTokenException (String.Format ("Invalid SAML Evidence element: element '{0}' in namespace '{1}' is unexpected.", reader.LocalName, reader.NamespaceURI));
				switch (reader.LocalName) {
				case "Assertion":
					SamlAssertion a = new SamlAssertion ();
					a.ReadXml (reader, samlSerializer, keyInfoSerializer, outOfBandTokenResolver);
					assertions.Add (a);
					break;
				case "AssertionIDReference":
					references.Add (reader.ReadElementContentAsString ());
					break;
				default:
					throw new SecurityTokenException (String.Format ("Invalid SAML Evidence element: SAML element '{0}' is unexpected.", reader.LocalName));
				}
			}
			reader.ReadEndElement ();

			if (references.Count == 0 && assertions.Count == 0)
				throw new SecurityTokenException ("At least either one of AssertionIDReference or Assertion must exist in SAML Evidence.");
		}

		public virtual void WriteXml (XmlDictionaryWriter writer,
			SamlSerializer samlSerializer, 
			SecurityTokenSerializer keyInfoSerializer)
		{
			if (writer == null)
				throw new ArgumentNullException ("writer");
			if (samlSerializer == null)
				throw new ArgumentNullException ("samlSerializer");
			if (references.Count == 0 && assertions.Count == 0)
				throw new SecurityTokenException ("At least either one of AssertionIDReference or Assertion must exist in SAML Evidence.");

			writer.WriteStartElement ("saml", "Evidence", SamlConstants.Namespace);
			foreach (string s in references)
				writer.WriteElementString ("saml", "AssertionIDReference", SamlConstants.Namespace, s);
			foreach (SamlAssertion a in assertions)
				a.WriteXml (writer, samlSerializer, keyInfoSerializer);
			writer.WriteEndElement ();
		}
	}
}
