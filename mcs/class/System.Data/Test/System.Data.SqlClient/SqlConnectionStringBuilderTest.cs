// SqlConnectionStringBuilderTest.cs - NUnit Test Cases for Testing the 
// SqlConnectionStringBuilder class
//
// Author: 
//      Sureshkumar T (tsureshkumar@novell.com)
//
//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
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


#region Using directives

using System;
using System.Text;
using System.Collections;

using System.Data;
using System.Data.SqlClient;

using NUnit.Framework;

#endregion

namespace MonoTests.System.Data.Common
{

        [TestFixture]
        public class SqlConnectionStringBuilderTest
        {
		private SqlConnectionStringBuilder builder = null;
		
		[Test]
		public void DefaultValuestTest ()
		{
			builder = new SqlConnectionStringBuilder ();
			Assert.AreEqual ("", builder.ConnectionString, "#DV1 default values is wrong");
		}

		[Test]
		public void DefaultValuestTest2 ()
		{
			builder = new SqlConnectionStringBuilder ("SERVER=localhost;");
			Assert.AreEqual ("Data Source=localhost", builder.ConnectionString, "#DVT1 default values is wrong");
		}

		[Test]
		[Category("NotWorking")] // https://github.com/dotnet/corefx/issues/22474
		public void PropertiesTest ()
		{
			builder = new SqlConnectionStringBuilder ("SERVER=localhost;");
			builder.AsynchronousProcessing = true;
			builder.ApplicationName = "mono test";
			Assert.AreEqual (true, 
					 builder.ConnectionString.Contains ("Asynchronous Processing=True"),
					 "#PT1 boolean value must be true");
		}
		
		[Test]
		[Category("NotWorking")] // https://github.com/dotnet/corefx/issues/22474
		public void ItemTest ()
		{
			builder = new SqlConnectionStringBuilder ("SERVER=localhost;");
			builder ["Network Library"] = "DBMSSOCN";
			Assert.AreEqual (true, 
					 builder.ConnectionString.Contains ("Network Library=dbmssocn"),
					 "#PT1 network library should exist");
		}

		public void NullTest ()
		{
			builder = new SqlConnectionStringBuilder ("SERVER=localhost;Network=DBMSSOCN");
			builder ["Network Library"] = null;
			Assert.AreEqual ("Data Source=localhost", builder.ConnectionString,
					 "#NT1 should remove the key if set with null");
		}

		public void ContainsKeyTest ()
		{
			builder = new SqlConnectionStringBuilder ("SERVER=localhost;Network=DBMSSOCN");
			Assert.AreEqual (true, builder.ContainsKey ("NETWORK"),
					 "#CKT1 should say true");
			Assert.AreEqual (false, builder.ContainsKey ("ABCD"),
					 "#CKT2 should say false");
		}
		
		[Test, ExpectedException (typeof (ArgumentException))]
		[Category("NotWorking")] // https://github.com/dotnet/corefx/issues/22474
		public void InvalidKeyTest ()
		{
			builder = new SqlConnectionStringBuilder ("SERVER=localhost;Network=DBMSSOCN");
			int value = (int) builder ["ABCD"];
			value++; // to avoid warning
		}

		[Test]
		[Category("NotWorking")] // https://github.com/dotnet/corefx/issues/22474
		public void RemoveTest ()
		{
			builder = new SqlConnectionStringBuilder ("SERVER = localhost ;Network=DBMSSOCN");
			// non existing key
			Assert.AreEqual (false, builder.Remove ("ABCD"),
					 "#RT1 cannot remove non existant key");
			Assert.AreEqual (true, builder.Remove ("NETWORK library"),
					 "#RT2 should remove the key");
			Assert.AreEqual ("Data Source=localhost", builder.ConnectionString,
					 "#RT3 should have removed the key");
		}
		
	}
}

