//
// Tests for System.Web.UI.WebControls.Adapters.DataBoundControlAdapter
//
// Author:
//	Dean Brettle (dean@brettle.com)
//
// Copyright (C) 2008 Novell, Inc (http://www.novell.com)
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

#if NET_2_0 && !TARGET_DOTNET
using NUnit.Framework;
using System;
using System.Collections;
using System.Drawing;
using System.IO;
using System.Globalization;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.Adapters;
using System.Web.Configuration;
using MonoTests.SystemWeb.Framework;


namespace MonoTests.System.Web.UI.WebControls.Adapters
{
	[TestFixture]
	public class DataBoundControlAdapterTest
	{
		MyDataBoundControl c;
		MyDataBoundControlAdapter a;

		[SetUp]
		public void SetUp ()
		{
			c = new MyDataBoundControl ();
			a = new MyDataBoundControlAdapter (c);
		}
		
		[Test]
		public void PerformDataBinding ()
		{
			ArrayList data = new ArrayList ();
			a.PerformDataBinding (data);
			Assert.AreEqual (data, c.data, "PerformDataBinding #1");
		}

		[Test]
		public void Control ()
		{
			Assert.AreEqual (c, a.Control, "Control #1");
		}
				
#region Support classes
		
		class MyDataBoundControl : DataBoundControl
		{
			internal IEnumerable data;
			
			protected internal override void PerformDataBinding (IEnumerable data)
			{
				this.data = data;
			}
		}

		class MyDataBoundControlAdapter : SystemWebTestShim.DataBoundControlAdapter
		{
			internal MyDataBoundControlAdapter (DataBoundControl c) : base (c)
			{
			}
			
			new internal DataBoundControl Control {
				get { return base.Control; }
			}
		}
#endregion
	}
}
#endif
