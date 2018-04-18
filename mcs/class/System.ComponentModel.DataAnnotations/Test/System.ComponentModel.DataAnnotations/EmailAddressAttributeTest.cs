//
// EmailAddressAttributeTest.cs
//
// Authors:
//      Pablo Ruiz García <pablo.ruiz@gmail.com>
//
// Copyright (C) 2010 Novell, Inc. (http://novell.com/)
// Copyright (C) 2013 Pablo Ruiz García
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
using System.ComponentModel.DataAnnotations;
using System.Text;

using NUnit.Framework;

namespace MonoTests.System.ComponentModel.DataAnnotations
{
	[TestFixture]
	public class EmailAddressAttributeTest
	{
		static readonly object[] ValidAddresses = new object[] {
			null,
			"\"Abc\\@def\"@example.com",
			"\"Fred Bloggs\"@example.com",
			"\"Joe\\\\Blow\"@example.com",
			"\"Abc@def\"@example.com",
			"customer/department=shipping@example.com",
			"$A12345@example.com",
			"!def!xyz%abc@example.com",
			"_somename@example.com",
		};

		static readonly object[] InvalidAddresses = new object[] {
			"",
			123,
			DateTime.Now,
			"invalid",
			"invalid@",
			"invalid @",
			"invalid@[555.666.777.888]",
			"invalid@[IPv6:123456]",
			"invalid@[127.0.0.1.]",
			"invalid@[127.0.0.1].",
			"invalid@[127.0.0.1]x",

			"valid.ipv4.addr@[123.1.72.10]",
			"valid.ipv6.addr@[IPv6:0::1]",
			"valid.ipv6.addr@[IPv6:2607:f0d0:1002:51::4]",
			"valid.ipv6.addr@[IPv6:fe80::230:48ff:fe33:bc33]",
			"valid.ipv6v4.addr@[IPv6:aaaa:aaaa:aaaa:aaaa:aaaa:aaaa:127.0.0.1]",
		};

		[Test]
		public void IsValid ()
		{
			var sla = new EmailAddressAttribute ();

			for (int i = 0; i < ValidAddresses.Length; i++)
				Assert.IsTrue (sla.IsValid (ValidAddresses[i]), "#A1-{0}", i);

			for (int i = 0; i < InvalidAddresses.Length; i++)
				Assert.IsFalse (sla.IsValid (InvalidAddresses[i]), "#B1-{0}", i);
		}
	}
}
