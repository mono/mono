//
// 
//
// Authors:
//      Alexander Olk (alex.olk@googlemail.com)
//

using System;
using System.Collections;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;

using NUnit.Framework;

namespace MonoTests.System.Windows.Forms
{
	[TestFixture]
	public class UpDownTest
	{
		[SetUp]
		public void SetUp ()
		{
			text_changed = 0;
			value_changed = 0;
		}

		[Test]
		public void UpDownBasePropTest ()
		{
			NumericUpDown n1 = new NumericUpDown ();
			
			Assert.AreEqual (BorderStyle.Fixed3D, n1.BorderStyle, "#1");
			Assert.AreEqual (true, n1.InterceptArrowKeys, "#2");
			Assert.AreEqual (LeftRightAlignment.Right, n1.UpDownAlign, "#3");
		}

		[Test]
		public void ToStringTest ()
		{
			NumericUpDown n1 = new NumericUpDown ();

			Assert.AreEqual ("System.Windows.Forms.NumericUpDown, Minimum = 0, Maximum = 100", n1.ToString (), "1");

			n1.Minimum = 0.33m;
			n1.Maximum = 100.33m;
			Assert.AreEqual (string.Format (CultureInfo.CurrentCulture,
				"System.Windows.Forms.NumericUpDown, Minimum = {0}, Maximum = {1}",
				0.33, 100.33), n1.ToString (), "2");
		}
		
		[Test]
		public void NumericUpDownStandardPropTest ()
		{
			NumericUpDown n1 = new NumericUpDown ();
			
			Assert.AreEqual (100, n1.Maximum, "#4");
			Assert.AreEqual (0, n1.Minimum, "#5");
			Assert.AreEqual (0, n1.Value, "#6");
			Assert.AreEqual (0, n1.DecimalPlaces, "#7");
			Assert.IsFalse (n1.Hexadecimal, "#8");
			Assert.IsFalse (n1.ThousandsSeparator, "#9");
			Assert.AreEqual (1, n1.Increment, "#10");
		}

		[Test]
		public void NumericUpDownEnhancedPropTest ()
		{
			NumericUpDown n1 = new NumericUpDown ();
			
			n1.Minimum = 200;
			Assert.AreEqual (200, n1.Maximum, "#11");
			Assert.AreEqual (200, n1.Value, "#12");
			
			n1.Minimum = 100;
			n1.Maximum = 50;
			Assert.AreEqual (50, n1.Minimum, "#13");
			
			n1.Minimum = 0;
			n1.Maximum = 100;
			n1.Value = 90;
			n1.Maximum = 50;
			Assert.AreEqual (50, n1.Value, "#14");
			
			n1.Minimum = 0;
			n1.Maximum = 100;
			n1.Value = 90;
			n1.DownButton ();
			Assert.AreEqual (89, n1.Value, "#15");
			
			n1.UpButton ();
			Assert.AreEqual (90, n1.Value, "#16");
		}

		[Test]
		[Category ("NotWorking")]
		public void NumericUpDownEditValidateTest ()
		{
			NumericNew nn = new NumericNew ();
			Assert.IsTrue (nn.update_edit_text_called, "#17");
			
			Assert.IsFalse (nn.user_edit, "#18");
			
			nn.Reset ();
			nn.user_edit = true;
			nn.Text = "10";
			Assert.IsTrue (nn.validate_edit_text_called, "#19");
			Assert.IsTrue (nn.update_edit_text_called, "#20");
			
			nn.Reset ();
			nn.user_edit = false;
			nn.Text = "11";
			Assert.IsTrue (nn.validate_edit_text_called, "#21");
			Assert.IsTrue (nn.update_edit_text_called, "#22");
			
			nn.DownButton ();
			Assert.AreEqual (10, nn.Value, "#23");
		}

