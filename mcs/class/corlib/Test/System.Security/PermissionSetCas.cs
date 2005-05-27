//
// PermissionSetCas.cs - CAS Unit Tests for PermissionSet
//
// Author:
//	Sebastien Pouliot (sebastien@ximian.com)
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

using System;
using System.Security;
using System.Security.Permissions;

using NUnit.Framework;

namespace MonoCasTests.System.Security {

	[TestFixture]
	[Category ("CAS")]
	public class PermissionSetCas {

		[SetUp]
		public void SetUp ()
		{
			if (!SecurityManager.SecurityEnabled)
				Assert.Ignore ("SecurityManager isn't enabled");
		}

#if NET_2_0
		[Test]
		[ExpectedException (typeof (ExecutionEngineException))]
		public void RevertAssert_WithoutAssertion ()
		{
			PermissionSet.RevertAssert ();
		}

		[Test]
		public void RevertAssert_WithAssertion ()
		{
			PermissionSet ups = new PermissionSet (PermissionState.Unrestricted);
			ups.Assert ();
			PermissionSet.RevertAssert ();
		}
#endif
	}
}
