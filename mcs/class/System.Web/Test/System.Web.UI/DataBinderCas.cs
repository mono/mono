//
// DataBinderCas.cs - CAS unit tests for System.Web.UI.DataBinder
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
using System.Collections;
using System.Security.Permissions;
using System.Web;
using System.Web.UI;

namespace MonoCasTests.System.Web.UI {

	[TestFixture]
	[Category ("CAS")]
	public class DataBinderCas : AspNetHostingMinimal {

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void Eval_Deny_Unrestricted ()
		{
			Assert.IsNull (DataBinder.Eval (null, "Data"), "Eval(object,string)");
			Assert.AreEqual (String.Empty, DataBinder.Eval (null, "Data", null), "Eval(object,string,string)");
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		[ExpectedException (typeof (ArgumentNullException))]
		public void GetIndexedPropertyValue2_Deny_Unrestricted ()
		{
			DataBinder.GetIndexedPropertyValue (null, "Data");
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		[ExpectedException (typeof (ArgumentNullException))]
		public void GetIndexedPropertyValue3_Deny_Unrestricted ()
		{
			DataBinder.GetIndexedPropertyValue (null, "Data", "{0}");
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		[ExpectedException (typeof (ArgumentNullException))]
		public void GetPropertyValue2_Deny_Unrestricted ()
		{
			DataBinder.GetPropertyValue (null, "Data");
			Assert.IsNull (DataBinder.GetPropertyValue (null, "Data", "{0}"), "GetPropertyValue(object,string,string)");
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		[ExpectedException (typeof (ArgumentNullException))]
		public void GetPropertyValue3_Deny_Unrestricted ()
		{
			DataBinder.GetPropertyValue (null, "Data", "{0}");
		}

#if NET_2_0
		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void GetDataItem_Deny_Unrestricted ()
		{
			Assert.IsNull (DataBinder.GetDataItem (null), "GetDataItem(object)");
			bool found = true;
			Assert.IsNull (DataBinder.GetDataItem (null, out found), "GetDataItem(object,out bool)");
			Assert.IsFalse (found, "found");
		}
#endif

		// LinkDemand

		public override Type Type {
			get { return typeof (DataBinder); }
		}
	}
}
