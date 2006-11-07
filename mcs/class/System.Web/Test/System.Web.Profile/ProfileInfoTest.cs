//
// System.Web.Profile.ProfileInfo.cs - Unit tests for System.Web.Profile.ProfileInfo
//
// Author:
//	Chris Toshok  <toshok@novell.com.com>
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
using System;
using System.Text;
using System.Web;
using System.Web.Profile;
using NUnit.Framework;

namespace MonoTests.System.Web.Profile {

	class ProfileInfoPoker : ProfileInfo {
	}

	[TestFixture]
	public class TestProfileInfo {

	  
		[Test]
		public void ProtectedCtor ()
		{
			ProfileInfoPoker poker = new ProfileInfoPoker();

			Assert.IsNull (poker.UserName, "A1");
			Assert.AreEqual (DateTime.MinValue, poker.LastUpdatedDate.Date, "A2");
			Assert.AreEqual (DateTime.MinValue, poker.LastActivityDate.Date, "A3");
			Assert.IsFalse  (poker.IsAnonymous, "A4");
			Assert.AreEqual (0, poker.Size, "A5");
		}
	}
}
#endif
