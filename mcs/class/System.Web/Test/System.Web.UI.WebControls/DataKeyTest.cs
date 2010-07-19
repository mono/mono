//
// Tests for System.Web.UI.WebControls.DataKeyTest.cs
//
// Author:
//	Yoni Klein (yonik@mainsoft.com)
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


#if NET_2_0

using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.IO;
using System.Collections;
using System.Collections.Specialized;
using NUnit.Framework;
using System.Data;
using System.ComponentModel;

namespace MonoTests.System.Web.UI.WebControls
{
	[TestFixture]
	public class DataKeyTest
	{
		// Keys and values can be assigned only from constractor 

		[Test]
		public void DataKey_Functionality()
		{
			OrderedDictionary  dictionary = new OrderedDictionary ();
			IOrderedDictionary iDictionary;
			dictionary.Add ("key", "value");
			DataKey key = new DataKey (dictionary);
			Assert.AreEqual ("value", key[0].ToString(), "DataKeyItemIndex");
			Assert.AreEqual ("value", key["key"].ToString (), "DataKeyItemKeyName");
			Assert.AreEqual ("value", key.Value, "FirstIndexValue");
			iDictionary = key.Values;
			Assert.AreEqual (1, iDictionary.Count, "AllValuesReferringToKey");
			Assert.AreEqual ("value", iDictionary[0], "ValueReferringToKey");
			dictionary.Add("key1", "value1");
			key = new DataKey (dictionary);
			iDictionary = key.Values;
			Assert.AreEqual (2, iDictionary.Count, "AllValuesReferringToKey#1");
			Assert.AreEqual ("value1", iDictionary[1], "ValueReferringToKey#1");
		}
#if NET_4_0
		[Test]
		public void DataKey_Equals ()
		{
			var dict = new OrderedDictionary ();
			dict.Add ("key", "value");
			dict.Add ("key1", "value1");
			var key1 = new DataKey (dict);

			dict = new OrderedDictionary ();
			dict.Add ("key", "value");
			dict.Add ("key1", "value1");
			var key2 = new DataKey (dict);

			Assert.IsTrue (key1.Equals (key2), "#A1-1");
			Assert.IsTrue (key2.Equals (key1), "#A1-2");

			dict = new OrderedDictionary ();
			dict.Add ("key", "value");
			dict.Add ("key1", "value1");
			key1 = new DataKey (dict);

			dict = new OrderedDictionary ();
			dict.Add ("key2", "value2");
			dict.Add ("key1", "value1");
			key2 = new DataKey (dict);

			Assert.IsFalse (key1.Equals (key2), "#A2-1");
			Assert.IsFalse (key2.Equals (key1), "#A2-2");

			dict = new OrderedDictionary ();
			key1 = new DataKey (dict);

			dict = new OrderedDictionary ();
			dict.Add ("key1", "value1");
			key2 = new DataKey (dict);

			Assert.IsFalse (key1.Equals (key2), "#A3-1");
			Assert.IsFalse (key2.Equals (key1), "#A3-2");

			dict = new OrderedDictionary ();
			key1 = new DataKey (dict);

			dict = new OrderedDictionary ();
			key2 = new DataKey (dict);
			Assert.IsTrue (key1.Equals (key2), "#A4-1");
			Assert.IsTrue (key2.Equals (key1), "#A4-2");

			dict = new OrderedDictionary ();
			key1 = new DataKey (null);
			key2 = new DataKey (dict);
			Assert.IsTrue (key1.Equals (key2), "#A5-1");
			// Throws NREX on .NET
			//Assert.IsTrue (key2.Equals (key1), "#A5-2");

			key1 = new DataKey (null);
			key2 = new DataKey (null);
			Assert.IsTrue (key1.Equals (key2), "#A6-1");
			Assert.IsTrue (key2.Equals (key1), "#A6-2");

			dict = new OrderedDictionary ();
			dict.Add ("key", "value");
			dict.Add ("key1", "value1");
			key1 = new DataKey (dict, new string[] { "key" });

			dict = new OrderedDictionary ();
			dict.Add ("key", "value");
			dict.Add ("key1", "value1");
			key2 = new DataKey (dict, new string[] { "key1" });
			Assert.IsFalse (key1.Equals (key2), "#A7-1");
			Assert.IsFalse (key2.Equals (key1), "#A7-2");

			Assert.IsFalse (key1.Equals ((DataKey) null), "#A8");

			dict = new OrderedDictionary ();
			dict.Add ("key", "value");
			dict.Add ("key1", "value1");
			key1 = new DataKey (dict);			
			key2 = new DataKey (null);
			// Throws NREX on .NET
			//Assert.IsFalse (key1.Equals (key2), "#A8-1");
			Assert.IsTrue (key2.Equals (key1), "#A8-2");

			key1 = new DataKey (null);
			Assert.IsFalse (key1.Equals ((DataKey) null), "#A9");

			dict = new OrderedDictionary ();
			key1 = new DataKey (dict, new string [] { "key" });

			dict = new OrderedDictionary ();
			key2 = new DataKey (dict, new string [] { "key1" });
			Assert.IsFalse (key1.Equals (key2), "#A10-1");
			Assert.IsFalse (key2.Equals (key1), "#A10-2");

			dict = new OrderedDictionary ();
			dict.Add ("KEY", "value");
			dict.Add ("key1", "value1");
			key1 = new DataKey (dict);

			dict = new OrderedDictionary ();
			dict.Add ("key", "value");
			dict.Add ("key1", "value1");
			key2 = new DataKey (dict);

			Assert.IsFalse (key1.Equals (key2), "#A11-1");
			Assert.IsFalse (key2.Equals (key1), "#A11-2");

			dict = new OrderedDictionary ();
			dict.Add ("key", "VALUE");
			dict.Add ("key1", "value1");
			key1 = new DataKey (dict);

			dict = new OrderedDictionary ();
			dict.Add ("key", "value");
			dict.Add ("key1", "value1");
			key2 = new DataKey (dict);

			Assert.IsFalse (key1.Equals (key2), "#A12-1");
			Assert.IsFalse (key2.Equals (key1), "#A12-2");

			dict = new OrderedDictionary ();
			dict.Add ("key", "value");
			dict.Add ("key1", "value1");
			key1 = new DataKey (dict);

			dict = new OrderedDictionary ();
			dict.Add ("key", "value");
			dict.Add ("key1", "value1");
			key2 = new DataKey (dict, new string [] { "key1" });
			Assert.IsFalse (key1.Equals (key2), "#A13-1");
			Assert.IsFalse (key2.Equals (key1), "#A13-2");

			dict = new OrderedDictionary ();
			key1 = new DataKey (dict, new string [] { "key" });

			dict = new OrderedDictionary ();
			key2 = new DataKey (dict, new string [] { "KEY" });
			Assert.IsFalse (key1.Equals (key2), "#A14-1");
			Assert.IsFalse (key2.Equals (key1), "#A14-2");
			
			key1 = new DataKey (null, new string [] { "key" });
			key2 = new DataKey (null, new string [] { "key" });
			Assert.IsTrue (key1.Equals (key2), "#A15-1");
			Assert.IsTrue (key2.Equals (key1), "#A15-2");

			key1 = new DataKey (null, new string [] { "KEY" });
			key2 = new DataKey (null, new string [] { "key" });
			Assert.IsFalse (key1.Equals (key2), "#A16-1");
			Assert.IsFalse (key2.Equals (key1), "#A16-2");

			dict = new OrderedDictionary ();
			dict.Add ("key", "value");
			dict.Add ("key1", "value1");
			key2 = new DataKey (dict, new string [] { });

			dict = new OrderedDictionary ();
			dict.Add ("key", "value");
			dict.Add ("key1", "value1");
			key1 = new DataKey (dict);
			Assert.IsFalse (key1.Equals (key2), "#A17-1");
			Assert.IsFalse (key2.Equals (key1), "#A17-2");
		}
#endif
	}
}
#endif