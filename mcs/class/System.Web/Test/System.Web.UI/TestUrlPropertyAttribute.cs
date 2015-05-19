//
// Tests for System.Web.UI.UrlPropertyAttribute.cs 
//
// Author:
//	Sanjay Gupta (gsanjay@novell.com)
//
// Copyright (C) 2004-2005 Novell, Inc (http://www.novell.com)
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
		string filter;

		[SetUp]
		public void SetUp ()		
		{
			filter = "filter";
			upa = new UrlPropertyAttribute ();
			upa1 = new UrlPropertyAttribute (filter);
		}

		[Test]
		public void TestFilter ()
		{
			Assert.AreEqual (upa.Filter, "*.*", "Filter#1");
			Assert.AreEqual (upa1.Filter, filter, "Filter#2");
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
			Assert.IsFalse (upa.Equals (upa1), "Equals#1");
		}
			
	}
}


