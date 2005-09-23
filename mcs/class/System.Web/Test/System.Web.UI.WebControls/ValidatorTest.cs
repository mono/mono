//
// Tests for System.Web.UI.WebControls.Validator
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
	public class ValidatorTest
	{
		protected Page Page;
		protected BaseValidator Validator;

		public ValidatorTest ()
		{
		}

		public void StartValidationTest (BaseValidator validator)
		{
			Page = new Page();
			Validator = validator;
			Validator.Page = Page;
			Page.Controls.Add (Validator);
		}

		public void StopValidationTest ()
		{
			Page = null;
			Validator = null;
		}

		public TextBox SetValidationTextBox (string name, string value)
		{
			TextBox box = new TextBox ();
			box.ID = name;
			box.Text = value;
			Validator.ControlToValidate = name;
			Page.Controls.Add (box);

			return box;
		}

		public TextBox AddTextBox (string name, string value)
		{
			TextBox box = new TextBox ();
			box.ID = name;
			box.Text = value;

			Page.Controls.Add (box);

			return box;
		}
	}
}
