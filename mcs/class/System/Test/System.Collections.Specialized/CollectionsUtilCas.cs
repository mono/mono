//
// CollectionsUtilCas.cs 
//	- CAS unit tests for System.Collections.Specialized.CollectionsUtil
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

#if !MOBILE

using NUnit.Framework;

using System;
using System.Collections;
using System.Collections.Specialized;
using System.Reflection;
using System.Security;
using System.Security.Permissions;

using MonoTests.System.Collections.Specialized;

namespace MonoCasTests.System.Collections.Specialized {

	[TestFixture]
	[Category ("CAS")]
	public class CollectionsUtilCas {

		[SetUp]
		public void SetUp ()
		{
			if (!SecurityManager.SecurityEnabled)
				Assert.Ignore ("SecurityManager.SecurityEnabled is OFF");
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void ReuseUnitTests_Deny_Unrestricted ()
		{
			Assert.IsNotNull (CollectionsUtil.CreateCaseInsensitiveHashtable (), "CreateCaseInsensitiveHashtable()");
			Assert.IsNotNull (CollectionsUtil.CreateCaseInsensitiveHashtable (1), "CreateCaseInsensitiveHashtable(int)");
			Assert.IsNotNull (CollectionsUtil.CreateCaseInsensitiveHashtable (new Hashtable ()), "CreateCaseInsensitiveHashtable(IDictionary)");
			Assert.IsNotNull (CollectionsUtil.CreateCaseInsensitiveSortedList (), "CreateCaseInsensitiveSortedList");
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void LinkDemand_Deny_Unrestricted ()
		{
			ConstructorInfo ci = typeof (CollectionsUtil).GetConstructor (new Type[0]);
			Assert.IsNotNull (ci, "default .ctor()");
			Assert.IsNotNull (ci.Invoke (null), "invoke");
		}
	}
}

#endif