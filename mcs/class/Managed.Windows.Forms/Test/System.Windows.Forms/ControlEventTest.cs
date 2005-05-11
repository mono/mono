
using System;
using NUnit.Framework;
using System.Windows.Forms;
using System.Drawing;
using System.Threading;

namespace Events
{
    [TestFixture]
    public class EventClass
    {
        static bool eventhandled = false;
        public static void Event_Handler1(object sender, EventArgs e)
        {
            eventhandled = true;
        }

        [Test]
        public void BackColorChangedTest()
        {
            Control c = new Control();
            // Test BackColorChanged Event
            c.BackColorChanged += new System.EventHandler(Event_Handler1);
            c.BackColor = Color.Black;
            Assert.AreEqual(true, eventhandled, "#A1");

        }

        [Test]
        public void BgrndImageChangedTest()
        {
            Control c = new Control();
            // Test BackgroundImageChanged Event
            c.BackgroundImageChanged += new System.EventHandler(Event_Handler1);
            string abc = "M.gif";
            eventhandled = false;
            c.BackgroundImage = Image.FromFile(abc);
            Assert.AreEqual(true, eventhandled, "#A2");
        }

        [Test]
        public void BindingContextChangedTest()
        {
            Control c = new Control();
            // Test BindingContextChanged Event
            c.BindingContextChanged += new System.EventHandler(Event_Handler1);
            BindingContext bcG1 = new BindingContext();
            eventhandled = false;
            c.BindingContext = bcG1;
            Assert.AreEqual(true, eventhandled, "#A3");

        }

        [Test]
        public void CausesValidationChangedTest()
        {
            Control c = new Control();
            // Test CausesValidationChanged Event
            c.CausesValidationChanged += new System.EventHandler(Event_Handler1);
            eventhandled = false;
            c.CausesValidation = false;
            Assert.AreEqual(true, eventhandled, "#A4");

        }

        [Test]
        public void CursorChangedTest()
        {
            Control c = new Control();
            // Test CursorChanged Event
            c.CursorChanged += new System.EventHandler(Event_Handler1);
            eventhandled = false;
            c.Cursor = Cursors.Hand;
            Assert.AreEqual(true, eventhandled, "#A6");
        }

        [Test]
        public void DisposedTest()
        {
            Control c = new Control();
            // Test Disposed Event
            c.Disposed += new System.EventHandler(Event_Handler1);
            eventhandled = false;
            c.Dispose();
            Assert.AreEqual(true, eventhandled, "#A7");
        }

        [Test]
        public void DockChangedTest()
        {
            Control c = new Control();
            // Test DockChanged Event
            c.DockChanged += new System.EventHandler(Event_Handler1);
            eventhandled = false;
            c.Dock = DockStyle.Bottom;
            Assert.AreEqual(true, eventhandled, "#A8");
        }

        [Test]
        public void EnabledChangedTest()
        {
            Control c = new Control();
            // Test EnabledChanged Event
            c.EnabledChanged += new System.EventHandler(Event_Handler1);
            eventhandled = false;
            c.Enabled = false;
            Assert.AreEqual(true, eventhandled, "#A9");
        }

        [Test]
        public void FontChangedTest()
        {
            Control c = new Control();
            // Test FontChanged Event
            c.FontChanged += new System.EventHandler(Event_Handler1);
            eventhandled = false;
            c.Font = new Font(c.Font, FontStyle.Bold);
            Assert.AreEqual(true, eventhandled, "#A11");
        }

        [Test]
        public void ForeColorChangedTest()
        {
            Control c = new Control();
            // Test ForeColorChanged Event
            c.ForeColorChanged += new System.EventHandler(Event_Handler1);
            eventhandled = false;
            c.ForeColor = Color.Red;
            Assert.AreEqual(true, eventhandled, "#A12");
        }
       
        [Test]
        public void HandleCreatedTest()
        {
            Control c = new Control();
            // Test HandleCreated Event
            c.HandleCreated += new System.EventHandler(Event_Handler1);
            eventhandled = false;
            c.Handle.GetType();
            Assert.AreEqual(true, eventhandled, "#A15");
        }

