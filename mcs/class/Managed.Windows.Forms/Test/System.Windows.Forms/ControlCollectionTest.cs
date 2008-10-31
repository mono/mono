#if NET_2_0
using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using System.Drawing;
using System.Windows.Forms;

namespace MonoTests.System.Windows.Forms
{
	[TestFixture]
	public class ControlCollectionTest : TestHelper
	{
		[Test]
		public void ControlCollectionTests ()
		{
			Control c = new Control ();
			c.Name = "A";
			Control c2 = new Control ();
			c2.Name = "B";
			Control c3 = new Control ();
			c3.Name = "a";
			Control c4 = new Control ();
			c4.Name = "B";
			Control c5 = new Control ();
			c5.Name = "a";
			
			c.Controls.Add (c2);
			c.Controls.Add (c3);
			c2.Controls.Add (c4);
			c2.Controls.Add (c5);
			
			// this[key]
			Assert.AreSame (c2, c.Controls["B"], "A1");
			Assert.AreSame (c2, c.Controls["b"], "A2");
			
			// Owner
			Assert.AreSame (c, c.Controls.Owner, "A3");
			
			// ContainsKey
			Assert.AreEqual (true, c.Controls.ContainsKey ("A"), "A4");
			Assert.AreEqual (true, c.Controls.ContainsKey ("a"), "A5");
			Assert.AreEqual (false, c.Controls.ContainsKey ("C"), "A6"); 
		
			// Find
			Assert.AreEqual (1, c.Controls.Find ("A", false).Length, "A7");
			Assert.AreEqual (1, c.Controls.Find ("a", false).Length, "A8");
			Assert.AreEqual (0, c.Controls.Find ("C", false).Length, "A9");

			Assert.AreEqual (2, c.Controls.Find ("A", true).Length, "A10");
			Assert.AreEqual (2, c.Controls.Find ("a", true).Length, "A11");
			Assert.AreEqual (0, c.Controls.Find ("C", true).Length, "A12");
			Assert.AreEqual (1, c2.Controls.Find ("b", true).Length, "A13");
			
			// IndexOfKey
			Assert.AreEqual (1, c.Controls.IndexOfKey ("A"), "A14");
			Assert.AreEqual (1, c.Controls.IndexOfKey ("a"), "A15");
			Assert.AreEqual (-1, c.Controls.IndexOfKey ("C"), "A16");
	
			// RemoveByKey
			c.Controls.RemoveByKey ("A");
			Assert.AreEqual (1, c.Controls.Count, "A17");
			
			c.Controls.RemoveByKey ("b");
			Assert.AreEqual (0, c.Controls.Count, "A18");

			c.Controls.RemoveByKey (null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ControlCollectionFindANE ()
		{
			Control c = new Control ();
			c.Controls.Find ("", false);
		}
	}
}
#endif