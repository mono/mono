// DbConnectionStringBuilderTest.cs - NUnit Test Cases for Testing the 
// DbConnectionStringBuilder class
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

#if NET_2_0

#region Using directives

using System;
using System.Text;

using System.Data;
using System.Reflection;
using System.Data.Common;
using System.ComponentModel;
using System.Data.SqlClient;
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

                [Test, Ignore ("FIXME : commented for a missing feature in gmcs, (CopyTo)")]
                public void IDictionaryCopyToTest ()
                {
                        KeyValuePair<string, object> [] dict = new KeyValuePair<string, object> [2];
                        builder.Add (SERVER, SERVER_VALUE);
                        builder.Add (SERVER + "1", SERVER_VALUE + "1");
                        IDictionary<string, object> s = builder;
                        //FIXME : s.CopyTo (dict, 0);
                        Assert.AreEqual (SERVER, dict [0].Key, "not equal");
                        Assert.AreEqual (SERVER_VALUE, dict [0].Value, "not equal");
                        Assert.AreEqual (SERVER + "1", dict [1].Key, "not equal");
                        Assert.AreEqual (SERVER_VALUE + "1", dict [1].Value, "not equal");
                }

                [Test, Ignore ("FIXME: commented for a missing feature in gmcs (CopyTo)")]
                [ExpectedException (typeof (ArgumentException))]
                public void NegIDictionaryCopyToTest ()
                {
                        KeyValuePair<string, object> [] dict = new KeyValuePair<string, object> [1];
                        builder.Add (SERVER, SERVER_VALUE);
                        builder.Add (SERVER + "1", SERVER_VALUE + "1");
                        IDictionary<string, object> s = builder;
                        //FIXME : s.CopyTo (dict, 0);
                        Assert.Fail ("Exception Destination Array not enough is not thrown!");
                }

                [Test, Ignore ("FIXME : currently mono is not supporting casting from generic type to"+
                 " non generic type")]
                public void ICollectionCopyToTest ()
                {

                        KeyValuePair <string, object> [] arr = new KeyValuePair <string, object> [2];
                        builder.Add (SERVER, SERVER_VALUE);
                        builder.Add (SERVER + "1", SERVER_VALUE + "1");
                        System.Collections.ICollection s = builder;
                        s.CopyTo ((Array) arr, 0);
                        Assert.AreEqual (SERVER, arr [0].Key, "not equal");
                        Assert.AreEqual (SERVER_VALUE, arr [0].Value, "not equal");
                        Assert.AreEqual (SERVER + "1", arr [1].Key, "not equal");
                        Assert.AreEqual (SERVER_VALUE + "1", arr [1].Value, "not equal");
                }

                [Test]
                [ExpectedException (typeof (ArgumentException))]
                public void NegICollectionCopyToTest ()
                {
                        string [] arr = new string [2];
                        builder.Add (SERVER, SERVER_VALUE);
                        builder.Add (SERVER + "1", SERVER_VALUE + "1");
                        System.Collections.ICollection s = builder;
                        s.CopyTo ((Array) arr, 0);
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
        }
}

#endif // NET_2_0
