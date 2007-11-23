//
// TraceContextRecordTest.cs
//
// Author:
//	Daniel Nauck  <dna(at)mono-project(dot)de>
//
// Copyright (C) 2007 Daniel Nauck
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
using System.Text;
using System.Web;
using NUnit.Framework;

namespace MonoTests.System.Web
{

	[TestFixture]
	public class TraceContextRecordTest
	{
		[Test]
		public void TraceContextRecordTest_Null () 
		{
			TraceContextRecord record = new TraceContextRecord (null, null, false, null);
			Assert.AreEqual (null, record.Category, "#A 1");
			Assert.AreEqual (null, record.Message, "#A 2");
			Assert.IsFalse (record.IsWarning, "#A 3");
			Assert.AreEqual (null, record.ErrorInfo, "#A 4");
		}

		[Test]
		public void TraceContextRecordTest_Empty ()
		{
			TraceContextRecord record = new TraceContextRecord (string.Empty, string.Empty, false, null);
			Assert.AreEqual (string.Empty, record.Category, "#B 1");
			Assert.AreEqual (string.Empty, record.Message, "#B 2");
			Assert.IsFalse (record.IsWarning, "#B 3");
			Assert.AreEqual (null, record.ErrorInfo, "#B 4");
		}

		[Test]
		public void TraceContextRecordTest_WithData ()
		{
			Exception ex = new Exception ();
			TraceContextRecord record = new TraceContextRecord ("Default.aspx", "Begin Page_Load", true, ex);
			Assert.AreEqual ("Default.aspx", record.Category, "#C 1");
			Assert.AreEqual ("Begin Page_Load", record.Message, "#C 2");
			Assert.IsTrue (record.IsWarning, "#C 3");
			Assert.AreEqual (ex, record.ErrorInfo, "#C 4");
		}
	}
}
#endif
