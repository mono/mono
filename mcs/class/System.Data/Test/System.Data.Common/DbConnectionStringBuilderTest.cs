// DbConnectionStringBuilderTest.cs - NUnit Test Cases for Testing the 
// DbConnectionStringBuilder class
//
// Author: 
//      Sureshkumar T (tsureshkumar@novell.com)
//	Daniel Morgan (monodanmorg@yahoo.com)
//
//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
// Copyright (C) 2008 Daniel Morgan
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

#region Using directives

using System;
using System.Text;

using System.Data;
using System.Reflection;
using System.Data.Common;
using System.ComponentModel;
using System.Data.SqlClient;
using System.Collections;
using System.Collections.Specialized;
using System.Collections.Generic;

using NUnit.Framework;

#endregion

namespace MonoTests.System.Data.Common
{

        [TestFixture]
        public class DbConnectionStringBuilderTest
        {
                private DbConnectionStringBuilder builder = null;
                private const string SERVER = "SERVER";
                private const string SERVER_VALUE = "localhost";

                [SetUp]
                public void SetUp ()
                {
                        builder = new DbConnectionStringBuilder ();
                }

                [Test]
                public void AddTest ()
                {
                        builder.Add (SERVER, SERVER_VALUE);
                        Assert.AreEqual (SERVER + "=" + SERVER_VALUE, builder.ConnectionString,
                                         "Adding to connection String failed!");
                }

                [Test]
                public void ClearTest ()
                {
                        builder.Add (SERVER, SERVER_VALUE);
                        builder.Clear ();
                        Assert.AreEqual ("", builder.ConnectionString,
                                         "Clearing connection String failed!");
                }

                [Test]
                public void AddDuplicateTest ()
                {
                        builder.Add (SERVER, SERVER_VALUE);
                        builder.Add (SERVER, SERVER_VALUE);
                        // should allow duplicate addition. rather, it should re-assign
                        Assert.AreEqual (SERVER + "=" + SERVER_VALUE, builder.ConnectionString,
                                         "Duplicates addition does not change the value!");
                }

                [Test]
                [ExpectedException (typeof (ArgumentException))]
                public void InvalidKeyTest ()
                {
                        builder.Add (SERVER, SERVER_VALUE);
                        string value = builder ["###"].ToString (); // some invalid key values
                        Assert.Fail ("Should have thrown exception!");
                }

                [Test]
                public void RemoveTest ()
                {
                        builder.Add (SERVER, SERVER_VALUE);
                        builder.Remove (SERVER);
                        Assert.AreEqual ("", builder.ConnectionString, "Remove does not work!");
                }

                [Test]
                public void ContainsKeyTest ()
                {
                        builder.Add (SERVER, SERVER_VALUE);
                        bool value = builder.ContainsKey (SERVER);
                        Assert.IsTrue (value, "Contains does not work!");
                }

                [Test]
                public void EquivalentToTest ()
                {
                        builder.Add (SERVER, SERVER_VALUE);
                        DbConnectionStringBuilder sb2 = new DbConnectionStringBuilder ();
                        sb2.Add (SERVER, SERVER_VALUE);
                        bool value = builder.EquivalentTo (sb2);
                        Assert.IsTrue (value, "builder comparision does not work!");

                        // negative tests
                        sb2.Add (SERVER + "1", SERVER_VALUE);
                        value = builder.EquivalentTo (sb2);
                        Assert.IsFalse (value, "builder comparision does not work for not equivalent strings!");
                }

                [Test]
                public void AppendKeyValuePairTest ()
                {
                        StringBuilder sb = new StringBuilder ();
                        DbConnectionStringBuilder.AppendKeyValuePair (sb, SERVER, SERVER_VALUE);
                        Assert.AreEqual (SERVER + "=" + SERVER_VALUE, sb.ToString (),
                                         "adding key value pair to existing string builder fails!");
                }

