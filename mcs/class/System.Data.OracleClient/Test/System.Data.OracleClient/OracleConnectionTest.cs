//
// OracleConnectionTest.cs - NUnit Test Cases for OracleConnection
//
// Author:
//      Gert Driesen  <drieseng@users.sourceforge.net>
//
// Copyright (C) 2008 Gert Driesen
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
using System.Data;
using System.Data.OracleClient;

using NUnit.Framework;

namespace MonoTests.System.Data.OracleClient
{
	[TestFixture]
	public class OracleConnectionTest
	{
		OracleConnection connection;

		[SetUp]
		public void SetUp ()
		{
			connection = new OracleConnection ();
		}

		[TearDown]
		public void TearDown ()
		{
			connection.Dispose ();
		}

		[Test]
		public void ConnectionString ()
		{
			connection.ConnectionString = "Data Source=Oracle8i;Integrated Security=yes";
			Assert.AreEqual ("Data Source=Oracle8i;Integrated Security=yes",
				connection.ConnectionString, "#1");
			connection.ConnectionString = null;
			Assert.AreEqual (string.Empty, connection.ConnectionString, "#2");
			connection.ConnectionString = "Data Source=Oracle8i;Integrated Security=yes";
			Assert.AreEqual ("Data Source=Oracle8i;Integrated Security=yes",
				connection.ConnectionString, "#3");
			connection.ConnectionString = string.Empty;
			Assert.AreEqual (string.Empty, connection.ConnectionString, "#3");
		}

#if NET_2_0
		[Test]
		public void ConnectionTimeout ()
		{
			OracleConnection connection = new OracleConnection ();
			Assert.AreEqual (0, connection.ConnectionTimeout, "#1");
			connection.ConnectionString = "Data Source=Oracle8i;Integrated Security=yes";
			Assert.AreEqual (0, connection.ConnectionTimeout, "#2");
		}
#endif

		[Test]
		public void ConnectionTimeout_IDbConnection ()
		{
			IDbConnection connection = new OracleConnection ();
			Assert.AreEqual (0, connection.ConnectionTimeout, "#1");
			connection.ConnectionString = "Data Source=Oracle8i;Integrated Security=yes";
			Assert.AreEqual (0, connection.ConnectionTimeout, "#2");
		}
	}
}
