//
// NativeGenericXmlToken.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2007 Novell, Inc.  http://www.novell.com
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
using System.Collections.ObjectModel;
using System.IO;
using System.IdentityModel.Selectors;
using System.IdentityModel.Tokens;
using System.Runtime.InteropServices;
using System.Xml;

namespace Mono.ServiceModel.IdentitySelectors.Win32
{
	[StructLayout (LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	class NativeGenericXmlToken : IDisposable
	{
		// This field order must be fixed for win32 API interop:
		long created;
		long expired;
		string xml_token;
		string internal_ref;
		string external_ref;

		public NativeGenericXmlToken (GenericXmlSecurityToken token, SecurityTokenSerializer serializer)
		{
			created = token.ValidFrom.ToFileTime ();
			expired = token.ValidTo.ToFileTime ();
			xml_token = token.TokenXml.OuterXml;
			XmlWriterSettings settings = new XmlWriterSettings ();
			settings.OmitXmlDeclaration = true;
			internal_ref = GetKeyIdentifierClauseXml (serializer, settings, token.InternalTokenReference);
			external_ref = GetKeyIdentifierClauseXml (serializer, settings, token.ExternalTokenReference);
		}

		void IDisposable.Dispose ()
		{
			FreeToken (this);
		}

		public static string GetKeyIdentifierClauseXml (SecurityTokenSerializer serializer, XmlWriterSettings settings, SecurityKeyIdentifierClause item)
		{
			StringWriter sw = new StringWriter ();
			using (XmlWriter xw = XmlWriter.Create (sw)) {
				serializer.WriteKeyIdentifierClause (xw, item);
			}
			return sw.ToString ();
		}

		public GenericXmlSecurityToken ToObject (NativeInfocardCryptoHandle proofTokenHandle, SecurityTokenSerializer serializer)
		{
			XmlDocument doc = new XmlDocument ();
			doc.LoadXml (xml_token);
			XmlElement token = doc.DocumentElement;
			SecurityToken proof = new CardSpaceProofToken (DateTime.FromFileTime (proofTokenHandle.Expiration), proofTokenHandle.GetAsymmetricKey ());

			DateTime effective = DateTime.FromFileTime (created);
			DateTime expiration = DateTime.FromFileTime (expired);

			SecurityKeyIdentifierClause intref =
				serializer.ReadKeyIdentifierClause (Create (internal_ref));
			SecurityKeyIdentifierClause extref =
				serializer.ReadKeyIdentifierClause (Create (external_ref));
			return new GenericXmlSecurityToken (token, proof, effective, expiration, intref, extref, null);
		}

		XmlDictionaryReader Create (string xml)
		{
			XmlReader xr = XmlReader.Create (new StringReader (xml));
			return XmlDictionaryReader.CreateDictionaryReader (xr);
		}

		[DllImport ("infocardapi")]
		static extern void FreeToken (NativeGenericXmlToken token);
	}
}
