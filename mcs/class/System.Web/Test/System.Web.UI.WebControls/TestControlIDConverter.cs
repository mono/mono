//
// Tests for System.Web.UI.WebControls.ControlIDConverter.cs 
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
using System.Web.UI.WebControls;
using System.Collections;
using System.ComponentModel;
using System.IO;

namespace MonoTests.System.Web.UI.WebControls
{
	[TestFixture]	
	public class ControlIDConverterTest 
	{
		ControlIDConverter ctrlConv;

		[TearDown]
		public void TearDown () {}

		[SetUp]
		public void SetUp ()		
		{
			ctrlConv = new ControlIDConverter ();
		}

		[Test]
		public void TestGetStandardValues ()
		{
			Assert.IsNull (ctrlConv.GetStandardValues (), "GSV#1");
			
			Assert.IsNull (ctrlConv.GetStandardValues (null), "GSV#2");
		}

		[Test]
		public void TestGetStandardValuesExclusive ()
		{
			Assert.IsFalse (ctrlConv.GetStandardValuesExclusive (), "GSVE#1");
			
			Assert.IsFalse (ctrlConv.GetStandardValuesExclusive (null), "GSVE#2");
		}

		[Test]
		public void TestGetStandardValuesSupported ()
		{
			Assert.IsFalse (ctrlConv.GetStandardValuesSupported (), "GSVS#1");
			
			Assert.IsFalse (ctrlConv.GetStandardValuesSupported (null), "GSVS#2");
		}
			
	}
}

#endif

