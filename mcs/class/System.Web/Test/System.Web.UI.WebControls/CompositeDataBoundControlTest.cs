//
// Tests for System.Web.UI.WebControls.CompositeDataBoundControl.cs 
//
// Author:
//	Igor Zelmanovich (igorz@mainsoft.com)
//

//
// Copyright (C) 2006 Mainsoft, Inc (http://www.mainsoft.com)
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
using System.Globalization;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Collections;

namespace MonoTests.System.Web.UI.WebControls
{
	[TestFixture]
	public class CompositeDataBoundControlTest
	{
		class MyCompositeDataBoundControl : CompositeDataBoundControl
		{
			public bool ensureDataBound = false;
			public bool ensureCreateChildControls = false;
			public bool createChildControls1 = false;
			public bool createChildControls2 = false;
			public bool dataBind = false;
			public bool CreateChildControls_ChildControlsCreated;
			public int CreateChildControls_Controls_Count;

			protected override int CreateChildControls (IEnumerable dataSource, bool dataBinding) {
				createChildControls2 = true;
				CreateChildControls_ChildControlsCreated = ChildControlsCreated;
				CreateChildControls_Controls_Count = Controls.Count;
				return 10;
			}

			public void DoEnsureChildControls () {
				EnsureChildControls ();
			}

			public void DoCreateChildControls () {
				CreateChildControls ();
			}

			public override void DataBind () {
				base.DataBind ();
				dataBind = true;
			}

			protected override void EnsureDataBound () {
				base.EnsureDataBound ();
				ensureDataBound = true;
			}

			protected override void EnsureChildControls () {
				base.EnsureChildControls ();
				ensureCreateChildControls = true;
			}

			protected internal override void CreateChildControls () {
				base.CreateChildControls ();
				createChildControls1 = true;
			}

			public bool GetRequiresDataBinding () {
				return RequiresDataBinding;
			}

			public void SetRequiresDataBinding (bool value) {
				RequiresDataBinding = value;
			}

			public bool GetChildControlsCreated () {
				return ChildControlsCreated;
			}
		}

		[Test]
		public void DetailsView_EnsureChildControlsFlow () {
			MyCompositeDataBoundControl c = new MyCompositeDataBoundControl ();
			Assert.IsFalse (c.GetRequiresDataBinding());
			c.DoEnsureChildControls ();
			Assert.IsTrue (c.ensureCreateChildControls);
			Assert.IsFalse (c.ensureDataBound);
			Assert.IsFalse (c.dataBind);
			Assert.IsTrue (c.createChildControls1);
			Assert.IsFalse (c.createChildControls2);
		}

		[Test]
		public void DetailsView_EnsureChildControlsFlow2 () {
			MyCompositeDataBoundControl c = new MyCompositeDataBoundControl ();
			c.SetRequiresDataBinding (true);
			Assert.IsTrue (c.GetRequiresDataBinding ());
			c.DoEnsureChildControls ();
			Assert.IsTrue (c.ensureCreateChildControls);
			Assert.IsTrue (c.ensureDataBound);
			Assert.IsFalse (c.dataBind);
			Assert.IsTrue (c.createChildControls1);
			Assert.IsFalse (c.createChildControls2);
		}


		[Test]
		public void DetailsView_CreateChildControls_Clear () {
			MyCompositeDataBoundControl c = new MyCompositeDataBoundControl ();
			c.Controls.Add (new WebControl (HtmlTextWriterTag.A));
			Assert.AreEqual (1, c.Controls.Count);
			c.DoCreateChildControls ();
			Assert.AreEqual (0, c.Controls.Count);
		}

		[Test]
		public void DetailsView_CreateChildControls_Clear2 () {
			MyCompositeDataBoundControl c = new MyCompositeDataBoundControl ();
			c.Controls.Add (new WebControl (HtmlTextWriterTag.A));
			Assert.AreEqual (1, c.Controls.Count, "Controls.Count before DataBind");
			c.DataBind ();
			Assert.AreEqual (0, c.CreateChildControls_Controls_Count, "Controls.Count in CreateChildControls");
			Assert.AreEqual (0, c.Controls.Count, "Controls.Count after DataBind");

		}
		
		[Test]
		public void DataBind_ChildControlsCreated () {
			Page p = new Page ();
			MyCompositeDataBoundControl c = new MyCompositeDataBoundControl ();
			p.Controls.Add (c);
			Assert.IsFalse (c.GetChildControlsCreated (), "ChildControlsCreated before DataBind");
			c.DataBind ();
			Assert.IsTrue (c.ensureCreateChildControls);
			Assert.IsTrue (c.createChildControls1);
			Assert.IsTrue (c.createChildControls2);
			Assert.IsTrue (c.CreateChildControls_ChildControlsCreated, "ChildControlsCreated in CreateChildControls");
			Assert.IsTrue (c.GetChildControlsCreated (), "ChildControlsCreated after DataBind");
		}
	}
}
#endif
