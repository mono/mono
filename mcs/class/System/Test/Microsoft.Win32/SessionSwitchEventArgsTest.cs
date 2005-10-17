//
// SessionSwitchEventArgsTest.cs 
//	- Unit tests for Microsoft.Win32.SessionSwitchEventArgs
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
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
using Microsoft.Win32;

namespace MonoTests.Microsoft.Win32 {

	[TestFixture]
	[Category ("CAS")]
	public class SessionSwitchEventArgsTest {

		[Test]
		public void Constructor ()
		{
			foreach (SessionSwitchReason ssr in Enum.GetValues (typeof (SessionSwitchReason))) {
				SessionSwitchEventArgs ssea = new SessionSwitchEventArgs (ssr);
				Assert.AreEqual (ssr, ssea.Reason, ssr.ToString ());
			}
		}

		[Test]
		public void OutOfRange ()
		{
			SessionSwitchReason ssr = (SessionSwitchReason) Int32.MinValue;
			SessionSwitchEventArgs ssea = new SessionSwitchEventArgs (ssr);
			Assert.AreEqual (ssr, ssea.Reason, "Int32.MinValue");
			// no validation is done on the enum value used
		}
	}
}

#endif
