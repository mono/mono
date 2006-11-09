//
// 
//
// Authors:
//      Alexander Olk (alex.olk@googlemail.com)
//

using System;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;

using NUnit.Framework;

namespace MonoTests.System.Windows.Forms
{
	[TestFixture]
	public class UpDownTest
	{
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
		[Category("NotWorking")]
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
		
		public class NumericNew : NumericUpDown
		{
			public bool update_edit_text_called = false;
			public bool validate_edit_text_called = false;
			
			public bool user_edit {
				get {
					return UserEdit;
				}
				set {
					UserEdit = value;
				}
			}
			
			public void Reset ()
			{
				update_edit_text_called = false;
				validate_edit_text_called = false;
			}
			
			protected override void UpdateEditText ()
			{
				update_edit_text_called = true;
				base.UpdateEditText ();
			}
			
			protected override void ValidateEditText ()
			{
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
	}
}
