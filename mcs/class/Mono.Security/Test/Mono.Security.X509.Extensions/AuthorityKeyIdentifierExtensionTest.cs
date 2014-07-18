//
// AuthorityKeyIdentifierExtensionTest.cs - NUnit Test Cases for 
//	Mono.Security.X509.Extensions.AuthorityKeyIdentifierExtension
//
// Authors:
//	Lex Li  <support@lextm.com>
//
// Copyright (C) 2014 Lex Li
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this
// software and associated documentation files (the "Software"), to deal in the Software
// without restriction, including without limitation the rights to use, copy, modify, merge,
// publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons
// to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or
// substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
// INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR
// PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE
// FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR
// OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.

using System;
using Mono.Security.X509.Extensions;
using NUnit.Framework;

namespace MonoTests.Mono.Security.X509.Extensions
{
	[TestFixture]
	public class AuthorityKeyIdentifierExtensionTest
	{
		private void Empty (AuthorityKeyIdentifierExtension aki)
		{
			Assert.IsFalse (aki.Critical, "Critical");
			Assert.AreEqual ("2.5.29.35", aki.Oid, "Oid");
			Assert.IsNotNull (aki.Name, "Name");
			Assert.IsFalse (aki.Name == aki.Oid, "Name!=Oid");
			Assert.AreEqual (new byte[] {
				0x00, 0x00, 0x00, 0x00, 0x00,
				0x00, 0x00, 0x00, 0x00, 0x00,
				0x00, 0x00, 0x00, 0x00, 0x00,
				0x00, 0x00, 0x00, 0x00, 0x00
			}, aki.Identifier, "Identifier");
		}

		[Test]
		public void Constructor_Empty ()
		{
			AuthorityKeyIdentifierExtension aki = new AuthorityKeyIdentifierExtension ();
			aki.Identifier = new byte[] {
				0x00, 0x00, 0x00, 0x00, 0x00,
				0x00, 0x00, 0x00, 0x00, 0x00,
				0x00, 0x00, 0x00, 0x00, 0x00,
				0x00, 0x00, 0x00, 0x00, 0x00
			};
			Empty (aki);
		}

		[Test]
		public void Constructor_Extension ()
		{
			AuthorityKeyIdentifierExtension ext = new AuthorityKeyIdentifierExtension ();
			ext.Identifier = new byte[] {
				0x00, 0x00, 0x00, 0x00, 0x00,
				0x00, 0x00, 0x00, 0x00, 0x00,
				0x00, 0x00, 0x00, 0x00, 0x00,
				0x00, 0x00, 0x00, 0x00, 0x00
			};
			AuthorityKeyIdentifierExtension aki = new AuthorityKeyIdentifierExtension (ext);
			Empty (aki);
		}

		[Test]
		public void Constructor_ASN1 ()
		{
			AuthorityKeyIdentifierExtension ext = new AuthorityKeyIdentifierExtension ();
			ext.Identifier = new byte[] {
				0x00, 0x00, 0x00, 0x00, 0x00,
				0x00, 0x00, 0x00, 0x00, 0x00,
				0x00, 0x00, 0x00, 0x00, 0x00,
				0x00, 0x00, 0x00, 0x00, 0x00
			};
			AuthorityKeyIdentifierExtension aki = new AuthorityKeyIdentifierExtension (ext.ASN1);
			Empty (aki);
		}

		[Test]
		public void AuthorityKeyIdentifier_Critical ()
		{
			AuthorityKeyIdentifierExtension aki = new AuthorityKeyIdentifierExtension ();
			aki.Critical = true;
			aki.Identifier = new byte[] {
				0x00, 0x00, 0x00, 0x00, 0x00,
				0x00, 0x00, 0x00, 0x00, 0x00,
				0x00, 0x00, 0x00, 0x00, 0x00,
				0x00, 0x00, 0x00, 0x00, 0x00
			};
			Assert.AreEqual ("30-22-06-03-55-1D-23-01-01-FF-04-18-30-16-80-14-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00", BitConverter.ToString (aki.GetBytes ()), "GetBytes");

			AuthorityKeyIdentifierExtension aki2 = new AuthorityKeyIdentifierExtension (aki.ASN1);
			Assert.IsTrue (aki2.Critical, "Critical");
			Assert.AreEqual (new byte[] {
				0x00, 0x00, 0x00, 0x00, 0x00,
				0x00, 0x00, 0x00, 0x00, 0x00,
				0x00, 0x00, 0x00, 0x00, 0x00,
				0x00, 0x00, 0x00, 0x00, 0x00
			}, aki2.Identifier, "Identifier");
		}

		[Test]
		[ExpectedException (typeof(InvalidOperationException))]
		public void EmptyIdentifier ()
		{
			AuthorityKeyIdentifierExtension ext = new AuthorityKeyIdentifierExtension ();
			AuthorityKeyIdentifierExtension aki = new AuthorityKeyIdentifierExtension (ext);
			Empty (aki);
		}
	}
}
