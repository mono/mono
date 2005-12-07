//
// Tests for System.Web.UI.WebControls.ListItem.cs 
//
// Author:
//	Gonzalo Paniagua Javier (gonzalo@novell.com)
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
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace MonoTests.System.Web.UI.WebControls
{
	[TestFixture]	
	public class ListItemTest 
	{
		[Test]
		public void Defaults ()
		{
			ListItem li = new ListItem ();
			Assert.AreEqual ("", li.Text, "#01");
			Assert.AreEqual ("", li.Value, "#02");
			Assert.AreEqual (false, li.Selected, "#03");
		}

		[Test]
		public void Defaults2 ()
		{
			ListItem li = new ListItem ("something");
			Assert.AreEqual ("something", li.Text, "#01");
			Assert.AreEqual ("something", li.Value, "#02");
			Assert.AreEqual (false, li.Selected, "#03");
		}

		[Test]
		public void Defaults3 ()
		{
			ListItem li = new ListItem ("something", "else");
			Assert.AreEqual ("something", li.Text, "#01");
			Assert.AreEqual ("else", li.Value, "#02");
			Assert.AreEqual (false, li.Selected, "#03");
		}

		[Test]
		[ExpectedException (typeof (HttpException))]
		public void AllowedChildren ()
		{
			IParserAccessor li = new ListItem ("something", "else");
			li.AddParsedSubObject ("hola");
		}

		[Test]
		[ExpectedException (typeof (NullReferenceException))]
		public void AllowedChildren2 ()
		{
			IParserAccessor li = new ListItem ("something", "else");
			li.AddParsedSubObject (null);
		}

		[Test]
		[ExpectedException (typeof (HttpException))]
		public void AllowedChildren3 ()
		{
			IParserAccessor li = new ListItem ("something", "else");
			li.AddParsedSubObject (new CheckBox ());
		}

		[Test]
		public void AllowedChildren4 ()
		{
			ListItem li = new ListItem ("something", "else");
			IParserAccessor parser = (ListItem) li;
			parser.AddParsedSubObject (new LiteralControl ("Hola"));
			Assert.AreEqual ("Hola", li.Text, "#01");
			Assert.AreEqual ("else", li.Value, "#02");
		}
	}
}

