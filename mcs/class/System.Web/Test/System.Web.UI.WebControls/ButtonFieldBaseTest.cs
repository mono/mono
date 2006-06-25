//
// Tests for System.Web.UI.WebControls.ButtonFieldBaseTest.cs
//
// Author:
//	Yoni Klein (yonik@mainsoft.com)
//
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


#if NET_2_0


using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.IO;
using System.Drawing;
using System.Collections;
using System.Collections.Specialized;
using NUnit.Framework;
using System.Data;




namespace MonoTests.System.Web.UI.WebControls
{
	class PokerButtonFieldBase : ButtonFieldBase
	{
		// View state Stuff
		public PokerButtonFieldBase ()
			: base ()
		{
			TrackViewState ();
		}

		public object SaveState ()
		{
			return SaveViewState ();
		}

		public void LoadState (object o)
		{
			LoadViewState (o);
		}

		public StateBag StateBag
		{
			get { return base.ViewState; }
		}

		protected override DataControlField CreateField ()
		{
			throw new Exception ("The method or operation is not implemented.");
		}

		public void DoCopyProperties (DataControlField newField)
		{
			base.CopyProperties (newField);
		}
	}

	[TestFixture]
	public class ButtonFieldBaseTest
	{
		[Test]
		public void ButtonFieldBase_DefaultProperty ()
		{
			PokerButtonFieldBase button = new PokerButtonFieldBase ();
			Assert.AreEqual (ButtonType.Link, button.ButtonType, "ButtonType");
			Assert.AreEqual (false, button.CausesValidation, "CausesValidation");
			Assert.AreEqual (false, button.ShowHeader, "ShowHeader");
			Assert.AreEqual ("", button.ValidationGroup, "ValidationGroup"); 
		}

		[Test]
		public void ButtonFieldBase_AssignProperty ()
		{
			PokerButtonFieldBase button = new PokerButtonFieldBase ();
			button.ButtonType = ButtonType.Image;
			Assert.AreEqual (ButtonType.Image, button.ButtonType, "ButtonType");
			button.CausesValidation = true;
			Assert.AreEqual (true, button.CausesValidation, "CausesValidation");
			button.ShowHeader = true;
			Assert.AreEqual (true, button.ShowHeader, "ShowHeader");
			button.ValidationGroup = "test";
			Assert.AreEqual ("test", button.ValidationGroup, "ValidationGroup"); 
		}

		[Test]
		public void ButtonFieldBase_CopyProperties ()
		{
			PokerButtonFieldBase button = new PokerButtonFieldBase ();
			PokerButtonFieldBase copy = new PokerButtonFieldBase ();
			button.ButtonType = ButtonType.Image;
			button.CausesValidation = true;
			button.ShowHeader = true;
			button.ValidationGroup = "test";
			button.DoCopyProperties (copy);
			Assert.AreEqual ("test", copy.ValidationGroup, "ValidationGroup"); 
			Assert.AreEqual (ButtonType.Image, copy.ButtonType, "ButtonType");
			Assert.AreEqual (true, copy.CausesValidation, "CausesValidation");
			Assert.AreEqual (true, copy.ShowHeader, "ShowHeader");
		}
	}
}
#endif