//
// System.Web.Services.Configuration.SoapExtensionTypeElementTest.cs - Unit tests
// for System.Web.Services.Configuration.SoapExtensionTypeElement
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
using System.Configuration;
using System.Web.Services.Configuration;
using NUnit.Framework;

namespace MonoTests.System.Web.Services {
	[TestFixture]
	public class SoapExtensionTypeElementTest
	{
		[Test]
		public void Ctors ()
		{
			SoapExtensionTypeElement el = new SoapExtensionTypeElement ();

			Assert.AreEqual (PriorityGroup.Low, el.Group, "A1");
			Assert.AreEqual (0, el.Priority, "A2");
			Assert.IsNull (el.Type, "A3");

			el = new SoapExtensionTypeElement (typeof (string), 5, PriorityGroup.High);
			Assert.AreEqual (PriorityGroup.High, el.Group, "A4");
			Assert.AreEqual (5, el.Priority, "A5");
			Assert.AreEqual (typeof (string), el.Type, "A6");

			el = new SoapExtensionTypeElement ("System.String", 5, PriorityGroup.High);
			Assert.AreEqual (PriorityGroup.High, el.Group, "A7");
			Assert.AreEqual (5, el.Priority, "A8");
			Assert.AreEqual (typeof (string), el.Type, "A9");
		}

		[Test]
		public void GetSet ()
		{
			SoapExtensionTypeElement el = new SoapExtensionTypeElement ();

			el.Group = PriorityGroup.High;
			Assert.AreEqual (PriorityGroup.High, el.Group, "A1");

			el.Priority = 2;
			Assert.AreEqual (2, el.Priority, "A2");

			el.Type = typeof (string);
			Assert.AreEqual (typeof (string), el.Type, "A3");
		}

		[Test]
		[ExpectedException (typeof (ConfigurationErrorsException))]
		public void PriorityValidator1 ()
		{
			SoapExtensionTypeElement el = new SoapExtensionTypeElement ();
			el.Priority = -1;
		}

		[Test]
		public void PriorityValidator2 ()
		{
			SoapExtensionTypeElement el = new SoapExtensionTypeElement ();
			el.Priority = 0;
			el.Priority = Int32.MaxValue;
		}
	}
}

#endif
