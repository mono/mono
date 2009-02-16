#if NET_4_0
// WriteOnceTests.cs
//
// Copyright (c) 2008 Jérémie "Garuma" Laval
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
//
//

using System;
using System.Threading;

using NUnit;
using NUnit.Framework;

namespace ParallelFxTests
{
	
	[TestFixture]
	public class WriteOnceTests
	{
		WriteOnce<int> wo;
		
		[SetUpAttribute]
		public void Setup()
		{
			wo = new WriteOnce<int>();
		}
		
		[Test, ExpectedExceptionAttribute(typeof(InvalidOperationException))]
		public void OnlyOneWriteTestCase()
		{
			wo.Value = 1;
			wo.Value = 1;
		}
		
		[Test]
		public void HasValueTestCase()
		{
			Assert.IsFalse(wo.HasValue);
			wo.Value = 1;
			Assert.IsTrue(wo.HasValue);
		}
		
		[Test]
		public void EqualityTestCase()
		{
			Assert.AreEqual(wo, wo, "#1");
			wo.Value = 1;
			Assert.AreEqual(wo, wo, "#2");
			
			WriteOnce<object> wObj = new WriteOnce<object>();
			Assert.AreEqual(wObj, wObj, "#3");
			wObj.Value = new object();
			Assert.AreEqual(wObj, wObj, "#4");
		}
	}
}
#endif