		[Test]
		[Category ("NotWorking")]
		public void NumericUpDown_BeginInit ()
		{
			NumericNew nud = new NumericNew ();
			nud.TextChanged += new EventHandler (NumericUpDown_TextChanged);
			nud.ValueChanged += new EventHandler (NumericUpDown_ValueChanged);
			Assert.AreEqual (2, nud.CallStack.Count, "#A1");
			Assert.AreEqual ("ValidateEditText", nud.CallStack [0], "#A2");
			Assert.AreEqual ("UpdateEditText", nud.CallStack [1], "#A3");
			Assert.AreEqual (0, nud.Value, "#A4");
			Assert.AreEqual (2, nud.CallStack.Count, "#A5");
			Assert.AreEqual (0, value_changed, "#A6");
			Assert.AreEqual (0, text_changed, "#A7");

			nud.BeginInit ();
			Assert.AreEqual (2, nud.CallStack.Count, "#B1");
			nud.Value = 10;
			Assert.AreEqual (3, nud.CallStack.Count, "#B2");
			Assert.AreEqual ("UpdateEditText", nud.CallStack [2], "#B3");
			Assert.AreEqual ("0", nud.Text, "#B4");
			Assert.AreEqual (10, nud.Value, "#B5");
			Assert.AreEqual (3, nud.CallStack.Count, "#B6");
			Assert.AreEqual (1, value_changed, "#B7");
			Assert.AreEqual (0, text_changed, "#B8");
			nud.EndInit ();
			Assert.AreEqual (4, nud.CallStack.Count, "#B9");
			Assert.AreEqual ("UpdateEditText", nud.CallStack [3], "#B10");
			Assert.AreEqual (1, text_changed, "#B11");
			Assert.AreEqual ("10", nud.Text, "#B12");
			Assert.AreEqual (10, nud.Value, "#B13");
			Assert.AreEqual (4, nud.CallStack.Count, "#B14");
			Assert.AreEqual (1, value_changed, "#B15");
			Assert.AreEqual (1, text_changed, "#B16");

			// multiple calls to BeginInit are undone by a single EndInit call
			nud.BeginInit ();
			nud.BeginInit ();
			Assert.AreEqual (4, nud.CallStack.Count, "#C1");
			nud.Value = 20;
			Assert.AreEqual (5, nud.CallStack.Count, "#C2");
			Assert.AreEqual ("UpdateEditText", nud.CallStack [4], "#C3");
			Assert.AreEqual ("10", nud.Text, "#C4");
			Assert.AreEqual (20, nud.Value, "#C5");
			Assert.AreEqual (5, nud.CallStack.Count, "#C6");
			Assert.AreEqual (2, value_changed, "#C7");
			Assert.AreEqual (1, text_changed, "#C8");
			nud.EndInit ();
			Assert.AreEqual (6, nud.CallStack.Count, "#C9");
			Assert.AreEqual ("UpdateEditText", nud.CallStack [5], "#C10");
			Assert.AreEqual (2, text_changed, "#C11");
			Assert.AreEqual ("20", nud.Text, "#C12");
			Assert.AreEqual (20, nud.Value, "#C13");
			Assert.AreEqual (6, nud.CallStack.Count, "#C14");
			Assert.AreEqual (2, value_changed, "#C15");
			Assert.AreEqual (2, text_changed, "#C16");
		}

		[Test]
		[Category ("NotWorking")]
		public void NumericUpDown_ChangingText ()
		{
			NumericNew nud = new NumericNew ();
			nud.TextChanged += new EventHandler (NumericUpDown_TextChanged);
			nud.ValueChanged += new EventHandler (NumericUpDown_ValueChanged);
			Assert.IsFalse (nud.changing_text, "#A1");
			Assert.IsFalse (nud.user_edit, "#A2");
			Assert.AreEqual (0, text_changed, "#A3");

			nud.Text = "1";
			Assert.IsFalse (nud.changing_text, "#B1");
			Assert.IsFalse (nud.user_edit, "#B2");
			Assert.AreEqual (5, nud.CallStack.Count, "#B3");
			Assert.AreEqual (1, text_changed, "#B4");

			nud.changing_text = true;
			nud.Text = "2";
			Assert.IsFalse (nud.changing_text, "#C1");
			Assert.IsFalse (nud.user_edit, "#C2");
			Assert.AreEqual (5, nud.CallStack.Count, "#C3");
			Assert.AreEqual (2, text_changed, "#C4");
		}

