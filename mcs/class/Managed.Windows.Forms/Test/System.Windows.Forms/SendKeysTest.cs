/**
 * SendKeysTest.cs: Test cases for SendKeys
 * 
 * These tests can only run in ms.net one at a time.
 * Since ms.net apparently hooks the keyboard to 
 * implement this, running two tests in a row
 * makes the second test run before the hook
 * of the first test is released, effectively
 * hanging the keyboard. CTRL-ALT-DEL releases
 * the keyboard, but the test still hangs.
 * Running each test separately works.
 * 
 * Author:
 *		Andreia Gaita (avidigal@novell.com)
 * 
 * (C) 2005 Novell, Inc. (http://www.novell.com)
 * 
*/ 

using System;
using System.Windows.Forms;
using System.Drawing;
using System.Reflection;
using System.Text;
using System.Collections;
using NUnit.Framework;

namespace MonoTests.System.Windows.Forms
{
	[TestFixture]	
	[Category("NotDotNet")]
	[Category("NotWithXvfb")]
	[Category("Interactive")]
	public class SendKeysTest  : TestHelper {

		static Queue keys = new Queue();

		internal struct Keys {
			public string key;
			public bool up;
			public bool shift;
			public bool ctrl;
			public bool alt;

			public Keys(string key, bool up, bool shift, bool ctrl, bool alt) {
				this.key = key;
				this.up = up;
				this.shift = shift;
				this.ctrl = ctrl;
				this.alt = alt;
			}
		}

		internal class Custom: TextBox {

			protected override void OnKeyDown(KeyEventArgs e) {
				keys.Enqueue(new Keys(e.KeyData.ToString(), false, e.Shift, e.Control, e.Alt));
				base.OnKeyDown (e);
			}

			protected override void OnKeyUp(KeyEventArgs e) {
				keys.Enqueue(new Keys(e.KeyData.ToString(), true, e.Shift, e.Control, e.Alt));
				base.OnKeyUp (e);
			}
		}

		
		
		public SendKeysTest() {
		}

		Form f;
		Timer t;
		Custom c;

		[Test]
		public void SendKeysTest1() {
			f = new Form();
			f.Activated +=new EventHandler(SendKeysTest1_activated);
			c = new Custom();
			f.Controls.Add(c);
			Application.Run(f);
			c.Dispose();
		}

		private void SendKeysTest1_activated(object sender, EventArgs e) {
			SendKeys.SendWait("a");

			t = new  Timer();
			t.Interval = 1;
			t.Tick +=new EventHandler(SendKeysTest1_tick);
			t.Start();
			
		}

		private void SendKeysTest1_tick(object sender, EventArgs e) {
			if (f.InvokeRequired) {
				f.Invoke (new EventHandler (SendKeysTest1_tick), new object [] { sender, e });
				return;
			}
			t.Stop();
			Assert.AreEqual(2, keys.Count, "#A1");
			Keys k = (Keys)keys.Dequeue();
			Assert.IsFalse(k.up, "#A2");
			Assert.IsFalse(k.shift, "#A3");
			Assert.IsFalse(k.ctrl, "#A4");
			Assert.IsFalse(k.alt, "#A5");
			Assert.AreEqual("A", k.key, "#A6");

			k = (Keys)keys.Dequeue();
			Assert.IsTrue(k.up, "#A2");
			Assert.IsFalse(k.shift, "#A3");
			Assert.IsFalse(k.ctrl, "#A4");
			Assert.IsFalse(k.alt, "#A5");
			Assert.AreEqual("A", k.key, "#A6");
			
			t.Dispose();
			f.Close ();
		}

		[SetUp]
		protected override void SetUp () {
			keys.Clear();
			base.SetUp ();
		}


		[Test]
		public void SendKeysTest2() {
			f = new Form();
			f.Activated +=new EventHandler(SendKeysTest2_activated);
			c = new Custom();
			f.Controls.Add(c);
			Application.Run(f);
			c.Dispose();
		}


		private void SendKeysTest2_activated(object sender, EventArgs e) {
			SendKeys.SendWait("+(abc){BACKSPACE 2}");

			t = new Timer();
			t.Interval = 1;
			t.Tick +=new EventHandler(SendKeysTest2_tick);
			t.Start();
			
		}

