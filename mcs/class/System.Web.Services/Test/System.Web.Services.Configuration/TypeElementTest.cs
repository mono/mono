//
// System.Web.Services.Configuration.TypeElementTest.cs - Unit tests
// for System.Web.Services.Configuration.TypeElement
//
// Author:
//	Chris Toshok  <toshok@ximian.com>
//
// Copyright (C) 2006 Novell, Inc (http://www.novell.com)
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
using System.Web.Services.Configuration;
using NUnit.Framework;

namespace MonoTests.System.Web.Services.Configuration {
	[TestFixture]
	public class TypeElementTest
	{
		[Test]
		[Ignore ("causes NRE on .NET")]
		public void Ctors1 ()
		{
			TypeElement el = new TypeElement ();
			Assert.IsNull (el.Type, "A1");
		}

		[Test]
		public void Ctors2 ()
		{
			TypeElement el;

			el = new TypeElement (typeof (string));
			Assert.AreEqual (typeof (string), el.Type, "A2");

			el = new TypeElement ("System.String");
			Assert.AreEqual (typeof (string), el.Type, "A3");
		}

		[Test]
		public void GetSet ()
		{
			TypeElement el = new TypeElement ();

			el.Type = typeof (string);
			Assert.AreEqual (typeof (string), el.Type, "A1");
		}
	}
}

#endif
