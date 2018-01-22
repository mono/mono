//
// ServiceThrottlingBehaviorTest.cs
//
// Author:
//	Atsushi Enomoto  <atsushi@ximian.com>
//
// Copyright (C) 2009 Novell, Inc.  http://novell.com
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
#if !MOBILE && !XAMMAC_4_5
using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.ServiceModel.Channels;

namespace MonoTests.System.ServiceModel.Description
{
	[TestFixture]
	public class ServiceThrottlingBehaviorTest
	{
		[Test]
		public void DefaultValues ()
		{
			var t = new ServiceThrottlingBehavior ();
			Assert.AreEqual (10, t.MaxConcurrentSessions, "#1");
			Assert.AreEqual (16, t.MaxConcurrentCalls, "#2");
			Assert.AreEqual (26, t.MaxConcurrentInstances, "#3");
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))] // hmm...
		public void SetZero ()
		{
			new ServiceThrottlingBehavior () { MaxConcurrentCalls = 0 };
		}
	}
}
#endif