                [Test]
                public void ToStringTest ()
                {
                        builder.Add (SERVER, SERVER_VALUE);
                        string str = builder.ToString ();
                        string value = builder.ConnectionString;
                        Assert.AreEqual (value, str,
                                         "ToString shoud return ConnectionString!");
                }

                [Test]
                public void ItemTest ()
                {
                        builder.Add (SERVER, SERVER_VALUE);
                        string value = (string) builder [SERVER];
                        Assert.AreEqual (SERVER_VALUE, value,
                                         "Item indexor does not retrun correct value!");
                }

                [Test]
                public void ICollectionCopyToTest ()
                {
                        KeyValuePair<string, object> [] dict = new KeyValuePair<string, object> [2];
                        builder.Add (SERVER, SERVER_VALUE);
                        builder.Add (SERVER + "1", SERVER_VALUE + "1");

			int i = 0;
			int j = 1;
			((ICollection) builder).CopyTo (dict, 0);
                        Assert.AreEqual (SERVER, dict [i].Key, "not equal");
                        Assert.AreEqual (SERVER_VALUE, dict [i].Value, "not equal");
                        Assert.AreEqual (SERVER + "1", dict [j].Key, "not equal");
                        Assert.AreEqual (SERVER_VALUE + "1", dict [j].Value, "not equal");
                }

                [Test]
                [ExpectedException (typeof (ArgumentException))]
                public void NegICollectionCopyToTest ()
                {
                        KeyValuePair<string, object> [] dict = new KeyValuePair<string, object> [1];
                        builder.Add (SERVER, SERVER_VALUE);
                        builder.Add (SERVER + "1", SERVER_VALUE + "1");
			((ICollection) builder).CopyTo (dict, 0);
                        Assert.Fail ("Exception Destination Array not enough is not thrown!");
                }

                [Test]
                public void TryGetValueTest ()
                {
                        builder.Add (SERVER, SERVER_VALUE);
                        object value = "";
                        bool result = builder.TryGetValue (SERVER, out value);
                        Assert.AreEqual (SERVER_VALUE, (string) value,
                                         "TryGetValue does not return correct value in out parameter!");
                        Assert.IsTrue (result, "TryGetValue does not return true for existant key!");

                        result = builder.TryGetValue ("@@@@", out value);
                        Assert.IsFalse (result, "TryGetValue does not return false for non-existant key!");
                        Assert.IsNull ((string) value,
                                       "TryGetValue does not return correct value in out parameter for non existant key!");
                }

                [Test]
                public void ICTD_GetClassNameTest ()
                {
                        ICustomTypeDescriptor ictd = (ICustomTypeDescriptor) builder;
                        string className = ictd.GetClassName ();
                        Assert.AreEqual (builder.GetType ().ToString (), className, "Should return class name!");

                        AttributeCollection collection = ictd.GetAttributes ();
                        Assert.AreEqual (2, collection.Count);
                        object [] attr = builder.GetType ().GetCustomAttributes (typeof (DefaultMemberAttribute), false);
                        if (attr.Length > 0) {
                                DefaultMemberAttribute defAtt = (DefaultMemberAttribute) attr [0];
                                Assert.AreEqual ("Item", defAtt.MemberName, "default memeber attribute is not set!");
                        } else
                                Assert.Fail ("DbConnectionStringBuilder class does not implement DefaultMember attribute");

                        string compName = ictd.GetComponentName ();
                        Assert.IsNull (compName, "");

                        TypeConverter converter = ictd.GetConverter ();
                        Assert.AreEqual (typeof (CollectionConverter), converter.GetType (), "");

                        EventDescriptor evtDesc = ictd.GetDefaultEvent ();
                        Assert.IsNull (evtDesc, "");

                        PropertyDescriptor property = ictd.GetDefaultProperty ();
                        Assert.IsNull (property, "");

                }

