//
// Tests for System.Web.UI.WebControls.CommandFieldTest.cs
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

namespace MonoTests.System.Web.UI.WebControls
{
	class PokerCommandField : CommandField
	{
		// View state Stuff
		public PokerCommandField ()
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

		public void DoCopyProperties (DataControlField newField)
		{
			base.CopyProperties (newField);
		}

		public DataControlField DoCreateField ()
		{
			return base.CreateField ();
		}

	}

	[TestFixture]
	public class CommandFieldTest
	{
		[Test]
		public void CommandField_DefaultProperty ()
		{
			PokerCommandField field = new PokerCommandField ();
			Assert.AreEqual ("", field.CancelImageUrl, "CancelImageUrl");
			Assert.AreEqual ("Cancel", field.CancelText, "CancelText");
			Assert.AreEqual (true, field.CausesValidation, "CausesValidation");
			Assert.AreEqual ("", field.DeleteImageUrl, "DeleteImageUrl");
			Assert.AreEqual ("Delete", field.DeleteText, "DeleteText");
			Assert.AreEqual ("", field.EditImageUrl, "EditImageUrl");
			Assert.AreEqual ("Edit", field.EditText, "EditText");
			Assert.AreEqual ("", field.InsertImageUrl, "InsertImageUrl");
			Assert.AreEqual ("Insert", field.InsertText, "InsertText");
			Assert.AreEqual ("", field.NewImageUrl, "NewImageUrl");
			Assert.AreEqual ("New", field.NewText, "NewText");
			Assert.AreEqual ("", field.SelectImageUrl, "SelectImageUrl");
			Assert.AreEqual ("Select", field.SelectText, "SelectText");
			Assert.AreEqual (true, field.ShowCancelButton, "ShowCancelButton ");
			Assert.AreEqual (false, field.ShowDeleteButton, "ShowDeleteButton");
			Assert.AreEqual (false, field.ShowEditButton, "ShowEditButton");
			Assert.AreEqual (false, field.ShowInsertButton, "ShowInsertButton");
			Assert.AreEqual (false, field.ShowSelectButton , "ShowSelectButton");
			Assert.AreEqual ("", field.UpdateImageUrl, "UpdateImageUrl");
			Assert.AreEqual ("Update", field.UpdateText, "UpdateText");


		}

		[Test]
		public void CommandField_AssignProperty ()
		{
			PokerCommandField field = new PokerCommandField ();
			field.CancelImageUrl = "test";
			Assert.AreEqual ("test", field.CancelImageUrl, "CancelImageUrl");
			field.CancelText = "test";
			Assert.AreEqual ("test", field.CancelText, "CancelText");
			field.CausesValidation = false;
			Assert.AreEqual (false, field.CausesValidation, "CausesValidation");
			field.DeleteImageUrl = "test";	
			Assert.AreEqual ("test", field.DeleteImageUrl, "DeleteImageUrl");
			field.DeleteText = "test";
			Assert.AreEqual ("test", field.DeleteText, "DeleteText");
			field.EditImageUrl = "test";	
			Assert.AreEqual ("test", field.EditImageUrl, "EditImageUrl");
			field.EditText = "test";
			Assert.AreEqual ("test", field.EditText, "EditText");
			field.InsertImageUrl = "test";
			Assert.AreEqual ("test", field.InsertImageUrl, "InsertImageUrl");
			field.InsertText = "test";
			Assert.AreEqual ("test", field.InsertText, "InsertText");
			field.NewImageUrl = "test";
 			Assert.AreEqual ("test", field.NewImageUrl, "NewImageUrl");
			field.NewText = "test";
			Assert.AreEqual ("test", field.NewText, "NewText");
			field.SelectImageUrl = "test";
			Assert.AreEqual ("test", field.SelectImageUrl, "SelectImageUrl");
			field.SelectText = "test";
			Assert.AreEqual ("test", field.SelectText, "SelectText");
			field.ShowCancelButton = false;
			Assert.AreEqual (false, field.ShowCancelButton, "ShowCancelButton ");
			field.ShowDeleteButton = true;
			Assert.AreEqual (true, field.ShowDeleteButton, "ShowDeleteButton");
			field.ShowEditButton = true;
			Assert.AreEqual (true, field.ShowEditButton, "ShowEditButton");
			field.ShowInsertButton = true;
			Assert.AreEqual (true, field.ShowInsertButton, "ShowInsertButton");
			field.ShowSelectButton = true;
			Assert.AreEqual (true, field.ShowSelectButton, "ShowSelectButton");
			field.UpdateImageUrl = "test";
			Assert.AreEqual ("test", field.UpdateImageUrl, "UpdateImageUrl");
			field.UpdateText = "test";
			Assert.AreEqual ("test", field.UpdateText, "UpdateText");
		}

