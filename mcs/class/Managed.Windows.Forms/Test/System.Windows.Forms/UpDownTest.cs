//
// 
//
// Authors:
//      Alexander Olk (alex.olk@googlemail.com)
//      Gert Driesen (drieseng@users.sourceforge.net)
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
	public class UpDownTest : TestHelper
	{
		[SetUp]
		protected override void SetUp () {
			Reset ();
			base.SetUp ();
		}

		private void Reset ()
		{
			selected_item_changed = 0;
			text_changed = 0;
			value_changed = 0;
		}

		[Test]
		public void UpDownActiveControlTest ()
		{
			NumericUpDown n1 = new NumericUpDown ();
			Assert.IsNull (n1.ActiveControl, "1");
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

		[Test] // bug #80620
		public void NumericUpDownClientRectangle_Borders ()
		{
			NumericUpDown nud = new NumericUpDown ();
			nud.CreateControl ();
			Assert.AreEqual (nud.ClientRectangle, new NumericUpDown ().ClientRectangle);
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
#if NET_2_0
			} catch (ArgumentOutOfRangeException ex) {
				Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#A2");
				Assert.IsNotNull (ex.Message, "#A3");
				Assert.IsNotNull (ex.ParamName, "#A4");
				Assert.AreEqual ("Value", ex.ParamName, "#A5");
				Assert.IsNull (ex.InnerException, "#A6");
			}
#else
			} catch (ArgumentException ex) {
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#A2");
				Assert.IsNotNull (ex.Message, "#A3");
				Assert.IsNull (ex.ParamName, "#A4");
				Assert.IsNull (ex.InnerException, "#A5");
			}
#endif

			try {
				nud.Value = 1000;
				Assert.Fail ("#B1");
#if NET_2_0
			} catch (ArgumentOutOfRangeException ex) {
				Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#B2");
				Assert.IsNotNull (ex.Message, "#B3");
				Assert.IsNotNull (ex.ParamName, "#B4");
				Assert.AreEqual ("Value", ex.ParamName, "#B5");
				Assert.IsNull (ex.InnerException, "#B6");
			}
#else
			} catch (ArgumentException ex) {
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#B2");
				Assert.IsNotNull (ex.Message, "#B3");
				Assert.IsNull (ex.ParamName, "#B4");
				Assert.IsNull (ex.InnerException, "#B5");
			}
#endif

			try {
				nud.Value = -1000;
				Assert.Fail ("#C1");
#if NET_2_0
			} catch (ArgumentOutOfRangeException ex) {
				Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#C2");
				Assert.IsNotNull (ex.Message, "#C3");
				Assert.IsNotNull (ex.ParamName, "#C4");
				Assert.AreEqual ("Value", ex.ParamName, "#C5");
				Assert.IsNull (ex.InnerException, "#C6");
			}
#else
			} catch (ArgumentException ex) {
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#C2");
				Assert.IsNotNull (ex.Message, "#C3");
				Assert.IsNull (ex.ParamName, "#C4");
				Assert.IsNull (ex.InnerException, "#C5");
			}
#endif

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
#if NET_2_0
			} catch (ArgumentOutOfRangeException ex) {
				Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#E2");
				Assert.IsNotNull (ex.Message, "#E3");
				Assert.IsNotNull (ex.ParamName, "#E4");
				Assert.AreEqual ("Value", ex.ParamName, "#E5");
				Assert.IsNull (ex.InnerException, "#E6");
			}
#else
			} catch (ArgumentException ex) {
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#E2");
				Assert.IsNotNull (ex.Message, "#E3");
				Assert.IsNull (ex.ParamName, "#E4");
				Assert.IsNull (ex.InnerException, "#E5");
			}
