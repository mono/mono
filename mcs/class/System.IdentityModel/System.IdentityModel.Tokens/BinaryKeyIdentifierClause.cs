//
// BinaryKeyIdentifierClause.cs
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
	public abstract class BinaryKeyIdentifierClause : SecurityKeyIdentifierClause
	{
		protected BinaryKeyIdentifierClause (string clauseType, byte [] identificationData, bool cloneBuffer)
			: this (clauseType, identificationData, cloneBuffer, null, 0)
		{
		}

		protected BinaryKeyIdentifierClause (string clauseType, byte [] identificationData, bool cloneBuffer, byte [] derivationNonce, int derivationLength)
			: base (clauseType, derivationNonce, derivationLength)
		{
			this.data = cloneBuffer ?
				(byte []) identificationData.Clone () :
				identificationData;
		}

		byte [] data;

		public byte [] GetBuffer ()
		{
			return (byte []) GetRawBuffer ().Clone ();
		}

		public override bool Matches (SecurityKeyIdentifierClause keyIdentifierClause)
		{
			BinaryKeyIdentifierClause other =
				keyIdentifierClause as BinaryKeyIdentifierClause;
			if (other == null)
				return false;
			return Matches (other.GetRawBuffer ());
		}

		public bool Matches (byte [] data)
		{
			return Matches (data, 0);
		}

		public bool Matches (byte [] data, int offset)
		{
			if (data.Length + offset != this.data.Length)
				return false;
			for (int i = 0; i < this.data.Length; i++)
				if (data [i + offset] != this.data [i])
					return false;
			return true;
		}

		protected byte [] GetRawBuffer ()
		{
			return data;
		}
	}
}