        [Test]
        public void ImeModeChangedTest()
        {
            Control c = new Control();
            // Test ImeModeChanged Event
            c.ImeModeChanged += new System.EventHandler(Event_Handler1);
            eventhandled = false;
            c.ImeMode = ImeMode.Off;
            Assert.AreEqual(true, eventhandled, "#A19");
        }

        [Test]
        public void LocationChangedTest()
        {
            Control c = new Control();
            // Test LocationChanged Event
            c.LocationChanged += new System.EventHandler(Event_Handler1);
            eventhandled = false;
            c.Left = 20;
            Assert.AreEqual(true, eventhandled, "#A20");
        }

        [Test]
        public void ResizeTest()
        {
            Control c = new Control();
            // Test Resize Event
            c.Resize += new System.EventHandler(Event_Handler1);
            eventhandled = false;
            c.Height = 20;
            Assert.AreEqual(true, eventhandled, "#A22");
        }

        [Test]
        public void RightToLeftChangedTest()
        {
            Control c = new Control();
            // Test RightToLeftChanged Event
            c.RightToLeftChanged += new System.EventHandler(Event_Handler1);
            eventhandled = false;
            c.RightToLeft = RightToLeft.Yes;
            Assert.AreEqual(true, eventhandled, "#A23");
        }

        [Test]
        public void SizeChangedTest()
        {
            Control c = new Control();
            // Test SizeChanged Event
            c.SizeChanged += new System.EventHandler(Event_Handler1);
            eventhandled = false;
            c.Height = 80;
            Assert.AreEqual(true, eventhandled, "#A24");
        }

        [Test]
        public void TabIndexChangedTest()
        {
            Control c = new Control();
            // Test TabIndexChanged Event
            c.TabIndexChanged += new System.EventHandler(Event_Handler1);
            eventhandled = false;
            c.TabIndex = 1;
            Assert.AreEqual(true, eventhandled, "#A27");
        }

        [Test]
        public void TabStopChangedTest()
        {
            Control c = new Control();
            // Test TabStopChanged Event
            c.TabStopChanged += new System.EventHandler(Event_Handler1);
            eventhandled = false;
            c.TabStop = false;
            Assert.AreEqual(true, eventhandled, "#A28");
        }

        [Test]
        public void TextChangedTest()
        {
            Control c = new Control();
            // Test TextChanged Event
            c.TextChanged += new System.EventHandler(Event_Handler1);
            eventhandled = false;
            c.Text = "some Text";
            Assert.AreEqual(true, eventhandled, "#A29");
        }

        [Test]
        public void VisibleChangedTest()
        {
            Control c = new Control();
            // Test VisibleChanged Event
            c.VisibleChanged += new System.EventHandler(Event_Handler1);
            eventhandled = false;
            c.Visible = false;
            Assert.AreEqual(true, eventhandled, "#A30");
        }
    }
    
    
    [TestFixture]
    public class LayoutEventClass
    {
        static bool eventhandled = false;
        public static void LayoutEvent(object sender, LayoutEventArgs e)
        {
            eventhandled = true;
        }

        [Test]
        public void LayoutTest()
        {
            Control c = new Control();
            c.Layout += new System.Windows.Forms.LayoutEventHandler(LayoutEvent);
            eventhandled = false;
            c.Visible = true;
            c.Height = 100;
            Assert.AreEqual(true, eventhandled, "#D1");

        }
    }

    [TestFixture]
    public class ControlAddRemoveEventClass
    {
        static bool eventhandled = false;
        public static void ControlEvent(object sender, ControlEventArgs e)
        {
            eventhandled = true;
        }

        [Test]
        public void ControlAddedTest()
        {
            Control c = new Control();
            c.ControlAdded += new System.Windows.Forms.ControlEventHandler(ControlEvent);
            TextBox TB = new TextBox();
            eventhandled = false;
            c.Controls.Add(TB);
            Assert.AreEqual(true, eventhandled, "#F1");
        }

        [Test]
        public void ControlRemovedTest()
        {
            Control c = new Control();
            c.ControlRemoved += new System.Windows.Forms.ControlEventHandler(ControlEvent);
            TextBox TB = new TextBox();
            c.Controls.Add(TB);
            eventhandled = false;
            c.Controls.Remove(TB);
            Assert.AreEqual(true, eventhandled, "#F2");
        }

    }

}
