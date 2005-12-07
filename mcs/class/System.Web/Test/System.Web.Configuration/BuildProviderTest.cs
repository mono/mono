//
// BuildProviderTest.cs 
//	- unit tests for System.Web.Configuration.BuildProvider
//
// Author:
//	Chris Toshok  <toshok@ximian.com>
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
using System.Configuration;
using System.Web.Configuration;
using System.Web;
using System.Web.Security;

namespace MonoTests.System.Web.Configuration {

	[TestFixture]
	public class BuildProviderTest  {

		[Test]
		public void EqualsAndHashCode ()
		{
			BuildProvider b1, b2;

			b1 = new BuildProvider (".hi", "System.Bye");
			b2 = new BuildProvider (".hi", "System.Bye");

			Assert.IsTrue (b1.Equals (b2), "A1");
			Assert.AreEqual (b1.GetHashCode (), b2.GetHashCode (), "A2");
		}

		[Test]
		[ExpectedException (typeof (ConfigurationErrorsException))]
		public void ctor_validationFailure1 ()
		{
			BuildProvider b = new BuildProvider ("", "hi");
		}

		[Test]
		[ExpectedException (typeof (ConfigurationErrorsException))]
		public void ctor_validationFailure2 ()
		{
			BuildProvider b = new BuildProvider ("hi", "");
		}
		
		[Test]
		[ExpectedException (typeof (ConfigurationErrorsException))]
		public void Extension_validationFailure ()
		{
			BuildProvider b = new BuildProvider ("hi", "bye");

			b.Extension = "";
		}

		[Test]
		[ExpectedException (typeof (ConfigurationErrorsException))]
		public void Type_validationFailure ()
		{
			BuildProvider b = new BuildProvider ("hi", "bye");

			b.Type = "";
		}
	}
}

#endif
