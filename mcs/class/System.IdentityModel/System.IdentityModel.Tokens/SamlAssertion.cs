//
// SamlAssertion.cs
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
using System.Globalization;
using System.Xml;
using System.IdentityModel.Selectors;

namespace System.IdentityModel.Tokens
{
	public class SamlAssertion
	{
		bool is_readonly;
		SamlAdvice advice;
		SamlConditions conditions;
		string assertion_id, issuer;
		DateTime issue_instant;
		int major, minor;
		SigningCredentials signing_credentials;
		List<SamlStatement> statements = new List<SamlStatement> ();

		public SamlAssertion ()
		{
			assertion_id = "SamlSecurityToken-" + Guid.NewGuid ();
			major = 1;
			minor = 1;
			issue_instant = DateTime.Now.ToUniversalTime ();
		}

		public SamlAssertion (string assertionId, string issuer,
			DateTime issueInstant, SamlConditions samlConditions,
			SamlAdvice samlAdvice, IEnumerable<SamlStatement> samlStatements)
		{
			if (IsInvalidAssertionId (assertionId))
				throw new ArgumentException (String.Format ("The assertionId '{0}' must be a valid XML NCName.", assertionId));

			if (issuer == null || issuer.Length == 0)
				throw new ArgumentException ("issuer");
			if (samlStatements == null)
				throw new ArgumentNullException ("samlStatements");

			major = 1;
			minor = 1;

			assertion_id = assertionId;
			this.issuer = issuer;
			issue_instant = issueInstant;
			this.conditions = samlConditions;
			this.advice = samlAdvice;
			foreach (SamlStatement s in samlStatements) {
				if (s == null)
					throw new ArgumentException ("statements contain null item.");
				this.statements.Add (s);
			}
			if (this.statements.Count == 0)
				throw new ArgumentException ("At least one assertion statement is required.");
		}

		bool IsInvalidAssertionId (string assertionId)
		{
			if (assertionId == null || assertionId.Length == 0)
				return true;
			try {
				XmlConvert.VerifyNCName (assertionId);
			} catch (XmlException) {
				return true;
			}
			return false;
		}

		public SamlAdvice Advice {
			get { return advice; }
			set {
				CheckReadOnly ();
				advice = value;
			}
		}

		public string AssertionId {
			get { return assertion_id; }
			set {
				CheckReadOnly ();
				assertion_id = value;
			}
		}

		public SamlConditions Conditions {
			get { return conditions; }
			set {
				CheckReadOnly ();
				conditions = value;
			}
		}

		public DateTime IssueInstant {
			get { return issue_instant; }
			set {
				CheckReadOnly ();
				issue_instant = value;
			}
		}

		public string Issuer {
			get { return issuer; }
			set {
				CheckReadOnly ();
				issuer = value;
			}
		}

		public int MajorVersion {
			get { return major; }
		}

		public int MinorVersion {
			get { return minor; }
		}

		public SigningCredentials SigningCredentials {
			get { return signing_credentials; }
			set {
				CheckReadOnly ();
				signing_credentials = value;
			}
		}

		[MonoTODO]
		public SecurityToken SigningToken {
			get {
				if (signing_credentials == null)
					return null;
				throw new NotImplementedException ();
			}
		}

		public IList<SamlStatement> Statements {
			get { return statements; }
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
			if (writer == null)
				throw new ArgumentNullException ("writer");

			if (Issuer == null || Issuer.Length == 0)
				throw new SecurityTokenException ("Issuer must not be null or empty.");
			if (Statements.Count == 0)
				throw new SecurityTokenException ("At least one assertion statement is required.");

			if (samlSerializer == null)
				throw new ArgumentNullException ("samlSerializer");
			CultureInfo invariant = CultureInfo.InvariantCulture;

			writer.WriteStartElement ("saml", "Assertion", SamlConstants.Namespace);
			writer.WriteAttributeString ("MajorVersion", MajorVersion.ToString (invariant));
			writer.WriteAttributeString ("MinorVersion", MinorVersion.ToString (invariant));
			writer.WriteAttributeString ("AssertionID", AssertionId);
			writer.WriteAttributeString ("Issuer", Issuer);
			writer.WriteAttributeString ("IssueInstant", IssueInstant.ToString (SamlConstants.DateFormat, invariant));

			try {
				if (Conditions != null)
					Conditions.WriteXml (writer, samlSerializer, keyInfoSerializer);
				if (Advice != null)
					Advice.WriteXml (writer, samlSerializer, keyInfoSerializer);
				foreach (SamlStatement statement in Statements)
					statement.WriteXml (writer, samlSerializer, keyInfoSerializer);
			} catch (NotImplementedException) {
				throw;
			} catch (Exception ex) { // bad catch, eh?
				throw new InvalidOperationException ("There is an error on writing assertion statements.", ex);
			}
			writer.WriteEndElement ();
		}

		[MonoTODO]
		protected void ReadSignature (XmlDictionaryReader reader,
			SecurityTokenSerializer keyInfoSerializer,
			SecurityTokenResolver outOfBandTokenResolver,
			SamlSerializer samlSerializer)
		{
			throw new NotImplementedException ();
		}
	}
}
