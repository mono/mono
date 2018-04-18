//
// LocalIdKeyIdentifierClause.cs
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
using System.IdentityModel.Policy;

namespace System.IdentityModel.Tokens
{
	public class LocalIdKeyIdentifierClause : SecurityKeyIdentifierClause
	{
		public LocalIdKeyIdentifierClause (string localId)
			: this (localId, null)
		{
		}

		public LocalIdKeyIdentifierClause (string localId, Type ownerType)
			: this (localId, null, 0, ownerType)
		{
		}

		public LocalIdKeyIdentifierClause (string localId, byte [] derivationNonce, int derivationLength, Type ownerType)
			: base (localId, derivationNonce, derivationLength)
		{
			local_id = localId;
			owner_type =ownerType;
		}

		string local_id;
		Type owner_type;

		public string LocalId {
			get { return local_id; }
		}

		public Type OwnerType {
			get { return owner_type; }
		}

		public override bool Matches (SecurityKeyIdentifierClause keyIdentifierClause)
		{
			if (keyIdentifierClause == null)
				throw new ArgumentNullException ("keyIdentifierClause");
			LocalIdKeyIdentifierClause c =
				keyIdentifierClause as LocalIdKeyIdentifierClause;
			return c != null && Matches (c.LocalId, c.OwnerType);
		}

		public bool Matches (string localId, Type ownerType)
		{
			return local_id == localId && (owner_type == null || owner_type == ownerType);
		}

		[MonoTODO]
		public override string ToString ()
		{
			return base.ToString ();
		}
	}
}
