//
// SecurityToken.cs
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
using System.Collections.ObjectModel;
using System.Xml;
using System.IdentityModel.Policy;

namespace System.IdentityModel.Tokens
{
	public abstract class SecurityToken
	{
		protected SecurityToken ()
		{
		}

		[MonoTODO]
		public abstract DateTime ValidFrom { get; }

		[MonoTODO]
		public abstract DateTime ValidTo { get; }

		public abstract string Id { get; }

		public abstract ReadOnlyCollection<SecurityKey> SecurityKeys { get; }

		[MonoTODO]
		public virtual bool CanCreateKeyIdentifierClause<T> ()
			where T : SecurityKeyIdentifierClause
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual T CreateKeyIdentifierClause<T> ()
			where T : SecurityKeyIdentifierClause
		{
			throw new NotImplementedException ();
		}

		public virtual bool MatchesKeyIdentifierClause (
			SecurityKeyIdentifierClause keyIdentifierClause)
		{
			return false;
		}

		[MonoTODO]
		public virtual SecurityKey ResolveKeyIdentifierClause (
			SecurityKeyIdentifierClause keyIdentifierClause)
		{
			if (keyIdentifierClause == null)
				throw new ArgumentNullException ("keyIdentifierClause");
			if (!MatchesKeyIdentifierClause (keyIdentifierClause))
				throw new InvalidOperationException (String.Format ("This '{0}' security token does not support resolving '{1}' key identifier clause.", GetType (), keyIdentifierClause));
			if (keyIdentifierClause.CanCreateKey)
				return keyIdentifierClause.CreateKey ();
			// FIXME: examine it.
			if (SecurityKeys.Count == 0)
				throw new InvalidOperationException (String.Format ("This '{0}' security token does not have any keys that can be resolved.", GetType (), keyIdentifierClause));
			return SecurityKeys [0];
		}
	}
}
