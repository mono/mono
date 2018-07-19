//
// SecurityContextKeyIdentifierClause.cs
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

using System.IdentityModel.Selectors;
using System.IdentityModel.Tokens;
using System.Xml;

namespace System.ServiceModel.Security
{
	public class SecurityContextKeyIdentifierClause : SecurityKeyIdentifierClause
	{
		public SecurityContextKeyIdentifierClause (UniqueId contextId)
			: this (contextId, new UniqueId ())
		{
		}

		public SecurityContextKeyIdentifierClause (UniqueId contextId, UniqueId generation)
			: this (contextId, generation, null, 0)
		{
		}

		public SecurityContextKeyIdentifierClause (UniqueId contextId, UniqueId generation, byte [] derivationNonce, int derivationLength)
			: base (null, derivationNonce, derivationLength)
		{
			this.context = contextId;
			this.generation = generation;
		}

		UniqueId context, generation;

		public UniqueId ContextId {
			get { return context; }
		}

		public UniqueId Generation {
			get { return generation; }
		}

		public override bool Matches (
			SecurityKeyIdentifierClause keyIdentifierClause)
		{
			SecurityContextKeyIdentifierClause other =
				keyIdentifierClause as SecurityContextKeyIdentifierClause;
			return  other != null && Matches (other.context, other.generation);
		}

		public bool Matches (UniqueId contextId, UniqueId generation)
		{
			return context == contextId &&
				this.generation == generation;
		}

		[MonoTODO]
		public override string ToString ()
		{
			return base.ToString ();
		}
	}
}
