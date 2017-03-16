//
// X509SubjectKeyIdentifierClause.cs
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
using System.Security.Cryptography.X509Certificates;
using System.IdentityModel.Policy;

namespace System.IdentityModel.Tokens
{
	public class X509SubjectKeyIdentifierClause : BinaryKeyIdentifierClause
	{
		[MonoTODO]
		public X509SubjectKeyIdentifierClause (byte [] ski)
			: base (null, ski, true)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static bool CanCreateFrom (X509Certificate2 certificate)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public byte [] GetX509SubjectKeyIdentifier ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public bool Matches (X509Certificate2 certificate)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override string ToString ()
		{
			return base.ToString ();
		}

		[MonoTODO]
		public static bool TryCreateFrom (X509Certificate2 certificate, out X509SubjectKeyIdentifierClause keyIdentifierClause)
		{
			throw new NotImplementedException ();
		}
	}
}