#endif
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

		[Test] // bug #80620
		public void DomainUpDownClientRectangle_Borders ()
		{
			DomainUpDown dud = new DomainUpDown ();
			dud.CreateControl ();
			Assert.AreEqual (dud.ClientRectangle, new DomainUpDown ().ClientRectangle);
		}

		[Test]
		[Category ("NotWorking")]
		public void DomainUpDown_SelectedIndex ()
		{
			MockDomainUpDown dud = new MockDomainUpDown ();
			dud.SelectedItemChanged += new EventHandler (DomainUpDown_SelectedItemChanged);
			dud.TextChanged += new EventHandler (DomainUpDown_TextChanged);
			Assert.AreEqual (1, dud.CallStack.Count, "#A1");
			Assert.AreEqual ("set_Text: (0)", dud.CallStack [0], "#A2");
			Assert.AreEqual (0, selected_item_changed, "#A3");
			Assert.AreEqual (0, text_changed, "#A4");
			Assert.AreEqual (-1, dud.SelectedIndex, "#A5");

			string itemA = "itemA";
			dud.Items.Add (itemA);
			Assert.AreEqual (1, dud.CallStack.Count, "#B1");
			Assert.AreEqual (0, selected_item_changed, "#B2");
			Assert.AreEqual (0, text_changed, "#B3");
			Assert.AreEqual (-1, dud.SelectedIndex, "#B4");

			dud.SelectedIndex = 0;
			Assert.AreEqual (4, dud.CallStack.Count, "#C1");
			Assert.AreEqual ("UpdateEditText", dud.CallStack [1], "#C2");
			Assert.AreEqual ("set_Text:itemA (5)", dud.CallStack [2], "#C3");
			Assert.AreEqual ("OnChanged", dud.CallStack [3], "#C4");
			Assert.AreEqual (1, selected_item_changed, "#C5");
			Assert.AreEqual (1, text_changed, "#C6");
			Assert.AreEqual (0, dud.SelectedIndex, "#C7");

			dud.SelectedIndex = 0;
			Assert.AreEqual (4, dud.CallStack.Count, "#D1");
			Assert.AreEqual (1, selected_item_changed, "#D2");
			Assert.AreEqual (1, text_changed, "#D3");
			Assert.AreEqual (0, dud.SelectedIndex, "#D4");

			dud.SelectedIndex = -1;
			Assert.AreEqual (4, dud.CallStack.Count, "#E1");
			Assert.AreEqual (1, selected_item_changed, "#E2");
			Assert.AreEqual (1, text_changed, "#E3");
			Assert.AreEqual (-1, dud.SelectedIndex, "#E4");

			dud.SelectedIndex = 0;
			Assert.AreEqual (6, dud.CallStack.Count, "#F1");
			Assert.AreEqual ("UpdateEditText", dud.CallStack [4], "#F2");
			Assert.AreEqual ("set_Text:itemA (5)", dud.CallStack [5], "#F3");
			Assert.AreEqual (1, selected_item_changed, "#F4");
			Assert.AreEqual (1, text_changed, "#F5");
			Assert.AreEqual (0, dud.SelectedIndex, "#F6");

			string itemAbis = "itemA";
			dud.Items.Add (itemAbis);
			Assert.AreEqual (6, dud.CallStack.Count, "#G1");
			Assert.AreEqual (1, selected_item_changed, "#G2");
			Assert.AreEqual (1, text_changed, "#G3");
			Assert.AreEqual (0, dud.SelectedIndex, "#G4");

			dud.SelectedIndex = 1;
			Assert.AreEqual (8, dud.CallStack.Count, "#H1");
			Assert.AreEqual ("UpdateEditText", dud.CallStack [6], "#H2");
			Assert.AreEqual ("set_Text:itemA (5)", dud.CallStack [7], "#H3");
			Assert.AreEqual (1, selected_item_changed, "#H4");
			Assert.AreEqual (1, text_changed, "#H5");
			Assert.AreEqual (1, dud.SelectedIndex, "#H6");

			string itemB = "itemB";
			dud.Items.Add (itemB);
			Assert.AreEqual (8, dud.CallStack.Count, "#I1");
			Assert.AreEqual (1, selected_item_changed, "#I2");
			Assert.AreEqual (1, text_changed, "#I3");
			Assert.AreEqual (1, dud.SelectedIndex, "#I4");

			dud.SelectedIndex = 2;
			Assert.AreEqual (11, dud.CallStack.Count, "#J1");
			Assert.AreEqual ("UpdateEditText", dud.CallStack [8], "#J2");
			Assert.AreEqual ("set_Text:itemB (5)", dud.CallStack [9], "#J3");
			Assert.AreEqual ("OnChanged", dud.CallStack [10], "#J4");
			Assert.AreEqual (2, selected_item_changed, "#J5");
			Assert.AreEqual (2, text_changed, "#J6");
			Assert.AreEqual (2, dud.SelectedIndex, "#J7");
		}

		[Test]
		[Category ("NotWorking")]
		public void DomainUpDown_Items_Add ()
		{
			MockItem itemA = new MockItem ("itemA");
			MockItem itemB = new MockItem ("itemB");
			MockItem itemC = new MockItem ("itemC");

			MockDomainUpDown dud = new MockDomainUpDown ();
			dud.SelectedItemChanged += new EventHandler (DomainUpDown_SelectedItemChanged);
			dud.TextChanged += new EventHandler (DomainUpDown_TextChanged);
			dud.Reset ();

			dud.Items.Add (itemA);
			Assert.AreEqual (0, dud.CallStack.Count, "#A1");
			Assert.AreEqual (0, selected_item_changed, "#A2");
			Assert.AreEqual (0, text_changed, "#A3");
			Assert.AreEqual (-1, dud.SelectedIndex, "#A4");
			Assert.AreEqual (string.Empty, dud.Text, "#A5");
			Assert.AreEqual (1, dud.Items.Count, "#A6");
			Assert.AreSame (itemA, dud.Items [0], "#A7");

			dud.Items.Add (itemC);
			Assert.AreEqual (0, dud.CallStack.Count, "#B1");
			Assert.AreEqual (0, selected_item_changed, "#B2");
			Assert.AreEqual (0, text_changed, "#B3");
			Assert.AreEqual (-1, dud.SelectedIndex, "#B4");
			Assert.AreEqual (string.Empty, dud.Text, "#B5");
			Assert.AreEqual (2, dud.Items.Count, "#B6");
			Assert.AreSame (itemC, dud.Items [1], "#B7");

			dud.Items.Add (itemA);
			Assert.AreEqual (0, dud.CallStack.Count, "#C1");
			Assert.AreEqual (0, selected_item_changed, "#C2");
			Assert.AreEqual (0, text_changed, "#C3");
			Assert.AreEqual (-1, dud.SelectedIndex, "#C4");
			Assert.AreEqual (string.Empty, dud.Text, "#C5");
			Assert.AreEqual (3, dud.Items.Count, "#C6");
			Assert.AreSame (itemA, dud.Items [2], "#C7");

			dud.Sorted = true;
			Assert.AreEqual (3, dud.Items.Count, "#D1");
			Assert.AreSame (itemA, dud.Items [0], "#D2");
			Assert.AreSame (itemA, dud.Items [1], "#D3");
			Assert.AreSame (itemC, dud.Items [2], "#D4");

			// adding item causes re-sort
			dud.Items.Add (itemB);
			Assert.AreEqual (0, dud.CallStack.Count, "#E1");
			Assert.AreEqual (0, selected_item_changed, "#E2");
			Assert.AreEqual (0, text_changed, "#E3");
			Assert.AreEqual (-1, dud.SelectedIndex, "#E4");
			Assert.AreEqual (string.Empty, dud.Text, "#E5");
			Assert.AreEqual (4, dud.Items.Count, "#E6");
			Assert.AreSame (itemA, dud.Items [0], "#E7");
			Assert.AreSame (itemA, dud.Items [1], "#E8");
			Assert.AreSame (itemB, dud.Items [2], "#E9");
			Assert.AreSame (itemC, dud.Items [3], "#E10");
		}

		[Test]
		[Category ("NotWorking")]
		public void DomainUpDown_Items_Indexer ()
		{
			MockItem itemA = new MockItem ("itemA");
			MockItem itemAbis = new MockItem ("itemA");
			MockItem itemB = new MockItem ("itemB");
			MockItem itemC = new MockItem ("itemC");
			MockItem itemD = new MockItem ("itemD");
			MockItem itemE = new MockItem ("itemE");

			TestHelper.RemoveWarning (itemAbis);
			
			MockDomainUpDown dud = new MockDomainUpDown ();
			dud.SelectedItemChanged += new EventHandler (DomainUpDown_SelectedItemChanged);
			dud.TextChanged += new EventHandler (DomainUpDown_TextChanged);
			dud.Items.Add (itemC);
			dud.Items.Add (itemA);
			dud.Items.Add (itemB);
			dud.Items.Add (itemA);
			dud.SelectedIndex = 1;
			dud.Reset ();
			Reset ();

			Assert.AreSame (itemC, dud.Items [0], "#A1");
			Assert.AreSame (itemA, dud.Items [1], "#A2");
			Assert.AreSame (itemB, dud.Items [2], "#A3");
			Assert.AreSame (itemA, dud.Items [3], "#A4");
			Assert.AreEqual (itemA.Text, dud.Text, "#A5");

			dud.Items [3] = itemD;
			Assert.AreEqual (0, dud.CallStack.Count, "#B1");
			Assert.AreEqual (0, selected_item_changed, "#B2");
			Assert.AreEqual (0, text_changed, "#B3");
			Assert.AreEqual (1, dud.SelectedIndex, "#B4");
			Assert.AreEqual (itemA.Text, dud.Text, "#B5");

			dud.Items [1] = itemE;
			Assert.AreEqual (3, dud.CallStack.Count, "#C1");
			Assert.AreEqual ("UpdateEditText", dud.CallStack [0], "#C2");
			Assert.AreEqual ("set_Text:itemE (5)", dud.CallStack [1], "#C3");
			Assert.AreEqual ("OnChanged", dud.CallStack [2], "#C4");
			Assert.AreEqual (1, selected_item_changed, "#C5");
			Assert.AreEqual (1, text_changed, "#C6");
			Assert.AreEqual (1, dud.SelectedIndex, "#C7");
			Assert.AreEqual (itemE.Text, dud.Text, "#C8");

			dud.Sorted = true;
			Assert.AreEqual (8, dud.CallStack.Count, "#D1");
			Assert.AreEqual ("UpdateEditText", dud.CallStack [3], "#D2");
			Assert.AreEqual ("set_Text:itemC (5)", dud.CallStack [4], "#D3");
			Assert.AreEqual ("OnChanged", dud.CallStack [5], "#D4");
			Assert.AreEqual ("UpdateEditText", dud.CallStack [6], "#D5");
			Assert.AreEqual ("set_Text:itemC (5)", dud.CallStack [7], "#D6");
			Assert.AreEqual (2, selected_item_changed, "#D7");
			Assert.AreEqual (2, text_changed, "#D8");
			Assert.AreEqual (1, dud.SelectedIndex, "#D9");
			Assert.AreEqual (itemC.Text, dud.Text, "#D10");
			Assert.AreSame (itemB, dud.Items [0], "#D11");
			Assert.AreSame (itemC, dud.Items [1], "#D12");
			Assert.AreSame (itemD, dud.Items [2], "#D13");
			Assert.AreSame (itemE, dud.Items [3], "#D14");

			dud.Items [3] = itemA;
			Assert.AreEqual (13, dud.CallStack.Count, "#E1");
			Assert.AreEqual ("UpdateEditText", dud.CallStack [8], "#E2");
			Assert.AreEqual ("set_Text:itemB (5)", dud.CallStack [9], "#E3");
			Assert.AreEqual ("OnChanged", dud.CallStack [10], "#E4");
			Assert.AreEqual ("UpdateEditText", dud.CallStack [11], "#E5");
			Assert.AreEqual ("set_Text:itemB (5)", dud.CallStack [12], "#E6");
			Assert.AreEqual (3, selected_item_changed, "#E7");
			Assert.AreEqual (3, text_changed, "#E8");
			Assert.AreEqual (1, dud.SelectedIndex, "#E9");
			Assert.AreEqual (itemB.Text, dud.Text, "#E10");
		}

		[Test]
		[Category ("NotWorking")]
		public void DomainUpDown_Items_Indexer_Null ()
		{
			MockDomainUpDown dud = new MockDomainUpDown ();
			dud.Items.Add ("itemA");
			dud.Items.Add ("itemB");
			dud.Items.Add ("itemC");
			dud.SelectedIndex = 0;

			// TODO: report as MS bug
			dud.Items [2] = null;
			dud.Items [1] = null;
			try {
				dud.Items [0] = null;
				Assert.Fail ();
			} catch (NullReferenceException ex) {
				TestHelper.RemoveWarning (ex);
			}
		}

		[Test]
		public void DomainUpDown_Items_Insert ()
		{
			// TODO
		}

		[Test]
		public void DomainUpDown_Items_Remove ()
		{
			// TODO
		}

		[Test]
		public void DomainUpDown_Items_RemoveAt ()
		{
			// TODO
		}

		[Test]
		[Category ("NotWorking")]
		public void DomainUpDown_SelectedIndex_Invalid ()
		{
			DomainUpDown dud = new DomainUpDown ();
			dud.Items.Add ("item1");

			try {
				dud.SelectedIndex = -2;
				Assert.Fail ("#A1");
#if NET_2_0
			} catch (ArgumentOutOfRangeException ex) {
				Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#A2");
				Assert.IsNotNull (ex.Message, "#A3");
				Assert.IsNotNull (ex.ParamName, "#A4");
				Assert.AreEqual ("SelectedIndex", ex.ParamName, "#A5");
				Assert.IsNull (ex.InnerException, "#A6");
			}
#else
			} catch (ArgumentException ex) {
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#A2");
				Assert.IsNotNull (ex.Message, "#A3");
				Assert.IsNull (ex.ParamName, "#A4");
				Assert.IsNull (ex.InnerException, "#A5");
			}
