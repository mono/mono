
using System;
using System.ComponentModel;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;
using System.Collections;

using NUnit.Framework;

using CategoryAttribute = NUnit.Framework.CategoryAttribute;

namespace MonoTests.System.Windows.Forms
{
	[TestFixture]
	public class NumericUpDownTest
	{
		[Test]
		public void DefaultValues ()
		{
			NumericUpDown n = new NumericUpDown ();
#if NET_2_0
			Assert.IsFalse (n.Accelerations.IsReadOnly, "#A1");
#endif
		}

#if NET_2_0
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
#endif
	}
}