		[Test]
		[Category ("NotWorking")]
		public void NumericUpDown_EndInit ()
		{
			NumericNew nud = new NumericNew ();
			nud.TextChanged += new EventHandler (NumericUpDown_TextChanged);
			nud.ValueChanged += new EventHandler (NumericUpDown_ValueChanged);
			Assert.AreEqual (2, nud.CallStack.Count, "#A1");
			Assert.AreEqual ("ValidateEditText", nud.CallStack [0], "#A2");
			Assert.AreEqual ("UpdateEditText", nud.CallStack [1], "#A3");
			Assert.AreEqual (0, nud.Value, "#A4");
			Assert.AreEqual (2, nud.CallStack.Count, "#A5");
			Assert.AreEqual (0, value_changed, "#A6");
			Assert.AreEqual (0, text_changed, "#A7");

			// EndInit without corresponding BeginInit
			nud.EndInit ();
			Assert.AreEqual (3, nud.CallStack.Count, "#B1");
			Assert.AreEqual ("UpdateEditText", nud.CallStack [1], "#B2");
			Assert.AreEqual (0, nud.Value, "#B3");
			Assert.AreEqual (3, nud.CallStack.Count, "#B4");
			Assert.AreEqual (0, value_changed, "#B5");
			Assert.AreEqual (0, text_changed, "#B6");
		}

		[Test]
		[Category ("NotWorking")]
		public void NumericUpDown_UpButton ()
		{
			NumericNew nud = new NumericNew ();
			nud.TextChanged += new EventHandler (NumericUpDown_TextChanged);
			nud.ValueChanged += new EventHandler (NumericUpDown_ValueChanged);
			nud.UpButton ();
			Assert.AreEqual (3, nud.CallStack.Count, "#A1");
			Assert.AreEqual ("ValidateEditText", nud.CallStack [0], "#A2");
			Assert.AreEqual ("UpdateEditText", nud.CallStack [1], "#A3");
			Assert.AreEqual ("UpdateEditText", nud.CallStack [2], "#A4");
			Assert.AreEqual (1, value_changed, "#A5");
			Assert.AreEqual (1, nud.Value, "#A6");
			Assert.AreEqual (3, nud.CallStack.Count, "#A7");
			Assert.AreEqual (1, value_changed, "#A8");
			Assert.AreEqual (1, text_changed, "#A9");

			nud.Text = "5";
			nud.UpButton ();
			Assert.AreEqual (7, nud.CallStack.Count, "#B1");
			Assert.AreEqual ("ValidateEditText", nud.CallStack [3], "#B2");
			Assert.AreEqual ("UpdateEditText", nud.CallStack [4], "#B3");
			Assert.AreEqual ("UpdateEditText", nud.CallStack [5], "#B4");
			Assert.AreEqual ("UpdateEditText", nud.CallStack [6], "#B5");
			Assert.AreEqual (3, value_changed, "#B6");
			Assert.AreEqual (6, nud.Value, "#B7");
			Assert.AreEqual ("6", nud.Text, "#B8");
			Assert.AreEqual (7, nud.CallStack.Count, "#B9");
			Assert.AreEqual (3, value_changed, "#B10");
			Assert.AreEqual (3, text_changed, "#B11");

			nud.Text = "7";
			nud.user_edit = true;
			nud.UpButton ();
			Assert.AreEqual (11, nud.CallStack.Count, "#C1");
			Assert.AreEqual ("ValidateEditText", nud.CallStack [7], "#C2");
			Assert.AreEqual ("UpdateEditText", nud.CallStack [8], "#C3");
			Assert.AreEqual ("UpdateEditText", nud.CallStack [9], "#C4");
			Assert.AreEqual ("UpdateEditText", nud.CallStack [10], "#C5");
			Assert.AreEqual (5, value_changed, "#C6");
			Assert.AreEqual (8, nud.Value, "#C7");
			Assert.AreEqual (11, nud.CallStack.Count, "#C8");
			Assert.AreEqual (5, value_changed, "#C9");
			Assert.AreEqual (5, text_changed, "#C10");
			nud.user_edit = false;
			Assert.AreEqual ("8", nud.Text, "#C11");
			Assert.AreEqual (11, nud.CallStack.Count, "#C12");
			Assert.AreEqual (5, value_changed, "#C13");
			Assert.AreEqual (5, text_changed, "#C14");

			nud.user_edit = false;
			nud.Text = "555";
			nud.user_edit = true;
			nud.UpButton ();
			Assert.AreEqual (14, nud.CallStack.Count, "#D1");
			Assert.AreEqual ("ValidateEditText", nud.CallStack [11], "#D2");
			Assert.AreEqual ("UpdateEditText", nud.CallStack [12], "#D3");
			Assert.AreEqual ("UpdateEditText", nud.CallStack [13], "#D4");
			Assert.AreEqual (6, value_changed, "#D5");
			Assert.AreEqual (100, nud.Value, "#D6");
			Assert.AreEqual (14, nud.CallStack.Count, "#D7");
			Assert.AreEqual (6, value_changed, "#D8");
			Assert.AreEqual (7, text_changed, "#D9");
			nud.user_edit = false;
			Assert.AreEqual ("100", nud.Text, "#D10");
			Assert.AreEqual (14, nud.CallStack.Count, "#D11");
			Assert.AreEqual (6, value_changed, "#D12");
			Assert.AreEqual (7, text_changed, "#D13");
		}

