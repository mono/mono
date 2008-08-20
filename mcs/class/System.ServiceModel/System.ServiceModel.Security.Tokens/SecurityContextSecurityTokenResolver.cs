//
// SecurityContextSecurityTokenResolver.cs
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
using System.Collections.ObjectModel;
using System.IdentityModel.Selectors;
using System.IdentityModel.Tokens;
using System.Xml;

using Table = System.Collections.Generic.Dictionary<System.Xml.UniqueId,System.ServiceModel.Security.Tokens.SecurityContextSecurityToken>;

namespace System.ServiceModel.Security.Tokens
{
	public class SecurityContextSecurityTokenResolver : SecurityTokenResolver, ISecurityContextSecurityTokenCache
	{
		int capacity;
		bool allow_removal;

		Dictionary<UniqueId,Table> cache =
			new Dictionary<UniqueId,Table> ();

		public SecurityContextSecurityTokenResolver (
			int securityContextCacheCapacity,
			bool removeOldestTokensOnCacheFull)
		{
			capacity = securityContextCacheCapacity;
			allow_removal = removeOldestTokensOnCacheFull;
		}

		public bool RemoveOldestTokensOnCacheFull {
			get { return allow_removal; }
		}

		public int SecurityContextTokenCacheCapacity {
			get { return capacity; }
		}

		public void AddContext (SecurityContextSecurityToken token)
		{
			if (!TryAddContext (token))
				throw new InvalidOperationException ("Argument token is already in the cache.");
		}

		public void ClearContexts ()
		{
			cache.Clear ();
		}

		public Collection<SecurityContextSecurityToken> GetAllContexts (UniqueId contextId)
		{
			Table table;
			if (!cache.TryGetValue (contextId, out table))
				return new Collection<SecurityContextSecurityToken> ();
			SecurityContextSecurityToken [] arr =
				new SecurityContextSecurityToken [table.Count];
			table.Values.CopyTo (arr, 0);
			return new Collection<SecurityContextSecurityToken> (arr);
		}

		public SecurityContextSecurityToken GetContext (UniqueId contextId, UniqueId generation)
		{
			Table table;
			if (!cache.TryGetValue (contextId, out table))
				return null;
			SecurityContextSecurityToken ret;
			return table.TryGetValue (generation, out ret) ? ret : null;
		}

		public void RemoveAllContexts (UniqueId contextId)
		{
			cache.Remove (contextId);
		}

		public void RemoveContext (UniqueId contextId, UniqueId generation)
		{
			Table table;
			if (!cache.TryGetValue (contextId, out table))
				return;
			table.Remove (generation);
		}

		public bool TryAddContext (SecurityContextSecurityToken token)
		{
			Table table;
			if (!cache.TryGetValue (token.ContextId, out table)) {
				table = new Table ();
				table [token.KeyGeneration] = token;
			} else {
				if (table.ContainsKey (token.KeyGeneration))
					return false;
				table [token.KeyGeneration] = token;
			}
			return true;
		}

		[MonoTODO]
		public void UpdateContextCachingTime (SecurityContextSecurityToken context, DateTime expirationTime)
		{
			throw new NotImplementedException ();
		}

		// SecurityTokenResolver

		[MonoTODO]
		protected override bool TryResolveSecurityKeyCore (
			SecurityKeyIdentifierClause keyIdentifierClause,
			out SecurityKey key)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected override bool TryResolveTokenCore (
			SecurityKeyIdentifier keyIdentifier,
			out SecurityToken token)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected override bool TryResolveTokenCore (
			SecurityKeyIdentifierClause keyIdentifierClause,
			out SecurityToken token)
		{
			throw new NotImplementedException ();
		}
	}
}
