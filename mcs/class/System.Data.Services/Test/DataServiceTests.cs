//
// DataServiceTests.cs
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
	public class DataServiceTests {
		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void AttachHostNull()
		{
			var d = new DataService<string>();
			d.AttachHost (null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ProcessRequestForMessageNull()
		{
			var d = new DataService<string>();
			d.ProcessRequestForMessage (null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void HandleExceptionNull()
		{
			var d = new TestDataService<string>();
			d.TestHandleException (null);
		}
	}

	public class TestDataService<T>
		: DataService<T>
	{
		public T TestCurrentDataSource
		{
			get { return this.CurrentDataSource; }
		}

		public T TestCreateDataSource()
		{
			return this.CreateDataSource();
		}

		public void TestHandleException (HandleExceptionArgs args)
		{
			this.HandleException (args);
		}

		public void TestOnStartProcessingRequest (ProcessRequestArgs args)
		{
			this.OnStartProcessingRequest (args);
		}
	}
}