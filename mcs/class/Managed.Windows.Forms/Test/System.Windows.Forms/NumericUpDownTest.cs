using System;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Reflection;
using System.Windows.Forms;
using System.Collections;
using Threading = System.Threading;

using NUnit.Framework;

using CategoryAttribute = NUnit.Framework.CategoryAttribute;

namespace MonoTests.System.Windows.Forms
{
	[TestFixture]
	public class NumericUpDownTest : TestHelper
	{
		[Test]
		public void DefaultValues ()
		{
			NumericUpDown n = new NumericUpDown ();
			Assert.IsFalse (n.Accelerations.IsReadOnly, "#A1");
		}

		[Test]
		public void SortedAccelerationsTest ()
		{
			NumericUpDown numericUpDown1 = new NumericUpDown ();
			numericUpDown1.Maximum = 40000;
			numericUpDown1.Minimum = -40000;

			numericUpDown1.Accelerations.Add (new NumericUpDownAcceleration (9, 100));
			numericUpDown1.Accelerations.Add (new NumericUpDownAcceleration (2, 1000));
			numericUpDown1.Accelerations.Add (new NumericUpDownAcceleration (10, 2000));
			numericUpDown1.Accelerations.Add (new NumericUpDownAcceleration (8, 5000));

			Assert.AreEqual (2, numericUpDown1.Accelerations[0].Seconds, "#A1");
			Assert.AreEqual (8, numericUpDown1.Accelerations[1].Seconds, "#A2");
			Assert.AreEqual (9, numericUpDown1.Accelerations[2].Seconds, "#A3");
			Assert.AreEqual (10, numericUpDown1.Accelerations[3].Seconds, "#A4");
		}

		[Test]
		public void Minimum ()
		{
			Form f = new Form ();
			NumericUpDown nud = new NumericUpDown ();
			nud.Value = 0;
			nud.Minimum = 2;
			nud.Maximum = 4;
			f.Controls.Add (nud);
			f.Show ();

			Assert.AreEqual (2, nud.Value, "#A1");
			nud.Minimum = 3;
			Assert.AreEqual (3, nud.Value, "#A2");
			f.Dispose ();
		}

		[Test]
		public void Maximum ()
		{
			Form f = new Form ();
			NumericUpDown nud = new NumericUpDown ();
			nud.BeginInit ();
			nud.Value = 1000;
			nud.Minimum = 2;
			nud.Maximum = 4;
			nud.EndInit ();
			f.Controls.Add (nud);
			f.Show ();

			Assert.AreEqual (4, nud.Value, "#A1");
			nud.Maximum = 3;
			Assert.AreEqual (3, nud.Value, "#A2");
			f.Dispose ();
		}

		[Test]
		public void Hexadecimal ()
		{
			Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo ("en-US");
			Form f = new Form ();
			NumericUpDown nud = new NumericUpDown ();
			nud.Maximum = 100000;
			f.Controls.Add (nud);
			f.Show ();

			nud.Value = 56789;
			nud.Hexadecimal = true;
			Assert.AreEqual ("DDD5", nud.Text, "#A1");
			Assert.AreEqual (56789, nud.Value, "#A2");
			f.Dispose ();
		}

		[Test]
		public void ThousandsSeparator ()
		{
			Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo ("en-US");
			Form f = new Form ();
			NumericUpDown nud = new NumericUpDown ();
			nud.Maximum = 100000;
			f.Controls.Add (nud);
			f.Show ();

			nud.Value = 12345;
			nud.ThousandsSeparator = true;
			Assert.AreEqual ("12,345", nud.Text, "#A1");
			Assert.AreEqual (12345, nud.Value, "#A2");
			f.Dispose ();
		}

		[Test]
		public void Height ()
		{
			NumericUpDown nud = new NumericUpDown ();
			Assert.AreEqual (20, nud.PreferredHeight, "#1");
			nud.Height = 9999;
			Assert.AreEqual (nud.PreferredHeight, nud.Height, "#2");
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void SetValueThrowsException ()
		{
			NumericUpDown nud = new NumericUpDown ();
			nud.Maximum = 3;
			nud.Value = 4;
			nud.Dispose ();
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void InitTest ()
		{
			NumericUpDown nud = new NumericUpDown ();
			nud.BeginInit ();
			nud.Maximum = 3;
			nud.BeginInit ();
			nud.EndInit ();
			nud.Value = 4;
			nud.Dispose ();
		}
	}
}
