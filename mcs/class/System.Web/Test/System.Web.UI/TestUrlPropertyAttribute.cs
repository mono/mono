//
// Tests for System.Web.UI.UrlPropertyAttribute.cs 
//
// Author:
//	Sanjay Gupta (gsanjay@novell.com)
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

using NUnit.Framework;
using System;
using System.Web;
using System.Web.UI;
using System.Collections;
using System.ComponentModel;
using System.IO;

namespace MonoTests.System.Web.UI
{
	[TestFixture]	
	public class UrlPropertyAttributeTest 
	{
		UrlPropertyAttribute upa;
		UrlPropertyAttribute upa1;
		UrlPropertyAttribute upa2;
		string filter;
		UrlTypes urlTypes;

		[TearDown]
		public void TearDown () {}

		[SetUp]
		public void SetUp ()		
		{
			filter = "filter";
			urlTypes = UrlTypes.DocRelative;
			upa = new UrlPropertyAttribute ();
			upa1 = new UrlPropertyAttribute (filter);
			upa2 = new UrlPropertyAttribute (filter, urlTypes);
		}

		[Test]
		public void TestFilter ()
		{
			Assert.AreEqual (upa.Filter, "*.*", "Filter#1");
			Assert.AreEqual (upa1.Filter, filter, "Filter#2");
			Assert.AreEqual (upa2.Filter, filter, "Filter#3");
		}

		[Test]
		public void TestAllowedTypes ()
		{
			UrlTypes types = UrlTypes.Absolute |
					UrlTypes.AppRelative |
					UrlTypes.DocRelative |
					UrlTypes.RootRelative;

			Assert.AreEqual (upa.AllowedTypes, types, "Types#1");
			Assert.AreEqual (upa1.AllowedTypes, types, "Types#2");
			Assert.AreEqual (upa2.AllowedTypes, urlTypes, "Types#3");
		}

		[Test]
		public void TestGetHashCode ()
		{
			string filter1 = "*.*";
			Assert.AreEqual (upa.GetHashCode (), filter1.GetHashCode (), "GHC#1");
			Assert.AreEqual (upa1.GetHashCode (), filter.GetHashCode (), "GHC#2");
		}

		[Test]
		public void TestEquals ()
		{
			upa = new UrlPropertyAttribute ("sanjay");
			
			Assert.IsFalse (upa.Equals (upa2), "Equals#1");
			//Assert.IsTrue (upa.Equals (upa1), "Equals#2");

			Assert.IsFalse (upa1.Equals (upa2), "Equals#3");
			
			upa1 = new UrlPropertyAttribute ("sanjay", UrlTypes.Absolute);
			Assert.IsFalse (upa2.Equals (upa1), "Equals#4");
			upa1 = new UrlPropertyAttribute ("sanjay", UrlTypes.DocRelative);
			//Assert.IsTrue (upa2.Equals (upa1), "Equals#5");

		}
			
	}
}

#endif

