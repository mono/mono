//
// Tests for System.Web.UI.WebControls.RangeValidator
//
// Author:
//	Peter Dennis Bartok (pbartok@novell.com)
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
using WebSpace = System.Web;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace MonoTests.System.Web.UI.WebControls
{

	[TestFixture]	
	public class RangeValidatorTest : ValidatorTest {
		public class RangeValidatorTestClass : RangeValidator {
			public RangeValidatorTestClass () {
				TrackViewState();
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

			public void CallInit() {
				base.OnInit(EventArgs.Empty);
			}

			public string Render () {
				HtmlTextWriter	writer;

				writer = RangeValidatorTest.GetWriter();
				base.Render (writer);
				return writer.InnerWriter.ToString ();
			}

			public bool UpRender () {
				return base.RenderUplevel;
			}
		}

		private static HtmlTextWriter GetWriter () {
			StringWriter sw = new StringWriter ();
			sw.NewLine = "\n";
			return new HtmlTextWriter (sw);
		}

		[Test]
		public void State () {
			RangeValidatorTestClass p;
			RangeValidatorTestClass p_copy;
			object			state;

			p = new RangeValidatorTestClass();

			Assert.AreEqual (p.ControlToValidate, String.Empty, "S1");
			Assert.AreEqual (p.MinimumValue, String.Empty, "S2");
			Assert.AreEqual (p.MaximumValue, String.Empty, "S3");

			p.ControlToValidate = "TextBox";
			Assert.AreEqual ("TextBox", p.ControlToValidate, "S4");

			p.MinimumValue = "123";
			Assert.AreEqual ("123", p.MinimumValue, "S5");

			p.MaximumValue = "456";
			Assert.AreEqual ("456", p.MaximumValue, "S6");

			state = p.SaveState ();

			p_copy = new RangeValidatorTestClass ();
			p_copy.LoadState (state);
			Assert.AreEqual ("TextBox", p.ControlToValidate, "S7");
			Assert.AreEqual ("123", p.MinimumValue, "S8");
			Assert.AreEqual ("456", p.MaximumValue, "S9");
		}
			
		[Test]
		public void Defaults ()
		{
			RangeValidatorTestClass p;

			p = new RangeValidatorTestClass();

			Assert.AreEqual (String.Empty, p.ControlToValidate, "D1");
			Assert.AreEqual (String.Empty, p.MinimumValue, "D2");
			Assert.AreEqual (String.Empty, p.MaximumValue, "D3");
			Assert.AreEqual (true, p.EnableClientScript, "D4");
			Assert.AreEqual (false, p.UpRender(), "D5");
		}

		[Test]
		public void Render () {
			RangeValidatorTestClass p;
			TextBox			t;

			p = new RangeValidatorTestClass();
			StartValidationTest(p);
			p.Type = ValidationDataType.Integer;

			Assert.AreEqual (false, p.UpRender(), "R0");

			t = SetValidationTextBox("textbox", "3");

			p.MinimumValue = "1";
			p.MaximumValue = "2";
			p.Validate();
			p.ErrorMessage = "aw shucks";
			p.Display = ValidatorDisplay.Static;
			p.Enabled = true;
			p.EnableViewState = true;

			Assert.AreEqual("<span>aw shucks</span>", p.Render(), "R1");
		}


		[Test]
		public void ValidateRangeTest () {
			RangeValidatorTestClass p;
			TextBox			t;

			p = new RangeValidatorTestClass();

			StartValidationTest(p);
			p.Type = ValidationDataType.Integer;
			p.MinimumValue = "2";
			p.MaximumValue = "4";

			t = SetValidationTextBox("textbox", "1");
			Assert.AreEqual(false, p.DoEvaluateIsValid(), "V1");

			t.Text = "2";
			Assert.AreEqual(true, p.DoEvaluateIsValid(), "V2");

			t.Text = "3";
			Assert.AreEqual(true, p.DoEvaluateIsValid(), "V3");

			t.Text = "4";
			Assert.AreEqual(true, p.DoEvaluateIsValid(), "V4");

			t.Text = "5";
			Assert.AreEqual(false, p.DoEvaluateIsValid(), "V5");

			StopValidationTest();
		}

		[Test]
		[ExpectedException(typeof(WebSpace.HttpException))]
		public void Exception1Test () {
			RangeValidatorTestClass p;
			TextBox			t;

			p = new RangeValidatorTestClass();
			p.CheckProperties();
		}

		[Test]
		[ExpectedException(typeof(WebSpace.HttpException))]
		public void Exception2Test () {
			RangeValidatorTestClass p;
			TextBox			t;

			p = new RangeValidatorTestClass();

			StartValidationTest(p);
			t = SetValidationTextBox("textbox", "1");
			p.Type = ValidationDataType.Integer;
			p.MaximumValue = "1";

			p.CheckProperties();
		}

		[Test]
		[ExpectedException(typeof(WebSpace.HttpException))]
		public void Exception3Test () {
			RangeValidatorTestClass p;
			TextBox			t;

			p = new RangeValidatorTestClass();

			StartValidationTest(p);
			t = SetValidationTextBox("textbox", "1");
			p.Type = ValidationDataType.Integer;
			p.MinimumValue = "1";

			p.CheckProperties();
		}

		[Test]
		[ExpectedException(typeof(WebSpace.HttpException))]
		public void Exception4Test () {
			RangeValidatorTestClass p;
			TextBox			t;

			p = new RangeValidatorTestClass();

			StartValidationTest(p);
			t = SetValidationTextBox("textbox", "1");
			p.Type = ValidationDataType.Date;
			p.MaximumValue = "1";

			p.CheckProperties();
		}

		[Test]
		[ExpectedException(typeof(WebSpace.HttpException))]
		public void Exception5Test () {
			RangeValidatorTestClass p;
			TextBox			t;

			p = new RangeValidatorTestClass();

			StartValidationTest(p);
			t = SetValidationTextBox("textbox", "1");
			p.Type = ValidationDataType.Integer;
			p.MinimumValue = "3";
			p.MaximumValue = "1";

			p.CheckProperties();
		}

		[Test]
		[ExpectedException(typeof(WebSpace.HttpException))]
		public void Exception6Test () {
			RangeValidatorTestClass p;
			TextBox			t;

			p = new RangeValidatorTestClass();

			StartValidationTest(p);
			t = SetValidationTextBox("textbox", "1");
			p.Type = ValidationDataType.Date;
			p.MinimumValue = "01/01/02";
			p.MaximumValue = "01/01/01";

			p.CheckProperties();
		}

		[Test]
		//[ExpectedException(typeof(WebSpace.HttpException))]
		public void NoException7Test () {
			RangeValidatorTestClass p;
			TextBox			t;

			p = new RangeValidatorTestClass();

			StartValidationTest(p);
			t = SetValidationTextBox("textbox", "1");
			p.Type = ValidationDataType.String;
			p.MinimumValue = "lasdjflk jasldfj ";
			p.MaximumValue = "s.dfjalsd fl;asdf";

			p.CheckProperties();
		}

		[Test]
		[ExpectedException(typeof(WebSpace.HttpException))]
		public void Exception8Test () {
			RangeValidatorTestClass p;
			TextBox			t;

			p = new RangeValidatorTestClass();

			StartValidationTest(p);
			p.Type = ValidationDataType.Integer;
			p.MinimumValue = "2";
			p.MaximumValue = "4";

			t = SetValidationTextBox("textbox", "1");
			t = SetValidationTextBox("textbox", "2");
			p.DoEvaluateIsValid();
		}
	}
}
