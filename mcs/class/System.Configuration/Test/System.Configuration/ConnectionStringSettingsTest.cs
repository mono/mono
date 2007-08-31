//
// System.Configuration.ConnectionStringSettingsTest.cs - Unit tests
// for System.Configuration.ConnectionStringSettings
//
// Author:
//	Chris Toshok  <toshok@ximian.com>
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

using System;
using System.Configuration;
using NUnit.Framework;

namespace MonoTests.System.Configuration
{
	[TestFixture]
	public class ConnectionStringSettingsTest
	{
		[Test]
		public void Defaults ()
		{
			ConnectionStringSettings s;

			s = new ConnectionStringSettings ();

			Assert.AreEqual (null, s.Name, "A1");
			Assert.AreEqual ("", s.ProviderName, "A2");
			Assert.AreEqual ("", s.ConnectionString, "A3");

			s = new ConnectionStringSettings ("name", "connectionString");
			Assert.AreEqual ("name", s.Name, "A4");
			Assert.AreEqual ("", s.ProviderName, "A5");
			Assert.AreEqual ("connectionString", s.ConnectionString, "A6");

			s = new ConnectionStringSettings ("name", "connectionString", "provider");
			Assert.AreEqual ("name", s.Name, "A7");
			Assert.AreEqual ("provider", s.ProviderName, "A8");
			Assert.AreEqual ("connectionString", s.ConnectionString, "A9");
		}

		[Test]
		public void NameNull ()
		{
			ConnectionStringSettings s;

			s = new ConnectionStringSettings ("name", "connectionString", "provider");
			Assert.AreEqual ("name", s.Name, "A1");
			s.Name = null;
			Assert.IsNull (s.Name, "A2");
		}

		[Test]
		[ExpectedException (typeof(ConfigurationErrorsException))]
		[Category ("NotWorking")]
		public void Validators_Name1 ()
		{
			ConnectionStringSettings s = new ConnectionStringSettings ();
			s.Name = "";
		}

		[Test]
		public void Validators_Name2 ()
		{
			ConnectionStringSettings s = new ConnectionStringSettings ();
 			s.Name = null;
		}

		[Test]
		public void Validators_ProviderName1 ()
		{
			ConnectionStringSettings s = new ConnectionStringSettings ();
			s.ProviderName = "";
		}

		[Test]
		public void Validators_ProviderName2 ()
		{
			ConnectionStringSettings s = new ConnectionStringSettings ();
			s.ProviderName = null;
		}

		[Test]
		public void Validators_ConnectionString1 ()
		{
			ConnectionStringSettings s = new ConnectionStringSettings ();
			s.ConnectionString = "";
		}

		[Test]
		public void Validators_ConnectionString2 ()
		{
			ConnectionStringSettings s = new ConnectionStringSettings ();
			s.ConnectionString = null;
		}

		[Test]
		public void ToStringTest ()
		{
			ConnectionStringSettings s = new ConnectionStringSettings (
				"name", "connectionString", "provider");
			Assert.AreEqual ("connectionString", s.ToString(), "A1");
		}
	}
}

#endif
