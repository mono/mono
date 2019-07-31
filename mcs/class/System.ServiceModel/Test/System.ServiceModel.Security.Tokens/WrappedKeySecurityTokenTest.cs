//
// WrappedKeySecurityTokenTest.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2007 Novell, Inc.  http://www.novell.com
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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IdentityModel.Selectors;
using System.IdentityModel.Tokens;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Security;
using System.ServiceModel.Security.Tokens;
using NUnit.Framework;

using MonoTests.Helpers;

namespace MonoTests.System.ServiceModel
{
	[TestFixture]
	public class WrappedKeySecurityTokenTest
	{
		static readonly X509Certificate2 cert =
			new X509Certificate2 (TestResourceHelper.GetFullPathOfResource ("Test/Resources/test.pfx"), "mono");

		WrappedKeySecurityToken GetReferent ()
		{
			string id = "referent";
			byte [] key = new byte [32];
			X509SecurityToken token = new X509SecurityToken (cert);
			SecurityKeyIdentifierClause kic =
				new X509ThumbprintKeyIdentifierClause (cert);
			string alg = SecurityAlgorithms.RsaOaepKeyWrap;
			return new WrappedKeySecurityToken (id, key, alg, token,
				new SecurityKeyIdentifier (kic));
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void CtorNullId ()
		{
			WrappedKeySecurityToken w = GetReferent ();
			new WrappedKeySecurityToken (null, new byte [32],
				w.WrappingAlgorithm,
				w.WrappingToken,
				w.WrappingTokenReference);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void CtorNullKey ()
		{
			WrappedKeySecurityToken w = GetReferent ();
			new WrappedKeySecurityToken (w.Id, null,
				w.WrappingAlgorithm,
				w.WrappingToken,
				w.WrappingTokenReference);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void CtorNullWrappingAlgorithm ()
		{
			WrappedKeySecurityToken w = GetReferent ();
			new WrappedKeySecurityToken (w.Id, new byte [32],
				null,
				w.WrappingToken,
				w.WrappingTokenReference);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void CtorNullWrappingToken ()
		{
			WrappedKeySecurityToken w = GetReferent ();
			new WrappedKeySecurityToken (w.Id, new byte [32],
				w.WrappingAlgorithm,
				null,
				w.WrappingTokenReference);
		}

		[Test]
		// null SecurityKeyIdentifier is allowed.
		//[ExpectedException (typeof (ArgumentNullException))]
		public void CtorNullWrappingTokenReference ()
		{
			WrappedKeySecurityToken w = GetReferent ();
			new WrappedKeySecurityToken (w.Id, new byte [32],
				w.WrappingAlgorithm,
				w.WrappingToken,
				null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void UserNameToken () // it does not support any encryption operation.
		{
			byte [] bytes = new byte [32];
			SecurityToken wt = new UserNameSecurityToken ("eno", "enopass");
			SecurityKeyIdentifierClause kic =
				new X509ThumbprintKeyIdentifierClause (cert);
			new WrappedKeySecurityToken ("urn:gyabo",
				bytes, SecurityAlgorithms.RsaOaepKeyWrap, wt,
				new SecurityKeyIdentifier (kic));
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void X509TokenForSymmetricKeyWrap ()
		{
			byte [] bytes = new byte [32];
			SecurityToken wt = new X509SecurityToken (cert);
			SecurityKeyIdentifierClause kic =
				new X509ThumbprintKeyIdentifierClause (cert);
			new WrappedKeySecurityToken ("urn:gyabo",
				bytes, SecurityAlgorithms.Aes256KeyWrap, wt,
				new SecurityKeyIdentifier (kic));
		}

		[Test]
		public void BinarySecretTokenForSymmetricKeyWrap ()
		{
			byte [] bytes = new byte [32];
			SecurityToken wt = new BinarySecretSecurityToken (bytes);
			SecurityKeyIdentifierClause kic =
				new X509ThumbprintKeyIdentifierClause (cert);
			new WrappedKeySecurityToken ("urn:gyabo",
				bytes, SecurityAlgorithms.Aes256KeyWrap, wt,
				new SecurityKeyIdentifier (kic));
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void BinarySecretTokenForAsymmetricKeyWrap ()
		{
			byte [] bytes = new byte [32];
			SecurityToken wt = new BinarySecretSecurityToken (bytes);
			SecurityKeyIdentifierClause kic =
				new X509ThumbprintKeyIdentifierClause (cert);
			new WrappedKeySecurityToken ("urn:gyabo",
				bytes, SecurityAlgorithms.RsaOaepKeyWrap, wt,
				new SecurityKeyIdentifier (kic));
		}

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void CreateBinarySecretKeyIdentifierClause ()
		{
			byte [] bytes = new byte [32];
			SecurityToken wt = new BinarySecretSecurityToken (bytes);
			SecurityKeyIdentifierClause kic =
				new BinarySecretKeyIdentifierClause (bytes);
			WrappedKeySecurityToken token = new WrappedKeySecurityToken ("urn:gyabo",
				bytes, SecurityAlgorithms.Aes256KeyWrap, wt,
				new SecurityKeyIdentifier (kic));
			token.CreateKeyIdentifierClause<BinarySecretKeyIdentifierClause> ();
		}

		[Test]
		public void X509WrappingToken1 ()
		{
			byte [] bytes = new byte [32];
			X509SecurityToken xt = new X509SecurityToken (cert);
			SecurityKeyIdentifierClause kic =
				new X509ThumbprintKeyIdentifierClause (cert);
			string alg = SecurityAlgorithms.RsaOaepKeyWrap;
			WrappedKeySecurityToken token = new WrappedKeySecurityToken ("urn:gyabo",
				bytes, alg, xt,
				new SecurityKeyIdentifier (kic));
			Assert.AreEqual ("urn:gyabo", token.Id, "#1");
			Assert.AreEqual (alg, token.WrappingAlgorithm, "#3");
			Assert.AreEqual (xt, token.WrappingToken, "#4");
			Assert.AreEqual (1, token.WrappingTokenReference.Count, "#5");
			Assert.AreEqual (1, token.SecurityKeys.Count, "#6");
			Assert.IsTrue (token.SecurityKeys [0] is InMemorySymmetricSecurityKey, "#7");
			Assert.AreEqual (bytes, new X509AsymmetricSecurityKey (cert).DecryptKey (token.WrappingAlgorithm, token.GetWrappedKey ()), "#8");
			// wrapped keys cannot be compared, due to the nature of rsa-oaep.
			// Assert.AreEqual (new X509AsymmetricSecurityKey (cert).EncryptKey (token.WrappingAlgorithm, bytes), token.GetWrappedKey (), "#9-1");
			// Assert.AreEqual (token.GetWrappedKey (), new WrappedKeySecurityToken ("urn:gyabo",
			//	bytes, alg, xt,
			//	new SecurityKeyIdentifier (kic)).GetWrappedKey (), "#9");
		}
	}
}
#endif