		private void SendKeysTest2_tick(object sender, EventArgs e) {
			t.Stop();
			if (f.InvokeRequired) {
				f.Invoke (new EventHandler (SendKeysTest2_tick), new object [] {sender, e});
				return;
			}
			Assert.AreEqual(12, keys.Count, "#A1");

			Keys k = (Keys)keys.Dequeue();
			Assert.IsFalse(k.up, "#A2");
			Assert.IsTrue(k.shift, "#A3");
			Assert.IsFalse(k.ctrl, "#A4");
			Assert.IsFalse(k.alt, "#A5");

			k = (Keys)keys.Dequeue();
			Assert.IsFalse(k.up, "#A7");
			Assert.IsTrue(k.shift, "#A8");
			Assert.IsFalse(k.ctrl, "#A9");
			Assert.IsFalse(k.alt, "#A10");
			Assert.AreEqual("A, Shift", k.key, "#A11");
			
			k = (Keys)keys.Dequeue();
			Assert.IsTrue(k.up, "#A12");
			Assert.IsTrue(k.shift, "#A13");
			Assert.IsFalse(k.ctrl, "#A14");
			Assert.IsFalse(k.alt, "#A15");
			Assert.AreEqual("A, Shift", k.key, "#A16");

			k = (Keys)keys.Dequeue();
			Assert.IsFalse(k.up, "#A17");
			Assert.IsTrue(k.shift, "#A18");
			Assert.IsFalse(k.ctrl, "#A19");
			Assert.IsFalse(k.alt, "#A20");
			Assert.AreEqual("B, Shift", k.key, "#A21");

			k = (Keys)keys.Dequeue();
			Assert.IsTrue(k.up, "#A22");
			Assert.IsTrue(k.shift, "#A23");
			Assert.IsFalse(k.ctrl, "#A24");
			Assert.IsFalse(k.alt, "#A25");
			Assert.AreEqual("B, Shift", k.key, "#A26");

			k = (Keys)keys.Dequeue();
			Assert.IsFalse(k.up, "#A27");
			Assert.IsTrue(k.shift, "#A28");
			Assert.IsFalse(k.ctrl, "#A28");
			Assert.IsFalse(k.alt, "#A29");
			Assert.AreEqual("C, Shift", k.key, "#A30");

			k = (Keys)keys.Dequeue();
			Assert.IsTrue(k.up, "#A31");
			Assert.IsTrue(k.shift, "#A32");
			Assert.IsFalse(k.ctrl, "#A33");
			Assert.IsFalse(k.alt, "#A34");
			Assert.AreEqual("C, Shift", k.key, "#A35");

			k = (Keys)keys.Dequeue();
			Assert.IsTrue(k.up, "#A36");
			Assert.IsFalse(k.shift, "#A37");
			Assert.IsFalse(k.ctrl, "#A38");
			Assert.IsFalse(k.alt, "#A39");
			Assert.AreEqual("ShiftKey", k.key, "#A40");

			k = (Keys)keys.Dequeue();
			Assert.IsFalse(k.up, "#b1");
			Assert.IsFalse(k.shift, "#b2");
			Assert.IsFalse(k.ctrl, "#b3");
			Assert.IsFalse(k.alt, "#b4");
			Assert.AreEqual("Back", k.key, "#b5");

			k = (Keys)keys.Dequeue();
			Assert.IsTrue(k.up, "#b6");
			Assert.IsFalse(k.shift, "#b7");
			Assert.IsFalse(k.ctrl, "#b8");
			Assert.IsFalse(k.alt, "#b9");
			Assert.AreEqual("Back", k.key, "#b10");

			k = (Keys)keys.Dequeue();
			Assert.IsFalse(k.up, "#c1");
			Assert.IsFalse(k.shift, "#c2");
			Assert.IsFalse(k.ctrl, "#c3");
			Assert.IsFalse(k.alt, "#c4");
			Assert.AreEqual("Back", k.key, "#c5");

			k = (Keys)keys.Dequeue();
			Assert.IsTrue(k.up, "#c6");
			Assert.IsFalse(k.shift, "#c7");
			Assert.IsFalse(k.ctrl, "#c8");
			Assert.IsFalse(k.alt, "#c9");
			Assert.AreEqual("Back", k.key, "#c10");

			Assert.AreEqual(0, keys.Count, "#d1");

			Assert.AreEqual("A", c.Text, "#e1");

			t.Dispose();
			f.Close ();
		}

	}
}
