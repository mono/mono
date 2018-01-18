//
// SecurityTokenResolverTest.cs
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
#if !MOBILE
using System;
using System.Collections.ObjectModel;
using System.IdentityModel.Selectors;
using System.IdentityModel.Tokens;
using NUnit.Framework;

namespace MonoTests.System.IdentityModel.Selectors
{
	[TestFixture]
	public class SecurityTokenResolverTest
	{
		SecurityTokenResolver GetResolver (bool canMatchLocalId, params SecurityToken [] tokens)
		{
			return SecurityTokenResolver.CreateDefaultSecurityTokenResolver (new ReadOnlyCollection<SecurityToken> (tokens), canMatchLocalId);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void TryResolveTokenNullClause ()
		{
			SecurityTokenResolver r = GetResolver (true, new SecurityToken [0]);
			SecurityToken token;
			r.TryResolveToken ((SecurityKeyIdentifierClause) null, out token);
		}

		[Test]
		public void TryResolveToken ()
		{
			SecurityTokenResolver r = GetResolver (true, new SecurityToken [0]);
			SecurityToken token;
			Assert.IsFalse (r.TryResolveToken (new LocalIdKeyIdentifierClause ("foo"), out token));

			UserNameSecurityToken userName =
				new UserNameSecurityToken ("mono", "", "urn:foo");
			LocalIdKeyIdentifierClause kic =
				new LocalIdKeyIdentifierClause ("urn:foo");

			r = GetResolver (true, new SecurityToken [] {userName});
			Assert.IsTrue (r.TryResolveToken (kic, out token));

			r = GetResolver (false, new SecurityToken [] {userName});
			Assert.IsFalse (r.TryResolveToken (kic, out token));
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void ResolveTokenNonExistent ()
		{
			SecurityTokenResolver r = GetResolver (true, new SecurityToken [0]);
			SecurityToken token;
			Assert.IsNull (r.ResolveToken (new LocalIdKeyIdentifierClause ("urn:foo")));
		}
	}
}
#endif
