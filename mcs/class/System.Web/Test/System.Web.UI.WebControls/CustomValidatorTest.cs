//
// Tests for System.Web.UI.WebControls.CustomValidator
//
// Author:
//	Peter Dennis Bartok (pbartok@novell.com)
//      Klain Yoni          (yonik@mainsoft.com)
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
//

using NUnit.Framework;
using System;
using System.Collections;
using System.Drawing;
using System.IO;
using System.Globalization;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using MonoTests.stand_alone.WebHarness;

namespace MonoTests.System.Web.UI.WebControls
{
	[TestFixture]	
	public class CustomValidatorTest : ValidatorTest {
		private bool	bool_result;

		public class CustomValidatorTestClass : CustomValidator {
			public CustomValidatorTestClass () {
				TrackViewState();
			}

			public bool AreControlPropertiesValid ()
			{
				return ControlPropertiesValid ();
			}

			public object SaveState () {
				return SaveViewState ();
			}

			public void LoadState (object o) {
				LoadViewState (o);
			}

			public void SetTrackingVS () {
				TrackViewState ();
			}

			public void CheckProperties () {
				ControlPropertiesValid ();
			}

			public bool DoEvaluateIsValid () {
				return EvaluateIsValid ();
			}

			protected new bool RenderUplevel {
				get {
					return true;
				}
			}

			public void CallInit() {
				base.OnInit(EventArgs.Empty);
			}

			public bool Evaluate() {
				return base.EvaluateIsValid();
			}

			public string Render () {
				HtmlTextWriter	writer;

				writer = CustomValidatorTest.GetWriter();
				base.Render (writer);
				return writer.InnerWriter.ToString ();
			}
		}

		private static HtmlTextWriter GetWriter () {
			StringWriter sw = new StringWriter ();
			sw.NewLine = "\n";
			return new HtmlTextWriter (sw);
		}

		private void ServerValidateMethod(object sender, ServerValidateEventArgs e) {
			bool_result = e.IsValid;
		}

		[Test]
		public void EventDefaults ()
		{
			CustomValidatorTestClass	c;
			TextBox				t;

			c = new CustomValidatorTestClass();

			Assert.AreEqual(true, c.Evaluate(), "E1");
			Assert.AreEqual(false, bool_result, "E2");

			c.ServerValidate += new ServerValidateEventHandler(ServerValidateMethod);
			bool_result = false;

			Assert.AreEqual(true, c.Evaluate(), "E3");
			Assert.AreEqual(true, bool_result, "E4");

			StartValidationTest(c);
			t = SetValidationTextBox("textbox", "3");
			bool_result = false;

			Assert.AreEqual(true, c.Evaluate(), "E5");
			Assert.AreEqual(true, bool_result, "E6");
		}

		[Test]
		public void Defaults () {
			CustomValidatorTestClass	c;

			c= new CustomValidatorTestClass();
			Assert.AreEqual(string.Empty, c.ClientValidationFunction , "D1");

			c.ClientValidationFunction = "Hurra, hurra, die Schule brennt";
			Assert.AreEqual("Hurra, hurra, die Schule brennt", c.ClientValidationFunction , "D1");
		}

		[Test]
		public void DefaultsNotWorking ()
		{
			CustomValidatorTestClass c = new CustomValidatorTestClass ();
			Assert.AreEqual (false, c.ValidateEmptyText, "ValidateEmptyText");
		}


		// Variable for checking events 
		private bool checker;

		[Test]
		public void ValidateEmptyText ()
		{
			CustomValidatorTestClass c = new CustomValidatorTestClass ();
			Page p = new Page ();
			TextBox tb = new TextBox ();
			tb.ID = "tb";
			
			CustomValidator v = new CustomValidator ();
			v.ServerValidate += new ServerValidateEventHandler (ServerValidate);
			v.ControlToValidate = "tb";
			p.Controls.Add (tb);
			p.Controls.Add (v);

			v.Validate ();
			Assert.AreEqual (false, checker, "Validate with ValidateEmptyText = false");
			v.ValidateEmptyText = true;
			v.Validate ();
			Assert.AreEqual (true, checker, "Validate with ValidateEmptyText = true");
		}

		void ServerValidate (object source, ServerValidateEventArgs value)
		{
			checker = true;
		}


		[Test]
		public void Render () {
			CustomValidatorTestClass	c;
			TextBox				t;

			c = new CustomValidatorTestClass();
			StartValidationTest(c);
			t = SetValidationTextBox("textbox", "3");
			c.Validate();
			c.ErrorMessage = "aw shucks";
			c.Display = ValidatorDisplay.Static;
			c.Enabled = true;
			c.EnableViewState = true;


			c.ClientValidationFunction = "Father to a sister of thought";
		}

		class Poker : CustomValidator
		{
			public string Render ()
			{
				HtmlTextWriter writer;
				writer = CustomValidatorTest.GetWriter ();
				base.Render (writer);
				return writer.InnerWriter.ToString ();
			}
		}

		[Test]
		public void Render_2_0 ()
		{
			Poker c = new Poker();
			c.ErrorMessage = "aw shucks";
			c.Display = ValidatorDisplay.Static;
			c.Enabled = true;
			c.EnableViewState = true;
			string html = c.Render ();
			HtmlDiff.AssertAreEqual ("<span>aw shucks</span>", html, "Render#1");
		}

		

		[Test]
		public void EmptyControlName ()
		{
			Page page = new Page ();
			HtmlForm form = new HtmlForm ();
			CustomValidatorTestClass tc = new CustomValidatorTestClass ();
			page.Controls.Add (form);
			form.Controls.Add (tc);
			Assert.IsTrue (tc.AreControlPropertiesValid (), "#01");
		}
	}
}
