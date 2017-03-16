//
// X509SecurityToken.cs
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
using System.Collections.ObjectModel;
using System.Xml;
using System.IdentityModel.Policy;
using System.Security.Cryptography.X509Certificates;

namespace System.IdentityModel.Tokens
{
	public class X509SecurityToken : SecurityToken, IDisposable
	{
		public X509SecurityToken (X509Certificate2 certificate)
			: this (certificate, "uuid:" + Guid.NewGuid ().ToString ())
		{
		}

		public X509SecurityToken (X509Certificate2 certificate, string id)
		{
			if (certificate == null)
				throw new ArgumentNullException ("certificate");
			if (id == null)
				throw new ArgumentNullException ("id");
			this.cert = certificate;
			this.id = id;
		}

		X509Certificate2 cert;
		string id;
		ReadOnlyCollection<SecurityKey> keys;

		public X509Certificate2 Certificate {
			get { return cert; }
		}

		public override DateTime ValidFrom {
			get { return cert.NotBefore.ToUniversalTime (); }
		}

		public override DateTime ValidTo {
			get { return cert.NotAfter.ToUniversalTime (); }
		}

		public override string Id {
			get { return id; }
		}

		public virtual void Dispose ()
		{
			cert.Reset ();
			cert = null;
		}

		public override ReadOnlyCollection<SecurityKey> SecurityKeys {
			get {
				if (keys == null)
					keys = new ReadOnlyCollection<SecurityKey> (new SecurityKey [] {new X509AsymmetricSecurityKey (cert)});
				return keys;
			}
		}

		public override bool CanCreateKeyIdentifierClause<T> ()
		{
			Type t = typeof (T);
			return
//				t == typeof (X509SubjectKeyIdentifierClause) ||
				t == typeof (X509ThumbprintKeyIdentifierClause) ||
				t == typeof (X509IssuerSerialKeyIdentifierClause) ||
				t == typeof (X509RawDataKeyIdentifierClause);
		}

		public override T CreateKeyIdentifierClause<T> ()
		{
			Type t = typeof (T);
//			if (t == typeof (X509SubjectKeyIdentifierClause))
//				return (T) (object) new X509SubjectKeyIdentifierClause (cert.SubjectName.RawData);
			if (t == typeof (X509ThumbprintKeyIdentifierClause))
				return (T) (object) new X509ThumbprintKeyIdentifierClause (cert);
			if (t == typeof (X509IssuerSerialKeyIdentifierClause))
				return (T) (object) new X509IssuerSerialKeyIdentifierClause (cert);
			if (t == typeof (X509RawDataKeyIdentifierClause))
				return (T) (object) new X509RawDataKeyIdentifierClause (cert);

			throw new NotSupportedException (String.Format ("X509SecurityToken does not support creation of {0}.", t));
		}

		[MonoTODO]
		public override bool MatchesKeyIdentifierClause (
			SecurityKeyIdentifierClause keyIdentifierClause)
		{
			LocalIdKeyIdentifierClause l =
				keyIdentifierClause as LocalIdKeyIdentifierClause;
			if (l != null)
				return l.LocalId == Id;

			X509ThumbprintKeyIdentifierClause t =
				keyIdentifierClause as X509ThumbprintKeyIdentifierClause;
			if (t != null)
				return t.Matches (cert);
			X509IssuerSerialKeyIdentifierClause i =
				keyIdentifierClause as X509IssuerSerialKeyIdentifierClause;
			if (i != null)
				return i.Matches (cert);
			X509SubjectKeyIdentifierClause s =
				keyIdentifierClause as X509SubjectKeyIdentifierClause;
			if (s != null)
				return s.Matches (cert);
			X509RawDataKeyIdentifierClause r =
				keyIdentifierClause as X509RawDataKeyIdentifierClause;
			if (r != null)
				return r.Matches (cert);

			return false;
		}

		protected void ThrowIfDisposed ()
		{
			if (cert == null)
				throw new ObjectDisposedException ("This X509SecurityToken has already been disposed.");
		}
	}
}
