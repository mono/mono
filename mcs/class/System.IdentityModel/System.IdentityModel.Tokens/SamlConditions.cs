//
// SamlConditions.cs
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
using System.Globalization;
using System.Xml;
using System.IdentityModel.Policy;
using System.IdentityModel.Selectors;
using System.Security.Cryptography;

namespace System.IdentityModel.Tokens
{
	public class SamlConditions
	{
		DateTime not_before = DateTime.SpecifyKind (DateTime.MinValue.AddDays (1), DateTimeKind.Utc);
		DateTime not_on_after = DateTime.SpecifyKind (DateTime.MaxValue.AddDays (-1), DateTimeKind.Utc);
		bool is_readonly, has_not_before, has_not_on_after;
		List<SamlCondition> conditions = new List<SamlCondition> ();

		public SamlConditions ()
		{
		}

		public SamlConditions (DateTime notBefore, DateTime notOnOrAfter)
		{
			this.NotBefore = notBefore;
			this.NotOnOrAfter = notOnOrAfter;
		}

		public SamlConditions (DateTime notBefore, DateTime notOnOrAfter,
			IEnumerable<SamlCondition> conditions)
			: this (notBefore, notOnOrAfter)
		{
			if (conditions != null) {
				foreach (SamlCondition cond in conditions)
					this.conditions.Add (cond);
			}
		}

		public IList<SamlCondition> Conditions {
			get { return conditions; }
		}

		public DateTime NotBefore {
			get { return not_before; }
			set {
				CheckReadOnly ();
				not_before = value;
				has_not_before = true;
			}
		}

		public DateTime NotOnOrAfter {
			get { return not_on_after; }
			set {
				CheckReadOnly ();
				not_on_after = value;
				has_not_on_after = true;
			}
		}

		public bool IsReadOnly {
			get { return is_readonly; }
		}

		private void CheckReadOnly ()
		{
			if (is_readonly)
				throw new InvalidOperationException ("This SAML 'Conditions' is read-only.");
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
			if (samlSerializer == null)
				throw new ArgumentNullException ("samlSerializer");
			writer.WriteStartElement ("saml", "Conditions", SamlConstants.Namespace);
			CultureInfo invariant = CultureInfo.InvariantCulture;
			if (has_not_before)
				writer.WriteAttributeString ("NotBefore", NotBefore.ToString (SamlConstants.DateFormat, invariant));
			if (has_not_on_after)
				writer.WriteAttributeString ("NotOnOrAfter", NotOnOrAfter.ToString (SamlConstants.DateFormat, invariant));
			foreach (SamlCondition cond in Conditions)
				cond.WriteXml (writer, samlSerializer, keyInfoSerializer);
			writer.WriteEndElement ();
		}
	}
}