		[Test]
		[Category ("NotWorking")]
		public void NumericUpDown_Value ()
		{
			// obtain Value when UserEdit is false
			NumericNew nud = new NumericNew ();
			nud.TextChanged += new EventHandler (NumericUpDown_TextChanged);
			nud.ValueChanged += new EventHandler (NumericUpDown_ValueChanged);
			Assert.AreEqual (2, nud.CallStack.Count, "#A1");
			Assert.AreEqual ("ValidateEditText", nud.CallStack [0], "#A2");
			Assert.AreEqual ("UpdateEditText", nud.CallStack [1], "#A3");
			Assert.AreEqual (0, nud.Value, "#A4");
			Assert.AreEqual (2, nud.CallStack.Count, "#A5");
			Assert.AreEqual (0, value_changed, "#A6");
			Assert.AreEqual (0, text_changed, "#A7");

			// obtain Value when UserEdit is true
			nud.user_edit = true;
			Assert.AreEqual (2, nud.CallStack.Count, "#B1");
			Assert.AreEqual (0, nud.Value, "#B2");
			Assert.AreEqual (4, nud.CallStack.Count, "#B3");
			Assert.AreEqual ("ValidateEditText", nud.CallStack [2], "#B4");
			Assert.AreEqual ("UpdateEditText", nud.CallStack [3], "#B5");
			Assert.AreEqual (0, value_changed, "#B6");
			Assert.AreEqual (0, text_changed, "#B7");

			// modify Value when UserEdit is false
			nud.user_edit = false;
			nud.Value = 10;
			Assert.AreEqual (5, nud.CallStack.Count, "#C1");
			Assert.AreEqual ("UpdateEditText", nud.CallStack [4], "#C2");
			Assert.AreEqual (1, value_changed, "#C3");
			Assert.AreEqual (1, text_changed, "#C4");
			Assert.AreEqual (10, nud.Value, "#C5");
			Assert.AreEqual (5, nud.CallStack.Count, "#C6");
			Assert.AreEqual (1, value_changed, "#C7");
			Assert.AreEqual (1, text_changed, "#C8");

			// setting same Value
			nud.Value = 10;
			Assert.AreEqual (5, nud.CallStack.Count, "#D1");
			Assert.AreEqual (1, value_changed, "#D2");
			Assert.AreEqual (10, nud.Value, "#D3");
			Assert.AreEqual (5, nud.CallStack.Count, "#D4");
			Assert.AreEqual (1, value_changed, "#D5");
			Assert.AreEqual (1, text_changed, "#D6");

			// modify Value when UserEdit is true
			nud.user_edit = true;
			nud.Value = 20;
			Assert.AreEqual (7, nud.CallStack.Count, "#E1");
			Assert.AreEqual ("UpdateEditText", nud.CallStack [5], "#E2");
			Assert.AreEqual ("UpdateEditText", nud.CallStack [6], "#E3");
			Assert.AreEqual (3, value_changed, "#E4");
			Assert.AreEqual (1, text_changed, "#E5");
			nud.user_edit = false; // reset UserEdit to avoid Value being parsed from Text
			Assert.AreEqual (10, nud.Value, "#E6");
			Assert.AreEqual (7, nud.CallStack.Count, "#E7");
			Assert.AreEqual (3, value_changed, "#E8");
			Assert.AreEqual (1, text_changed, "#E9");
		}

