//
// SamlAction.cs
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
using System.Xml;
using System.IdentityModel.Selectors;

namespace System.IdentityModel.Tokens
{
	public class SamlAction
	{
		string action, ns;
		bool is_readonly;

		public SamlAction ()
		{
		}

		public SamlAction (string action)
		{
			Action = action;
		}

		public SamlAction (string action, string ns)
		{
			Action = action;
			Namespace = ns;
		}

		public string Action {
			get { return action; }
			set {
				CheckReadOnly ();
				if (value == null || value.Length == 0)
					throw new ArgumentException ("SAML Action must be non-zero length string.");
				action = value;
			}
		}

		public bool IsReadOnly {
			get { return is_readonly; }
		}

		public string Namespace {
			get { return ns; }
			set {
				CheckReadOnly ();
				ns = value;
			}
		}

		void CheckReadOnly ()
		{
			if (is_readonly)
				throw new InvalidOperationException ("This SAML action is read-only.");
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
				throw new ArgumentNullException ("writer");
			if (samlSerializer == null)
				throw new ArgumentNullException ("samlSerializer");

			Namespace = reader.GetAttribute ("Namespace");
			Action =  reader.ReadElementContentAsString ("Action", SamlConstants.Namespace);

			if (Action == null)
				throw new SecurityTokenException ("non-zero length string must exist for SAML Action.");
		}

		public virtual void WriteXml (XmlDictionaryWriter writer,
			SamlSerializer samlSerializer,
			SecurityTokenSerializer keyInfoSerializer)
		{
			if (writer == null)
				throw new ArgumentNullException ("writer");
			if (samlSerializer == null)
				throw new ArgumentNullException ("samlSerializer");

			if (Action == null)
				throw new SecurityTokenException ("non-zero length string must be set for SAML Action before being written.");

			writer.WriteStartElement ("saml", "Action", SamlConstants.Namespace);
			if (Namespace != null)
				writer.WriteAttributeString ("Namespace", Namespace);
			writer.WriteString (Action);
			writer.WriteEndElement ();
		}
	}
}
