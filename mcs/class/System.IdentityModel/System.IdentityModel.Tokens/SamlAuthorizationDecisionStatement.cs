//
// SamlAuthorizationDecisionStatement.cs
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
using System.IdentityModel.Policy;
using System.IdentityModel.Selectors;

namespace System.IdentityModel.Tokens
{
	public class SamlAuthorizationDecisionStatement : SamlSubjectStatement
	{
		public static string ClaimType {
			get { return "http://schemas.microsoft.com/mb/2005/09/ClaimType/SamlAuthorizationDecision"; }
		}

		public SamlAuthorizationDecisionStatement ()
		{
		}

		public SamlAuthorizationDecisionStatement (
			SamlSubject samlSubject, string resource,
			SamlAccessDecision accessDecision,
			IEnumerable<SamlAction> samlActions)
			: base (samlSubject)
		{
			if (samlActions == null)
				throw new ArgumentNullException ("samlActions");
			if (resource == null || resource.Length == 0)
				throw new SecurityTokenException ("non-zero length string must be set to Resource of SAML AuthorizationDecisionStatement.");
			Resource = resource;
			AccessDecision = accessDecision;
			foreach (SamlAction a in samlActions) {
				if (a == null)
					throw new ArgumentException ("samlActions contain null item.");
				actions.Add (a);
			}
		}

		public SamlAuthorizationDecisionStatement (
			SamlSubject samlSubject, string resource,
			SamlAccessDecision accessDecision,
			IEnumerable<SamlAction> samlActions,
			SamlEvidence samlEvidence)
			: this (samlSubject, resource, accessDecision, samlActions)
		{
			evidence = samlEvidence;
		}

		SamlAccessDecision access_decision;
		SamlEvidence evidence;
		string resource;
		List<SamlAction> actions = new List<SamlAction> ();

		public IList<SamlAction> SamlActions {
			get { return actions; }
		}

		public SamlAccessDecision AccessDecision {
			get { return access_decision; }
			set {
				CheckReadOnly ();
				access_decision = value;
			}
		}

		public SamlEvidence Evidence {
			get { return evidence; }
			set {
				CheckReadOnly ();
				evidence = value;
			}
		}

		public string Resource {
			get { return resource; }
			set {
				CheckReadOnly ();
				if (value == null || value.Length == 0)
					throw new ArgumentException ("non-zero length string must be set to Resource of SAML AuthorizationDecisionStatement.");
				resource = value;
			}
		}

		public override bool IsReadOnly {
			get { return base.IsReadOnly; }
		}

		private void CheckReadOnly ()
		{
			if (IsReadOnly)
				throw new InvalidOperationException ("This SAML assertion is read-only.");
		}

		public override void MakeReadOnly ()
		{
			base.MakeReadOnly ();
		}

		[MonoTODO]
		protected override void AddClaimsToList (IList<Claim> claims)
		{
			throw new NotImplementedException ();
		}

		public override void ReadXml (XmlDictionaryReader reader,
			SamlSerializer samlSerializer, 
			SecurityTokenSerializer keyInfoSerializer, 
			SecurityTokenResolver outOfBandTokenResolver)
		{
			if (reader == null)
				throw new ArgumentNullException ("reader");
			if (samlSerializer == null)
				throw new ArgumentNullException ("samlSerializer");

			string decision = reader.GetAttribute ("Decision");
			switch (decision) {
			case "Permit":
				AccessDecision = SamlAccessDecision.Permit;
				break;
			case "Deny":
				AccessDecision = SamlAccessDecision.Deny;
				break;
			case "Indeterminate":
				AccessDecision = SamlAccessDecision.Indeterminate;
				break;
			default:
				throw new SecurityTokenException (String.Format ("AccessDecision value is wrong: {0}", decision));
			}
			Resource = reader.GetAttribute ("Resource");

			reader.ReadStartElement ("AuthorizationDecisionStatement", SamlConstants.Namespace);

			reader.MoveToContent ();
			SamlSubject = new SamlSubject ();
			SamlSubject.ReadXml (reader, samlSerializer, keyInfoSerializer, outOfBandTokenResolver);
			SamlActions.Clear ();
			for (reader.MoveToContent ();
			     reader.LocalName == "Action" &&
			     reader.NamespaceURI == SamlConstants.Namespace;
			     reader.MoveToContent ()) {
				SamlAction action = new SamlAction ();
				action.ReadXml (reader, samlSerializer, keyInfoSerializer, outOfBandTokenResolver);
				SamlActions.Add (action);
			}
			if (reader.LocalName == "Evidence" &&
			    reader.NamespaceURI == SamlConstants.Namespace) {
				Evidence = new SamlEvidence ();
				Evidence.ReadXml (reader, samlSerializer, keyInfoSerializer, outOfBandTokenResolver);
				reader.MoveToContent ();
			}
			reader.ReadEndElement ();

			// verify contents
			if (SamlActions.Count == 0)
				throw new SecurityTokenException ("SAML AuthorizationDecisionStatement must contain at least one Action.");

			if (SamlSubject == null)
				throw new SecurityTokenException ("SAML Subject must be set to SAML AuthorizationDecisionStatement before being written.");
			if (Resource == null || Resource.Length == 0)
				throw new SecurityTokenException ("non-zero string must be set to Resource on SAML AuthorizationDecisionStatement.");
		}

		public override void WriteXml (XmlDictionaryWriter writer,
			SamlSerializer samlSerializer, 
			SecurityTokenSerializer keyInfoSerializer)
		{
			if (writer == null)
				throw new ArgumentNullException ("writer");
			if (samlSerializer == null)
				throw new ArgumentNullException ("samlSerializer");
			if (SamlActions.Count == 0)
				throw new SecurityTokenException ("SAML AuthorizationDecisionStatement must contain at least one Action.");

			if (SamlSubject == null)
				throw new SecurityTokenException ("SAML Subject must be set to SAML AuthorizationDecisionStatement before being written.");
			if (Resource == null || Resource.Length == 0)
				throw new SecurityTokenException ("non-zero string must be set to Resource on SAML AuthorizationDecisionStatement.");

			writer.WriteStartElement ("saml", "AuthorizationDecisionStatement", SamlConstants.Namespace);

			writer.WriteStartAttribute ("Decision");
			switch (AccessDecision) {
			case SamlAccessDecision.Permit:
				writer.WriteString ("Permit");
				break;
			case SamlAccessDecision.Deny:
				writer.WriteString ("Deny");
				break;
			case SamlAccessDecision.Indeterminate:
				writer.WriteString ("Indeterminate");
				break;
			default:
				throw new ArgumentOutOfRangeException ("AccessDecision value is wrong.");
			}
			writer.WriteEndAttribute ();

			writer.WriteAttributeString ("Resource", Resource);
			SamlSubject.WriteXml (writer, samlSerializer, keyInfoSerializer);
			foreach (SamlAction action in SamlActions)
				action.WriteXml (writer, samlSerializer, keyInfoSerializer);
			if (Evidence != null)
				Evidence.WriteXml (writer, samlSerializer, keyInfoSerializer);

			writer.WriteEndElement ();
		}
	}
}