		[Test]
		public void EmbeddedCharTest1 ()
		{
			// Notice how the keywords show up in the connection string
			//  in the order they were added.
			// And notice the case of the keyword when added is preserved
			//  in the connection string.

			DbConnectionStringBuilder sb = new DbConnectionStringBuilder ();

			sb["Data Source"] = "testdb";
			sb["User ID"] = "someuser";
			sb["Password"] = "abcdef";
			Assert.AreEqual ("Data Source=testdb;User ID=someuser;Password=abcdef", 
				sb.ConnectionString, "cs#1");

			sb["Password"] = "abcdef#";
			Assert.AreEqual ("Data Source=testdb;User ID=someuser;Password=abcdef#", 
				sb.ConnectionString, "cs#2");

			// an embedded single-quote value will result in the value being delimieted with double quotes
			sb["Password"] = "abc\'def";
			Assert.AreEqual ("Data Source=testdb;User ID=someuser;Password=\"abc\'def\"", 
				sb.ConnectionString, "cs#3");

			// an embedded double-quote value will result in the value being delimieted with single quotes
			sb["Password"] = "abc\"def";
			Assert.AreEqual ("Data Source=testdb;User ID=someuser;Password=\'abc\"def\'", 
				sb.ConnectionString, "cs#4");

			// an embedded single-quote and double-quote in the value
			// will result in the value being delimited by double-quotes
			// with the embedded double quote being escaped with two double-quotes
			sb["Password"] = "abc\"d\'ef";
			Assert.AreEqual ("Data Source=testdb;User ID=someuser;Password=\"abc\"\"d\'ef\"", 
				sb.ConnectionString, "cs#5");

			sb = new DbConnectionStringBuilder ();
			sb["PASSWORD"] = "abcdef1";
			sb["user id"] = "someuser";
			sb["Data Source"] = "testdb";
			Assert.AreEqual ("PASSWORD=abcdef1;user id=someuser;Data Source=testdb", 
				sb.ConnectionString, "cs#6");

			// case is preserved for a keyword that was added the first time
			sb = new DbConnectionStringBuilder ();
			sb["PassWord"] = "abcdef2";
			sb["uSER iD"] = "someuser";
			sb["DaTa SoUrCe"] = "testdb";
			Assert.AreEqual ("PassWord=abcdef2;uSER iD=someuser;DaTa SoUrCe=testdb", 
				sb.ConnectionString, "cs#7");
			sb["passWORD"] = "abc123";
			Assert.AreEqual ("PassWord=abc123;uSER iD=someuser;DaTa SoUrCe=testdb", 
				sb.ConnectionString, "cs#8");

			// embedded equal sign in the value will cause the value to be
			// delimited with double-quotes
			sb = new DbConnectionStringBuilder ();
			sb["Password"] = "abc=def";
			sb["Data Source"] = "testdb";
			sb["User ID"] = "someuser";
			Assert.AreEqual ("Password=\"abc=def\";Data Source=testdb;User ID=someuser", 
				sb.ConnectionString, "cs#9");

			// embedded semicolon in the value will cause the value to be
			// delimited with double-quotes
			sb = new DbConnectionStringBuilder ();
			sb["Password"] = "abc;def";
			sb["Data Source"] = "testdb";
			sb["User ID"] = "someuser";
			Assert.AreEqual ("Password=\"abc;def\";Data Source=testdb;User ID=someuser", 
				sb.ConnectionString, "cs#10");

			// more right parentheses then left parentheses - happily takes it
			sb = new DbConnectionStringBuilder();
			sb.ConnectionString = "Data Source=(((Blah=Something))))))";
			Assert.AreEqual ("data source=\"(((Blah=Something))))))\"", 
				sb.ConnectionString, "cs#11");

			// more left curly braces then right curly braces - happily takes it
			sb = new DbConnectionStringBuilder();
			sb.ConnectionString = "Data Source={{{{Blah=Something}}";
			Assert.AreEqual ("data source=\"{{{{Blah=Something}}\"", 
				sb.ConnectionString, "cs#12");

			// spaces, empty string, null are treated like an empty string
			// and any previous settings is cleared
			sb.ConnectionString = "   ";
			Assert.AreEqual (String.Empty, 
				sb.ConnectionString, "cs#13");

			sb.ConnectionString = " ";
			Assert.AreEqual (String.Empty, 
				sb.ConnectionString, "cs#14");

			sb.ConnectionString = "";
			Assert.AreEqual (String.Empty, 
				sb.ConnectionString, "cs#15");

			sb.ConnectionString = String.Empty;
			Assert.AreEqual (String.Empty, 
				sb.ConnectionString, "cs#16");

			sb.ConnectionString = null;
			Assert.AreEqual (String.Empty, 
				sb.ConnectionString, "cs#17");

			sb = new DbConnectionStringBuilder();
			Assert.AreEqual (String.Empty, 
				sb.ConnectionString, "cs#18");
		}

