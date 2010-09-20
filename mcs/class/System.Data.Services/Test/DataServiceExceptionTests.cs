//
// DataServiceExceptionTests.cs
//
// Author:
//   Eric Maupin  <me@ermau.com>
//
// Copyright (c) 2009 Eric Maupin (http://www.ermau.com)
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

namespace System.Data.Services.Tests {
	[TestFixture]
	public class DataServiceExceptionTests {
		[Test]
		public void CtorMessage()
		{
			var ex = new DataServiceException ("message");
			Assert.AreEqual ("message", ex.Message);
			Assert.IsNull (ex.InnerException);
		}

		[Test]
		public void CtorMessageAndInner()
		{
			Exception inner = new Exception ("inner");
			var ex = new DataServiceException ("message", inner);

			Assert.AreEqual ("message", ex.Message);
			Assert.AreEqual (inner, ex.InnerException);
		}

		[Test]
		public void CtorStatusAndMessage()
		{
			var ex = new DataServiceException (404, "message");
			Assert.AreEqual (404, ex.StatusCode);
			Assert.AreEqual ("message", ex.Message);
		}

		[Test]
		public void CtorStatusAndErrorMessageLanguageException()
		{
			Exception inner = new Exception ("inner");
			var ex = new DataServiceException (404, "error", "message", "language", inner);
			Assert.AreEqual ("message", ex.Message);
			Assert.AreEqual (404, ex.StatusCode);
			Assert.AreEqual ("language", ex.MessageLanguage);
			Assert.AreEqual (inner, ex.InnerException);
		}
	}
}