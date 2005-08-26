//
// Tests for System.Web.UI.WebControls.CompareValidator 
//
// Author:
//	Chris Toshok (toshok@novell.com)
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
using System.IO;
using System.Globalization;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace MonoTests.System.Web.UI.WebControls
{
	class CompareValidatorPoker : CompareValidator {
		public CompareValidatorPoker ()
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

		public void CheckControlProperties ()
		{
			ControlPropertiesValid ();
		}

		public bool DoEvaluateIsValid ()
		{
			return EvaluateIsValid ();
		}
	}
	
	
	[TestFixture]	
	public class CompareValidatorTest : ValidatorTest {

		[Test]
		public void CompareValidator_ViewState ()
		{
			CompareValidatorPoker p = new CompareValidatorPoker ();

			Assert.AreEqual (p.ControlToCompare, String.Empty, "A1");
			Assert.AreEqual (p.ValueToCompare, String.Empty, "A2");

			p.ControlToCompare = "TextBox";
			Assert.AreEqual ("TextBox", p.ControlToCompare, "A3");

			p.ValueToCompare = "hello mom";
			Assert.AreEqual ("hello mom", p.ValueToCompare, "A4");

			object state = p.SaveState ();

			CompareValidatorPoker copy = new CompareValidatorPoker ();
			copy.LoadState (state);
			Assert.AreEqual ("TextBox", copy.ControlToCompare, "A5");
			Assert.AreEqual ("hello mom", copy.ValueToCompare, "A6");
		}

		[Test]
		public void CompareValidator_ValueToCompareTest ()
		{
			CompareValidatorPoker p = new CompareValidatorPoker ();
			TextBox box;

			StartValidationTest (p);

			p.Type = ValidationDataType.Integer;
			p.Operator = ValidationCompareOperator.Equal;
			p.ValueToCompare = "10";

			box = SetValidationTextBox ("textbox", "10");
			Assert.IsTrue (p.DoEvaluateIsValid (), "B1");

			box.Text = "11";
			Assert.IsFalse (p.DoEvaluateIsValid (), "B2");

			StopValidationTest();
		}

		[Test]
		public void CompareValidator_ControlToCompareTest ()
		{
			CompareValidatorPoker p = new CompareValidatorPoker ();
			TextBox box, box2;

			StartValidationTest (p);

			p.Type = ValidationDataType.Integer;
			p.Operator = ValidationCompareOperator.Equal;
			p.ControlToCompare = "textbox2";

			box = SetValidationTextBox ("textbox", "10");
			box2 = AddTextBox ("textbox2", "10");

			Assert.IsTrue (p.DoEvaluateIsValid (), "C1");

			box.Text = "11";
			Assert.IsFalse (p.DoEvaluateIsValid (), "C2");

			box2.Text = "";
			Assert.IsTrue (p.DoEvaluateIsValid (), "C3");
			
			StopValidationTest();
		}
	}
}

		
