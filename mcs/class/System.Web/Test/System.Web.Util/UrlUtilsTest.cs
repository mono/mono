﻿//
// System.Web.Util.UrlUtilsTest.cs - Unit tests for System.Web.Util.UrlUtils
//
// Author:
//	Noam Lampert <noaml@mainsoft.com>
//
// Copyright (C) 2006 Novell, Inc (http://www.novell.com)
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
using System.Text;
using System.Web;
using System.Web.Util;
using System.Collections.Specialized;
using NUnit.Framework;
using System.Diagnostics;

namespace MonoTests.System.Web.Util
{
	[TestFixture]
	public class UrlUtilsTest
	{
		[Test]
		public void CanonicTest()
		{
			Assert.AreEqual("/Sample.aspx",SystemWebTestShim.UrlUtils.Canonic("/WebApplication1//../Sample.aspx"));
		}
		[Test]
		public void CanonicTest2()
		{
			Assert.AreEqual("Sample.aspx",SystemWebTestShim.UrlUtils.Canonic("Path1/../Sample.aspx"));
		}
		[Test]
		public void CanonicTest3()
		{
			Assert.AreEqual("/Path1/Sample.aspx",SystemWebTestShim.UrlUtils.Canonic("/../Path1/Sample.aspx"));
		}
		[Test]
		public void CanonicTest4()
		{
			Assert.AreEqual("/Sample.aspx",SystemWebTestShim.UrlUtils.Canonic("/../Path1/../../Sample.aspx"));
		}
		[Test]
		[ExpectedException(typeof(HttpException))]
		public void CanonicTest5()
		{
			SystemWebTestShim.UrlUtils.Canonic("../Path1/../../Sample.aspx");
		}
	}
}