		[Test]
		public void EmbeddedCharTest2 ()
		{
			DbConnectionStringBuilder sb = new DbConnectionStringBuilder();

			// Odbc connection string can have a Driver keyword where the value is enclosed with { }
			// the result has the value delimited with double-quotes and the keywords are lower case

			sb.ConnectionString = "Driver={SQL Server};Server=(local);Trusted_Connection=Yes;" +
				"Database=AdventureWorks;";
			Assert.AreEqual ("{SQL Server}", 
				sb["Driver"], "csp#1");
			Assert.AreEqual ("(local)", 
				sb["Server"], "csp#2");
			Assert.AreEqual ("Yes", 
				sb["Trusted_Connection"], "csp#3");
			Assert.AreEqual ( 
				"driver=\"{SQL Server}\";server=(local);trusted_connection=Yes;" +
					"database=AdventureWorks", 
				sb.ConnectionString, "csp#4");
		}

		[Test]
		public void EmbeddedCharTest3 ()
		{
			DbConnectionStringBuilder sb = new DbConnectionStringBuilder();

			// an oracle connection string which uses a TNS network description
			// which has parentheses 
			// the result has the data source value delimited with double-quotes
			// yet the keywords are all lower case

			string dataSource = "(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST=192.168.1.101)" + 
				"(PORT=1521))(CONNECT_DATA=(SERVER=DEDICATED)(SERVICE_NAME=TESTDB)))";
			sb.ConnectionString = "User ID=SCOTT;Password=TIGER;Data Source=" + dataSource;
			Assert.AreEqual (dataSource,
				sb["Data Source"], "csp#5");
			Assert.AreEqual ("SCOTT", 
				sb["User ID"], "csp#6");
			Assert.AreEqual ("TIGER", 
				sb["Password"], "csp#7"); 
			sb["Password"] = "secret";
			Assert.AreEqual ( 
				"user id=SCOTT;password=secret;data source=\"(DESCRIPTION=(ADDRESS=(PROTOCOL=" +
				"TCP)(HOST=192.168.1.101)(PORT=1521))(CONNECT_DATA=(SERVER=DEDICATED)" +
				"(SERVICE_NAME=TESTDB)))\"", 
				sb.ConnectionString, "csp#8");
		}

		[Test]
		public void EmbeddedCharTest4 ()
		{
			// Notice how the keywords parsed from the setting of the set property ConnectionString
			// are lower case while the keyword added aftwareds has the case preserved

			DbConnectionStringBuilder sb = new DbConnectionStringBuilder();
			sb.ConnectionString = "PassWord=abcdef2;uSER iD=someuser;DaTa SoUrCe=testdb";
			sb["Integrated Security"] = "False";
			Assert.AreEqual ( 
				"password=abcdef2;user id=someuser;data source=testdb;Integrated Security=False",
				sb.ConnectionString, "csp#9");
		}
        }
}

#endif // NET_2_0
