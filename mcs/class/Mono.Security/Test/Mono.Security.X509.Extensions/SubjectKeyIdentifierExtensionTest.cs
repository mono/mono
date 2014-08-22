//
// SubjectKeyIdentifierExtensionTest.cs - NUnit Test Cases for 
//	Mono.Security.X509.Extensions.SubjectKeyIdentifierExtension
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
	public class SubjectKeyIdentifierExtensionTest
	{
		private void Empty (SubjectKeyIdentifierExtension ski)
		{
			Assert.IsFalse (ski.Critical, "Critical");
			Assert.AreEqual ("2.5.29.14", ski.Oid, "Oid");
			Assert.IsNotNull (ski.Name, "Name");
			Assert.IsFalse (ski.Name == ski.Oid, "Name!=Oid");
			Assert.AreEqual (new byte[] {
				0x00, 0x00, 0x00, 0x00, 0x00,
				0x00, 0x00, 0x00, 0x00, 0x00,
				0x00, 0x00, 0x00, 0x00, 0x00,
				0x00, 0x00, 0x00, 0x00, 0x00
			}, ski.Identifier, "Identifier");
		}

		[Test]
		public void Constructor_Empty ()
		{
			SubjectKeyIdentifierExtension ski = new SubjectKeyIdentifierExtension ();
			ski.Identifier = new byte[] {
				0x00, 0x00, 0x00, 0x00, 0x00,
				0x00, 0x00, 0x00, 0x00, 0x00,
				0x00, 0x00, 0x00, 0x00, 0x00,
				0x00, 0x00, 0x00, 0x00, 0x00
			};
			Empty (ski);
		}

		[Test]
		public void Constructor_Extension ()
		{
			SubjectKeyIdentifierExtension ext = new SubjectKeyIdentifierExtension ();
			ext.Identifier = new byte[] {
				0x00, 0x00, 0x00, 0x00, 0x00,
				0x00, 0x00, 0x00, 0x00, 0x00,
				0x00, 0x00, 0x00, 0x00, 0x00,
				0x00, 0x00, 0x00, 0x00, 0x00
			};
			SubjectKeyIdentifierExtension ski = new SubjectKeyIdentifierExtension (ext);
			Empty (ski);
		}

		[Test]
		public void Constructor_ASN1 ()
		{
			SubjectKeyIdentifierExtension ext = new SubjectKeyIdentifierExtension ();
			ext.Identifier = new byte[] {
				0x00, 0x00, 0x00, 0x00, 0x00,
				0x00, 0x00, 0x00, 0x00, 0x00,
				0x00, 0x00, 0x00, 0x00, 0x00,
				0x00, 0x00, 0x00, 0x00, 0x00
			};
			SubjectKeyIdentifierExtension ski = new SubjectKeyIdentifierExtension (ext.ASN1);
			Empty (ski);
		}

		[Test]
		public void AuthorityKeyIdentifier_Critical ()
		{
			SubjectKeyIdentifierExtension ski = new SubjectKeyIdentifierExtension ();
			ski.Critical = true;
			ski.Identifier = new byte[] {
				0x00, 0x00, 0x00, 0x00, 0x00,
				0x00, 0x00, 0x00, 0x00, 0x00,
				0x00, 0x00, 0x00, 0x00, 0x00,
				0x00, 0x00, 0x00, 0x00, 0x00
			};
			Assert.AreEqual ("30-20-06-03-55-1D-0E-01-01-FF-04-16-04-14-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00", BitConverter.ToString (ski.GetBytes ()), "GetBytes");

			SubjectKeyIdentifierExtension ski2 = new SubjectKeyIdentifierExtension (ski.ASN1);
			Assert.IsTrue (ski2.Critical, "Critical");
			Assert.AreEqual (new byte[] {
				0x00, 0x00, 0x00, 0x00, 0x00,
				0x00, 0x00, 0x00, 0x00, 0x00,
				0x00, 0x00, 0x00, 0x00, 0x00,
				0x00, 0x00, 0x00, 0x00, 0x00
			}, ski2.Identifier, "Identifier");
		}

		[Test]
		[ExpectedException (typeof(InvalidOperationException))]
		public void EmptyIdentifier ()
		{
			SubjectKeyIdentifierExtension ext = new SubjectKeyIdentifierExtension ();
			SubjectKeyIdentifierExtension ski = new SubjectKeyIdentifierExtension (ext);
			Empty (ski);
		}
	}
}
