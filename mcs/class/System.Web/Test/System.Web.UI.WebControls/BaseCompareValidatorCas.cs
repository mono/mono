//
// BaseCompareValidatorCas.cs 
//	- CAS unit tests for System.Web.UI.WebControls.BaseCompareValidator
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
using System.Security;
using System.Security.Permissions;
using System.Web.UI.WebControls;

using MonoTests.System.Web.UI.WebControls;

namespace MonoCasTests.System.Web.UI.WebControls {

	[TestFixture]
	[Category ("CAS")]
	public class BaseCompareValidatorCas {

		// note: we do not inherit from AspNetHostingMinimal because
		// BaseCompareValidator is an abstract class

		[SetUp]
		public virtual void SetUp ()
		{
			if (!SecurityManager.SecurityEnabled)
				Assert.Ignore ("SecurityManager.SecurityEnabled is OFF");
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void Deny_Unrestricted ()
		{
			BaseCompareValidatorTest unit = new BaseCompareValidatorTest ();
			unit.ViewState ();
			unit.CanConvert ();
			unit.Convert ();
			unit.Compare ();
			// can't call MiscPropertiesAndMethods because it change the
			// thread's culture (which requires ControlThread)
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void MiscPropertiesAndMethods ()
		{
			BaseCompareValidatorPoker p = new BaseCompareValidatorPoker ();

			Assert.AreEqual (p.GetCutoffYear (), 2029, "E1");
			Assert.AreEqual (p.GetFullYear (29), 2029, "E2");
#if NET_2_0
			Assert.AreEqual (p.GetFullYear (30), 1930, "E3");
#else
			Assert.AreEqual (p.GetFullYear (30), 2030, "E3"); // XXX this is broken
#endif
			Assert.IsNotNull (p.GetDateElementOrder (), "E4");
		}
	}
}
