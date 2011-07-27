#if NET_4_0
// AggregateExceptionTests.cs
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
using System.Linq;
using System.Threading;

using NUnit;
using NUnit.Core;
using NUnit.Framework;

namespace MonoTests.System
{
	[TestFixture()]
	public class AggregateExceptionTest
	{
		AggregateException e;
		
		[SetUpAttribute]
		public void Setup()
		{
			e = new AggregateException(new Exception("foo"), new AggregateException(new Exception("bar"), new Exception("foobar")));
		}

		[Test]
		public void SimpleInnerExceptionTestCase ()
		{
			var message = "Foo";
			var inner = new ApplicationException (message);
			var ex = new AggregateException (inner);

			Assert.IsNotNull (ex.InnerException);
			Assert.IsNotNull (ex.InnerExceptions);

			Assert.AreEqual (inner, ex.InnerException);
			Assert.AreEqual (1, ex.InnerExceptions.Count);
			Assert.AreEqual (inner, ex.InnerExceptions[0]);
			Assert.AreEqual (message, ex.InnerException.Message);
		}
		
		[TestAttribute]
		public void FlattenTestCase()
		{
			AggregateException ex = e.Flatten();
			
			Assert.AreEqual(3, ex.InnerExceptions.Count, "#1");
			Assert.AreEqual(3, ex.InnerExceptions.Where((exception) => !(exception is AggregateException)).Count(), "#2");
		}
	}
}
#endif