		[Test]
		[Category ("NotWorking")]
		public void NumericUpDown_Value_Invalid ()
		{
			NumericNew nud = new NumericNew ();

			try {
				nud.Value = 1000;
				Assert.Fail ("#A1");
			} catch (ArgumentException ex) {
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#A2");
				Assert.IsNotNull (ex.Message, "#A3");
				Assert.IsNull (ex.ParamName, "#A4");
				Assert.IsNull (ex.InnerException, "#A5");
			}

			try {
				nud.Value = 1000;
				Assert.Fail ("#B1");
			} catch (ArgumentException ex) {
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#B2");
				Assert.IsNotNull (ex.Message, "#B3");
				Assert.IsNull (ex.ParamName, "#B4");
				Assert.IsNull (ex.InnerException, "#B5");
			}

			try {
				nud.Value = -1000;
				Assert.Fail ("#C1");
			} catch (ArgumentException ex) {
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#C2");
				Assert.IsNotNull (ex.Message, "#C3");
				Assert.IsNull (ex.ParamName, "#C4");
				Assert.IsNull (ex.InnerException, "#C5");
			}

			nud.BeginInit ();

			nud.Value = 1000;
			Assert.AreEqual (1000, nud.Value, "#D1");
			nud.Value = 1000;
			Assert.AreEqual (1000, nud.Value, "#D2");
			nud.Value = -1000;
			Assert.AreEqual (-1000, nud.Value, "#D3");
			nud.EndInit ();
			try {
				nud.Value = -1000;
				Assert.Fail ("#E1");
			} catch (ArgumentException ex) {
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#E2");
				Assert.IsNotNull (ex.Message, "#E3");
				Assert.IsNull (ex.ParamName, "#E4");
				Assert.IsNull (ex.InnerException, "#E5");
			}
		}

		void NumericUpDown_TextChanged (object sender, EventArgs e)
		{
			text_changed++;
		}

		void NumericUpDown_ValueChanged (object sender, EventArgs e)
		{
			value_changed++;
		}

		public class NumericNew : NumericUpDown
		{
			public bool update_edit_text_called = false;
			public bool validate_edit_text_called = false;
			private ArrayList _callStack = new ArrayList ();

			public ArrayList CallStack {
				get { return _callStack; }
			}

			public bool user_edit {
				get {
					return UserEdit;
				}
				set {
					UserEdit = value;
				}
			}

			public bool changing_text {
				get {
					return ChangingText;
				}
				set {
					ChangingText = value;
				}
			}

			public void Reset ()
			{
				update_edit_text_called = false;
				validate_edit_text_called = false;
				_callStack.Clear ();
			}

			protected override void UpdateEditText ()
			{
				_callStack.Add ("UpdateEditText");
				update_edit_text_called = true;
				base.UpdateEditText ();
			}

