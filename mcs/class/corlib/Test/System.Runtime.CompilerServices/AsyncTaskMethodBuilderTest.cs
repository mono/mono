//
// AsyncTaskMethodBuilderTest.cs
//
// Authors:
//	Marek Safar  <marek.safar@gmail.com>
//
// Copyright (C) 2014 Xamarin, Inc (http://www.xamarin.com)
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
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using System.Runtime.CompilerServices;
using System.Runtime.Remoting.Messaging;

namespace MonoTests.System.Runtime.CompilerServices
{
	[TestFixture]
	public class AsyncTaskMethodBuilderTest
	{
#if !MONOTOUCH
		// For some reason MT excludes CallContext handling

		[Test]
		public void CallContextFlow ()
		{
			CallContext.LogicalSetData ("name0", "0");

			Assert.IsTrue (Task.WhenAll (Work ("A"), Work ("B")).Wait (4000), "#0");
			Assert.IsNull (CallContext.LogicalGetData ("A"), "#A");
			Assert.IsNull (CallContext.LogicalGetData ("B"), "#B");
		}

		static async Task Work (string name)
		{
			Assert.AreEqual ("0", CallContext.LogicalGetData ("name0"), "#1" + name);
			CallContext.LogicalSetData ("name", name);

			await Task.Delay (10);

			var found = CallContext.LogicalGetData ("name");
			Assert.AreEqual (name, found, "#2" + name);
		}
#endif
	}
}
