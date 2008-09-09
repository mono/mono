//
// HttpApplicationStateBaseTest.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2008 Novell Inc. http://novell.com
//

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
using System.Web;
using NUnit.Framework;

namespace MonoTests.System.Web
{
	class HttpApplicationStateImpl : HttpApplicationStateBase
	{
	}

	[TestFixture]
	public class HttpApplicationStateBaseTest
	{
		[Test]
		public void DefaultImplementation ()
		{
			var i = new HttpApplicationStateImpl ();
			object x;
			try {
				x = i.AllKeys;
				Assert.Fail ("#1");
			} catch (NotImplementedException) {
			}
			try {
				x = i.Contents;
				Assert.Fail ("#2");
			} catch (NotImplementedException) {
			}
			try {
				x = i.Count;
				Assert.Fail ("#3");
			} catch (NotImplementedException) {
			}
			try {
				x = i.IsSynchronized;
				Assert.Fail ("#4");
			} catch (NotImplementedException) {
			}
			try {
				x = i.StaticObjects;
				Assert.Fail ("#5");
			} catch (NotImplementedException) {
			}
			try {
				x = i.SyncRoot;
				Assert.Fail ("#6");
			} catch (NotImplementedException) {
			}

			try {
				i.Add (null, null);
				Assert.Fail ("#7");
			} catch (NotImplementedException) {
			}
			try {
				i.Clear ();
				Assert.Fail ("#8");
			} catch (NotImplementedException) {
			}
			try {
				i.CopyTo (null, 0);
				Assert.Fail ("#9");
			} catch (NotImplementedException) {
			}
			try {
				i.Get (null);
				Assert.Fail ("#10");
			} catch (NotImplementedException) {
			}
			try {
				i.GetEnumerator ();
				Assert.Fail ("#11");
			} catch (NotImplementedException) {
			}

			// ... so, default implementation is basically NIE.
		}
	}
}