		[Test]
		public void CommandField_CopyProperty ()
		{
			PokerCommandField field = new PokerCommandField ();
			CommandField copy = new CommandField ();
			field.CancelImageUrl = "test";
			field.CancelText = "test";
			field.CausesValidation = false;
			field.DeleteImageUrl = "test";
			field.DeleteText = "test";
			field.EditImageUrl = "test";
			field.EditText = "test";
			field.InsertImageUrl = "test";
			field.InsertText = "test";
			field.NewImageUrl = "test";
			field.NewText = "test";
			field.SelectImageUrl = "test";
			field.SelectText = "test";
			field.ShowCancelButton = false;
			field.ShowDeleteButton = true;
			field.ShowEditButton = true;
			field.ShowInsertButton = true;
			field.ShowSelectButton = true;
			field.UpdateImageUrl = "test";
			field.UpdateText = "test";

			field.DoCopyProperties (copy);
			Assert.AreEqual (false, copy.CausesValidation, "CausesValidation");
			Assert.AreEqual ("test", copy.DeleteImageUrl, "DeleteImageUrl");
			Assert.AreEqual ("test", copy.DeleteText, "DeleteText");
			Assert.AreEqual ("test", copy.EditImageUrl, "EditImageUrl");
			Assert.AreEqual ("test", copy.EditText, "EditText");
			Assert.AreEqual ("test", copy.InsertImageUrl, "InsertImageUrl");
			Assert.AreEqual ("test", copy.InsertText, "InsertText");
			Assert.AreEqual ("test", copy.NewImageUrl, "NewImageUrl");
			Assert.AreEqual ("test", copy.NewText, "NewText");
			Assert.AreEqual ("test", copy.SelectImageUrl, "SelectImageUrl");
			Assert.AreEqual ("test", copy.SelectText, "SelectText");
			Assert.AreEqual (false, copy.ShowCancelButton, "ShowCancelButton ");
			Assert.AreEqual (true, copy.ShowDeleteButton, "ShowDeleteButton");
			Assert.AreEqual (true, copy.ShowEditButton, "ShowEditButton");
			Assert.AreEqual (true, copy.ShowInsertButton, "ShowInsertButton");
			Assert.AreEqual (true, copy.ShowSelectButton, "ShowSelectButton");
			Assert.AreEqual ("test", copy.UpdateImageUrl, "UpdateImageUrl");
			Assert.AreEqual ("test", copy.UpdateText, "UpdateText");
			Assert.AreEqual ("test", copy.CancelImageUrl, "CancelImageUrl");
			Assert.AreEqual ("test", copy.CancelText, "CancelText");

		}

		[Test]
		public void CommandField_CreateField ()
		{
			PokerCommandField field = new PokerCommandField ();
			DataControlField newfield = field.DoCreateField ();
			if (!(newfield is CommandField)) {
				Assert.Fail ("New CommandField was not created");
			}

		}

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void CommandField_ValidateSupportsCallback ()
		{
			PokerCommandField field = new PokerCommandField ();
			field.ShowSelectButton = true;
			field.Initialize (true, new Control());
			field.ValidateSupportsCallback ();
		}


		[Test]
		public void CommandField_ValidateSupportsCallback_pass ()
		{
			PokerCommandField field = new PokerCommandField ();
			field.ShowSelectButton = false;
			field.Initialize (true, new Control ());
			field.ValidateSupportsCallback ();
		}
	}
}
#endif