			protected override void ValidateEditText ()
			{
				_callStack.Add ("ValidateEditText");
				validate_edit_text_called = true;
				base.ValidateEditText ();
			}
		}

		[Test]
		public void DomainUpDownStandardPropTest ()
		{
			DomainUpDown d1 = new DomainUpDown ();
			
			Assert.AreEqual (0, d1.Items.Count, "#24");
			Assert.AreEqual (false, d1.Sorted, "#25");
			Assert.AreEqual (false, d1.Wrap, "#26");
			Assert.AreEqual ("System.Windows.Forms.DomainUpDown, Items.Count: 0, SelectedIndex: -1", d1.ToString (), "#26a");
			
			d1.Items.Add ("item1");
			d1.Items.Add ("item2");
			d1.Items.Add ("item3");
			d1.Items.Add ("item4");
			
			Assert.AreEqual (4, d1.Items.Count, "#27");
			Assert.AreEqual (-1, d1.SelectedIndex, "#28");
			Assert.AreEqual (null, d1.SelectedItem, "#29");
		}
		
		[Test]
		[Category ("NotWorking")]
		public void DomainUpDownEnhancedPropTest ()
		{
			DomainUpDown d1 = new DomainUpDown ();
			
			d1.Items.Add ("item1");
			d1.Items.Add ("item2");
			d1.Items.Add ("item3");
			d1.Items.Add ("item4");
			
			d1.SelectedIndex = 3;
			Assert.AreEqual (3, d1.SelectedIndex, "#30");
			
			d1.Items.Remove ("item1");
			
			Assert.AreEqual (3, d1.Items.Count, "#31");
			Assert.AreEqual (2, d1.SelectedIndex, "#32");
			
			d1.Items.Remove ("item4");
			Assert.AreEqual (2, d1.Items.Count, "#33");
			Assert.AreEqual (-1, d1.SelectedIndex, "#34");
			Assert.AreEqual (null, d1.SelectedItem, "#35");
			
			// strange, ArgumentOutOfRangeException on windows
			// d1.SelectedIndex = 1;
			//Assert.AreEqual (1, d1.SelectedIndex, "#36");
			
			d1.Items.Clear ();
			Assert.AreEqual (0, d1.Items.Count, "#37");
			Assert.AreEqual (-1, d1.SelectedIndex, "#38");
			Assert.AreEqual (null, d1.SelectedItem, "#39");
			
			d1.Items.Add ("zitem1");
			d1.Items.Add ("oitem2");
			d1.Items.Add ("mitem3");
			d1.Items.Add ("aitem4");
			
			d1.SelectedIndex = 0;
			Assert.AreEqual ("zitem1", d1.SelectedItem.ToString (), "#40");
			
			d1.Sorted = true;
			Assert.AreEqual ("aitem4", d1.SelectedItem.ToString (), "#41");
			Assert.AreEqual ("aitem4", d1.Items[0].ToString (), "#42");
			
			d1.Items.Clear ();
			d1.Items.Add ("item1");
			d1.Items.Add ("item2");
			d1.Items.Add ("item3");
			d1.Items.Add ("item4");
			d1.SelectedIndex = 0;
			
			d1.UpButton ();
			Assert.AreEqual ("item1", d1.SelectedItem.ToString (), "#43");
			
			d1.DownButton ();
			Assert.AreEqual ("item2", d1.SelectedItem.ToString (), "#44");
			
			d1.SelectedIndex = 0;
			d1.Wrap = true;
			d1.UpButton ();
			Assert.AreEqual ("item4", d1.SelectedItem.ToString (), "#45");
			
			d1.DownButton ();
			Assert.AreEqual ("item1", d1.SelectedItem.ToString (), "#46");
			
			d1.Text = "item3";
			Assert.AreEqual (null, d1.SelectedItem, "#47");
		}

		private int text_changed = 0;
		private int value_changed = 0;
	}
}
