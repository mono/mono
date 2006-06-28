//
// Tests for System.Web.UI.WebControls.RegularExpressionValidator 
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
	class REValidatorPoker : RegularExpressionValidator {
		public REValidatorPoker ()
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

		public bool DoEvaluateIsValid ()
		{
			return EvaluateIsValid ();
		}

	}
	
	
	[TestFixture]	
	public class RegularExpressionValidatorTest : ValidatorTest {

		[Test]
		public void REValidator_ViewState ()
		{
			REValidatorPoker p = new REValidatorPoker ();

			Assert.AreEqual (p.ValidationExpression, String.Empty, "A1");

			p.ValidationExpression = "\\d{5}";
			Assert.AreEqual ("\\d{5}", p.ValidationExpression, "A2");

			object state = p.SaveState ();

			REValidatorPoker copy = new REValidatorPoker ();
			copy.LoadState (state);
			Assert.AreEqual ("\\d{5}", copy.ValidationExpression, "A3");
		}

		[Test]
		public void REValidator_ValidationTests ()
		{
			REValidatorPoker p = new REValidatorPoker ();
			TextBox box;

			StartValidationTest (p);

			p.ValidationExpression = "\\d{5}";
			box = SetValidationTextBox ("textbox", "94117");
			Assert.IsTrue (p.DoEvaluateIsValid (), "B1");

			box.Text = "9410";
			Assert.IsFalse (p.DoEvaluateIsValid (), "B2");
			
			box.Text = "12345 ";
			Assert.IsFalse (p.DoEvaluateIsValid (), "B3");

			box.Text = " 12345";
			Assert.IsFalse (p.DoEvaluateIsValid (), "B4");
			
			box.Text = " 12345 ";
			Assert.IsFalse (p.DoEvaluateIsValid (), "B5");
			
			p.ValidationExpression = "^\\d{5}$";
			box.Text = "12345";
			Assert.IsTrue (p.DoEvaluateIsValid (), "B6");

			StopValidationTest();
		}
	}
}

		
