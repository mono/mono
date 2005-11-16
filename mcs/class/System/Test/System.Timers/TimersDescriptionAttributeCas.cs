//
// TimersDescriptionAttributeCas.cs 
//	- CAS unit tests for System.Timers.TimersDescriptionAttributeCas
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

using NUnit.Framework;

using System;
using System.Reflection;
using System.Security;
using System.Security.Permissions;
using System.Timers;

namespace MonoCasTests.System.Timers {

	[TestFixture]
	[Category ("CAS")]
	public class TimersDescriptionAttributeCas {

		[SetUp]
		public void SetUp ()
		{
			if (!SecurityManager.SecurityEnabled)
				Assert.Ignore ("SecurityManager.SecurityEnabled is OFF");
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void Constructor_Deny_Unrestricted ()
		{
			TimersDescriptionAttribute tda = new TimersDescriptionAttribute ("Mono");
			// note: in the unit tests MS always (1.x and 2.0) return null
			// but when "created" indirectly (e.g. corcompare) the real value is returned
			// note: Mono always return the value
			Assert.AreEqual (tda.Description, tda.Description, "Description");
			// this assert doesn't do anything (except removing warning) but we know,
			// for CAS, that nothing protects the property getter
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void LinkDemand_Deny_Unrestricted ()
		{
			Type[] types = new Type[1] { typeof (string) };
			ConstructorInfo ci = typeof (TimersDescriptionAttribute).GetConstructor (types);
			Assert.IsNotNull (ci, ".ctor(string)");
			Assert.IsNotNull (ci.Invoke (new object[1] { "Mono" }), "invoke");
		}
	}
}
