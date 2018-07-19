//
// BinarySecretKeyIdentifierClause.cs
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
using System.IdentityModel.Policy;
using System.IdentityModel.Tokens;

namespace System.ServiceModel.Security
{
	public class BinarySecretKeyIdentifierClause : BinaryKeyIdentifierClause
	{
		public BinarySecretKeyIdentifierClause (byte [] key)
			: this (key, true)
		{
		}

		[MonoTODO ("ClauseType")]
		public BinarySecretKeyIdentifierClause (byte [] key, bool cloneBuffer)
			: base ("", key, cloneBuffer)
		{
		}

		[MonoTODO ("ClauseType")]
		public BinarySecretKeyIdentifierClause (byte [] key, bool cloneBuffer, byte [] derivationNonce, int derivationLength)
			: base ("", key, cloneBuffer, derivationNonce, derivationLength)
		{
		}

		public override bool CanCreateKey {
			get { return true; }
		}

		public byte [] GetKeyBytes ()
		{
			return GetBuffer ();
		}

		public override SecurityKey CreateKey ()
		{
			return new InMemorySymmetricSecurityKey (GetRawBuffer (), true);
		}

		public override bool Matches (SecurityKeyIdentifierClause clause)
		{
			if (clause == null)
				throw new ArgumentNullException ("clause");
			BinarySecretKeyIdentifierClause other =
				clause as BinarySecretKeyIdentifierClause;
			if (other == null)
				return false;
			byte [] b1 = GetRawBuffer ();
			byte [] b2 = other.GetRawBuffer ();
			if (b1.Length != b2.Length)
				return false;
			for (int i = 0; i < b1.Length; i++)
				if (b1 [i] != b2 [i])
					return false;
			return true;
		}
	}
}
