//
// UserNameSecurityTokenTest.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2006 Novell, Inc.  http://www.novell.com
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
#if !MOBILE
using System;
using System.IdentityModel.Selectors;
using System.IdentityModel.Tokens;
using System.Xml;
using NUnit.Framework;

namespace MonoTests.System.IdentityModel.Selectors
{
	[TestFixture]
	public class UserNameSecurityTokenTest
	{
		[Test]
		public void DefaultValues ()
		{
			UniqueId id = new UniqueId ();
			UserNameSecurityToken t = new UserNameSecurityToken ("mono", "poly", id.ToString ());
			Assert.AreEqual (id.ToString (), t.Id, "#1");
			Assert.AreEqual ("mono", t.UserName, "#2");
			Assert.AreEqual ("poly", t.Password,"#3");
			Assert.IsTrue (DateTime.Today.ToUniversalTime () <= t.ValidFrom && DateTime.Now.ToUniversalTime () >= t.ValidFrom, "#4");
			Assert.AreEqual (DateTime.MaxValue.AddDays (-1), t.ValidTo, "#5");
			Assert.AreEqual (0, t.SecurityKeys.Count, "#6");
		}
	}
}
#endif