#endif

			try {
				dud.SelectedIndex = 1;
				Assert.Fail ("#B1");
#if NET_2_0
			} catch (ArgumentOutOfRangeException ex) {
				Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#A2");
				Assert.IsNotNull (ex.Message, "#A3");
				Assert.IsNotNull (ex.ParamName, "#A4");
				Assert.AreEqual ("SelectedIndex", ex.ParamName, "#A5");
				Assert.IsNull (ex.InnerException, "#A6");
			}
#else
			} catch (ArgumentException ex) {
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#B2");
				Assert.IsNotNull (ex.Message, "#B3");
				Assert.IsNull (ex.ParamName, "#B4");
				Assert.IsNull (ex.InnerException, "#B5");
			}
#endif
		}

		[Test]
		public void DomainUpDown_SelectedItem_Null ()
		{
			DomainUpDown dud = new DomainUpDown ();
			dud.Items.Add ("item1");
			dud.SelectedIndex = 0;
			Assert.AreEqual (0, dud.SelectedIndex, "#A1");
			Assert.IsNotNull (dud.SelectedItem, "#A2");

			dud.SelectedItem = null;
			Assert.AreEqual (-1, dud.SelectedIndex, "#B1");
			Assert.IsNull (dud.SelectedItem, "#B2");
		}

		void DomainUpDown_TextChanged (object sender, EventArgs e)
		{
			text_changed++;
		}

		void DomainUpDown_SelectedItemChanged (object sender, EventArgs e)
		{
			selected_item_changed++;
		}

		public class MockDomainUpDown : DomainUpDown
		{
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
				_callStack.Clear ();
			}

			public override string Text {
				get {
					return base.Text;
				}
				set {
					if (value == null)
						_callStack.Add ("set_Text:null");
					else
						_callStack.Add ("set_Text:" + value + " (" + value.Length + ")");
					base.Text = value;
				}
			}

			protected override void OnChanged (object source, EventArgs e)
			{
				_callStack.Add ("OnChanged");
				base.OnChanged (source, e);
			}

			protected override void UpdateEditText ()
			{
				_callStack.Add ("UpdateEditText");
				base.UpdateEditText ();
			}

			protected override void ValidateEditText ()
			{
				_callStack.Add ("ValidateEditText");
				base.ValidateEditText ();
			}
		}

		private int selected_item_changed = 0;
		private int text_changed = 0;
		private int value_changed = 0;

		public class MockItem
		{
			public MockItem (string text)
			{
				_text = text;
			}

			public string Text {
				get { return _text; }
			}

			public override string ToString ()
			{
				return _text;
			}

			private readonly string _text;
		}
		
#if NET_2_0
		[Test]
		public void Defaults ()
		{
			UpDownBase udb = new MockUpDown ();
			
			Assert.AreEqual (new Size (0, 0), udb.MaximumSize, "A1");
			Assert.AreEqual (new Size (0, 0), udb.MinimumSize, "A2");
			
			udb.MaximumSize = new Size (100, 100);
			udb.MinimumSize = new Size (100, 100);

			Assert.AreEqual (new Size (100, 0), udb.MaximumSize, "A3");
			Assert.AreEqual (new Size (100, 0), udb.MinimumSize, "A4");
		}
		
		private class MockUpDown : UpDownBase
		{
			public MockUpDown () : base ()
			{
			}

			public override void DownButton ()
			{
				throw new Exception ("The method or operation is not implemented.");
			}

			public override void UpButton ()
			{
				throw new Exception ("The method or operation is not implemented.");
			}

			protected override void UpdateEditText ()
			{
				throw new Exception ("The method or operation is not implemented.");
			}
		}
#endif	
	}
}
