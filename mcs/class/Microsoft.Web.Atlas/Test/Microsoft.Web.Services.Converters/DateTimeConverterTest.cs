//
// Tests for Microsoft.Web.Services.Converters.DateTimeConverter
//
// Author:
//	Chris Toshok (toshok@ximian.com)
//

//
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
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

#if NET_2_0

using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Web;
using Microsoft.Web.Services;
using Microsoft.Web.Services.Converters;

namespace MonoTests.Microsoft.Web.Services.Converters
{
	[TestFixture]
	public class DateTimeConverterTest
	{
		class Poker : DateTimeConverter
		{
			public Type[] GetSupportedTypes ()
			{
				return base.SupportedTypes;
			}

			public string ClientTypeName (Type serverType)
			{
				return base.GetClientTypeName (serverType);
			}

			public override object Deserialize (string s, Type t)
			{
				Console.WriteLine ("asked to deserialize '{0}', with type {1}", s, t);
				return base.Deserialize (s, t);
			}

			public void Init ()
			{
				base.Initialize ();
			}
		}

		[Test]
		public void SupportedTypes ()
		{
			Poker p = new Poker ();

			Type[] ts = p.GetSupportedTypes();

			Assert.AreEqual (1, ts.Length, "A1");
			Assert.AreEqual (typeof (DateTime), ts[0], "A2");
		}

		[Test]
		public void SerializeObject ()
		{
			Poker p = new Poker ();
			DateTime dt = new DateTime (2005, 9, 25, 22, 30, 45);
			string s = p.Serialize (dt);
			Assert.AreEqual ("new Date(2005,8,25,22,30,45)", s, "A1");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void Serialize_NonDateTime ()
		{
			Poker p = new Poker ();
			DateTime dt = new DateTime (2005, 9, 25);
			string s = p.Serialize ("hi");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void Serialize_Null ()
		{
			Poker p = new Poker ();
			DateTime dt = new DateTime (2005, 9, 25);
			string s = p.Serialize (null);
		}

		[Test]
		public void ClientTypeName ()
		{
			Poker p = new Poker ();
			Assert.AreEqual ("Date", p.ClientTypeName (typeof (DateTime)), "A1");
			Assert.AreEqual ("Date", p.ClientTypeName (typeof (string)), "A2");
			Assert.AreEqual ("Date", p.ClientTypeName (null), "A3");
		}

		[Test]
		public void Deserialize ()
		{
			Poker p = new Poker ();
			p.Init ();

			DateTime dt = new DateTime (2005, 9, 25, 22, 30, 45);
			string s = p.Serialize (dt);
			dt = (DateTime)p.Deserialize (s, typeof (DateTime));
			Assert.IsNotNull (dt, "A1");
		}
	}

}

#endif
