//
// SecurityTokenResolver.cs
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
using System.Collections.ObjectModel;
using System.IdentityModel.Tokens;

namespace System.IdentityModel.Selectors
{
	public abstract class SecurityTokenResolver
	{
		protected SecurityTokenResolver ()
		{
		}

		public static SecurityTokenResolver  CreateDefaultSecurityTokenResolver (
			ReadOnlyCollection<SecurityToken> tokens,
			bool canMatchLocalId)
		{
			return new DefaultSecurityTokenResolver (tokens, canMatchLocalId);
		}

		public SecurityKey ResolveSecurityKey (
			SecurityKeyIdentifierClause keyIdentifierClause)
		{
			if (keyIdentifierClause == null)
				throw new ArgumentNullException ("keyIdentifierClause");
			SecurityKey ret;
			if (!TryResolveSecurityKey (keyIdentifierClause, out ret))
				throw new InvalidOperationException (String.Format ("Could not resolve security key with the key identifier clause '{0}'", keyIdentifierClause));
			return ret;
		}

		public SecurityToken ResolveToken (
			SecurityKeyIdentifier keyIdentifier)
		{
			if (keyIdentifier == null)
				throw new ArgumentNullException ("keyIdentifierClause");
			SecurityToken ret;
			if (!TryResolveToken (keyIdentifier, out ret))
				throw new InvalidOperationException (String.Format ("Could not resolve security token from the key identifier '{0}'", keyIdentifier));
			return ret;
		}

		public SecurityToken ResolveToken (
			SecurityKeyIdentifierClause keyIdentifierClause)
		{
			if (keyIdentifierClause == null)
				throw new ArgumentNullException ("keyIdentifierClause");
			SecurityToken ret;
			if (!TryResolveToken (keyIdentifierClause, out ret))
				throw new InvalidOperationException (String.Format ("Could not resolve security token from the key identifier clause '{0}'", keyIdentifierClause));
			return ret;
		}

		public bool TryResolveSecurityKey (
			SecurityKeyIdentifierClause keyIdentifierClause, out SecurityKey key)
		{
			return TryResolveSecurityKeyCore (keyIdentifierClause, out key);
		}

		public bool TryResolveToken (
			SecurityKeyIdentifier keyIdentifier,
			out SecurityToken token)
		{
			return TryResolveTokenCore (keyIdentifier, out token);
		}

		public bool TryResolveToken (
			SecurityKeyIdentifierClause keyIdentifierClause,
			out SecurityToken token)
		{
			return TryResolveTokenCore (keyIdentifierClause, out token);
		}

		protected abstract bool TryResolveSecurityKeyCore (
			SecurityKeyIdentifierClause keyIdentifierClause,
			out SecurityKey key);

		protected abstract bool TryResolveTokenCore (
			SecurityKeyIdentifier keyIdentifier,
			out SecurityToken token);

		protected abstract bool TryResolveTokenCore (
			SecurityKeyIdentifierClause keyIdentifierClause,
			out SecurityToken token);


		class DefaultSecurityTokenResolver : SecurityTokenResolver
		{
			ReadOnlyCollection<SecurityToken> tokens;
			bool match_local;

			public DefaultSecurityTokenResolver (
				ReadOnlyCollection<SecurityToken> tokens,
				bool canMatchLocalId)
			{
				this.tokens = tokens;
				this.match_local = canMatchLocalId;
			}

			protected override bool TryResolveSecurityKeyCore (
				SecurityKeyIdentifierClause clause,
				out SecurityKey key)
			{
				if (clause == null)
					throw new ArgumentNullException ("clause");
				foreach (SecurityToken token in tokens)
					if (TokenMatchesClause (token, clause)) {
						key = token.ResolveKeyIdentifierClause (clause);
						if (key != null)
							return true;
					}
				key = null;
				return false;
			}

			protected override bool TryResolveTokenCore (
				SecurityKeyIdentifier keyIdentifier,
				out SecurityToken token)
			{
				if (keyIdentifier == null)
					throw new ArgumentNullException ("keyIdentifier");
				foreach (SecurityKeyIdentifierClause kic in keyIdentifier)
					if (TryResolveTokenCore (kic, out token))
						return true;
				token = null;
				return false;
			}

			protected override bool TryResolveTokenCore (
				SecurityKeyIdentifierClause clause,
				out SecurityToken token)
			{
				if (clause == null)
					throw new ArgumentNullException ("clause");
				foreach (SecurityToken t in tokens)
					if (TokenMatchesClause (t, clause)) {
						token = t;
						return true;
					}
				token = null;
				return false;
			}

			bool TokenMatchesClause (SecurityToken token, SecurityKeyIdentifierClause clause)
			{
				if (token.MatchesKeyIdentifierClause (clause))
					return true;
				if (!match_local)
					return false;
				LocalIdKeyIdentifierClause l =
					clause as LocalIdKeyIdentifierClause;
				return l != null && l.Matches (token.Id, token.GetType ());
			}
		}
	}
}
