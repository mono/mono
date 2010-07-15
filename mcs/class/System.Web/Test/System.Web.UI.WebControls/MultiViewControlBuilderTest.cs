//
// Authors:
//	Marek Habersack <mhabersack@novell.com>
//
// (C) 2010 Novell, Inc (http://novell.com)
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
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using NUnit.Framework;

namespace MonoTests.System.Web.UI.WebControls
{
	[TestFixture]
	public class MultiViewControlBuilderTest
	{
		[Test (Description="Just check if it throws any exception")]
		public void AppendSubBuilder ()
		{
			var bldr = new MultiViewControlBuilder ();
			var subbldr = new ControlBuilder ();

			bldr.AppendSubBuilder (subbldr);

			subbldr = ControlBuilder.CreateBuilderFromType (null, null, typeof (TextBox), "TextBox", null, null, 0, "dummy");
			bldr.AppendSubBuilder (subbldr);

			subbldr = ControlBuilder.CreateBuilderFromType (null, null, typeof (View), "View", null, null, 0, "dummy");
			bldr.AppendSubBuilder (subbldr);

			subbldr = ControlBuilder.CreateBuilderFromType (null, null, typeof (string), "Literal", null, null, 0, "dummy");
			bldr.AppendSubBuilder (subbldr);
		}
	}
}
