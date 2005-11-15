//
// UriParserCas.cs - CAS unit tests for System.UriParser
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
using System.IO;
using System.Reflection;
using System.Security;
using System.Security.Permissions;

using MonoTests.System;

namespace MonoCasTests.System {

	[TestFixture]
	[Category ("CAS")]
	public class UriParserCas {

		[SetUp]
		public void SetUp ()
		{
			if (!SecurityManager.SecurityEnabled)
				Assert.Ignore ("SecurityManager.SecurityEnabled is OFF");
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void ReuseUnitTest_Deny_Unrestricted ()
		{
			UriParserTest unit = new UriParserTest ();
			unit.Prefix = "cas.deny.unrestricted.test.";
			// static stuff
			unit.IsKnownScheme_WellKnown ();
		}

		[Test]
		[SecurityPermission (SecurityAction.PermitOnly, Infrastructure = true)]
		public void ReuseUnitTest_PermitOnly_Infrastructure ()
		{
			UriParserTest unit = new UriParserTest ();
			unit.Prefix = "cas.permitonly.infrastructure..test.";
			// static stuff
			unit.Register ();
			unit.Register_Minus1Port ();
			unit.Register_UInt16PortMinus1 ();
			unit.OnRegister ();
		}

		[Test]
		[SecurityPermission (SecurityAction.Deny, Infrastructure = true)]
		[ExpectedException (typeof (SecurityException))]
		public void ReuseUnitTest_Deny_Infrastructure ()
		{
			UriParserTest unit = new UriParserTest ();
			unit.Prefix = "cas.deny.infrastructure.test.";
			// static stuff
			unit.Register ();
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void LinkDemand_Deny_Unrestricted ()
		{
			// UriParser is an asbtract class - so we use the unit test derived class
			// because we're 100% sure this doesn't introduce new security checks
			ConstructorInfo ci = typeof (UnitTestUriParser).GetConstructor (new Type[0]);
			Assert.IsNotNull (ci, "default .ctor()");
			Assert.IsNotNull (ci.Invoke (null), "invoke");
		}
	}
}

#endif
