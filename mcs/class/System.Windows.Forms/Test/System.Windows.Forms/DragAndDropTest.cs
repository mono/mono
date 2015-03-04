//
// DragAndDropTest.cs: tests for general drag and drop operations.
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
// Author:
//   Carlos Alberto Cortez <ccortes@novell.com>
//
// (C) 2008 Novell, Inc. (http://www.novell.com)
//
// NOTE: Since this is an interactive set of tests, I didn't include it as part
// of the MWF test suite build. You will need to build it by yourself and then
// use nunit-console, since the nunit GUI has some problems with DND (basically a COM
// issue).
//

using System;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;

using NUnit.Framework;

namespace MonoTests.System.Windows.Forms
{
	[TestFixture]
	public class DragAndDropTest
	{
		[Test]
		public void DropCancelledByEsc ()
		{
			DNDForm form = new DNDForm ();
			form.Text = MethodBase.GetCurrentMethod ().Name;
			form.InstructionsText =
				"Instructions:" + Environment.NewLine + Environment.NewLine +
				"1. Click with left button on the control on the left, holding it." + Environment.NewLine +
				"2. Move mouse pointer to the control in the right. " + Environment.NewLine +
				"3. While still pressing the mouse button, press ESC." + Environment.NewLine +
				"4. Close the form.";
			form.DragControl.DragData = "hello";
			form.DragControl.AllowedEffects = DragDropEffects.All;
			form.DropControl.DropEffect = DragDropEffects.Copy;

			Application.Run (form);

			Assert.AreEqual (DragDropEffects.None, form.DragControl.PerformedEffect, "A1");
			Assert.AreEqual (1, form.DropControl.EnterFiredCount, "A2");
			Assert.AreEqual (1, form.DropControl.LeaveFiredCount, "A3");
			Assert.AreEqual (0, form.DropControl.DropFiredCount, "A4");
		}

		[Test]
		public void DropCancelledByQuery ()
		{
			DNDForm form = new DNDForm ();
			form.Text = MethodBase.GetCurrentMethod ().Name;
			form.InstructionsText =
				"Instructions:" + Environment.NewLine + Environment.NewLine +
				"1. Click with left button on the control on the left, holding it." + Environment.NewLine +
				"2. Move mouse pointer to the control in the right. " + Environment.NewLine +
				"3. The drop should be cancelled after some seconds." + Environment.NewLine +
				"4. Close the form.";
			form.DragControl.DragData = "hello";
			form.DragControl.AllowedEffects = DragDropEffects.All;
			form.DropControl.DropEffect = DragDropEffects.Copy;

			query_counter = 0;
			form.DragControl.QueryContinueDrag += new QueryContinueDragEventHandler (DragControl_QueryContinueDrag);

			Application.Run (form);

			Assert.AreEqual (DragDropEffects.None, form.DragControl.PerformedEffect, "A1");
			Assert.AreEqual (1, form.DropControl.EnterFiredCount, "A2");
			Assert.AreEqual (1, form.DropControl.LeaveFiredCount, "A3");
			Assert.AreEqual (0, form.DropControl.DropFiredCount, "A4");
		}

		int query_counter = 0;

		void DragControl_QueryContinueDrag (object sender, QueryContinueDragEventArgs e)
		{
			DragDropControl c = (DragDropControl)sender;
			DNDForm f = (DNDForm)c.Parent;
			if (f.DropControl.EnterFiredCount == 0)
				return;

			// Cancel dnd operation AFTER we have reached the drop control
			if (query_counter++ >= 32)
				e.Action = DragAction.Cancel;
		}

		[Test]
		public void EventsAfterDrop ()
		{
			DNDForm form = new DNDForm ();
			form.Text = MethodBase.GetCurrentMethod ().Name;
			form.InstructionsText =
				"Instructions:" + Environment.NewLine + Environment.NewLine +
				"1. Click with left button on the control on the left, holding it." + Environment.NewLine +
				"2. Move mouse pointer to the control in the right. " + Environment.NewLine +
				"3. Move the pointer around and then release the mouse button." + Environment.NewLine +
				"4. Close the form.";
			form.DragControl.DragData = "hello";
			form.DragControl.AllowedEffects = DragDropEffects.All;
			form.DropControl.DropEffect = DragDropEffects.Copy;

			// These should clear the event counters AND
			// thus the event counters should remain at 0 after drop occurred
			// AND we should get any MouseUp event.
			mouse_up_counter = 0;
			form.DropControl.DragDrop += new DragEventHandler (DropControl_DragDrop);
			form.DropControl.MouseUp += new MouseEventHandler (DropControl_MouseUp);

			Application.Run (form);

			Assert.AreEqual (DragDropEffects.Copy, form.DragControl.PerformedEffect, "A1");
			Assert.AreEqual (0, form.DropControl.EnterFiredCount, "A2");
			Assert.AreEqual (0, form.DropControl.LeaveFiredCount, "A3");
			Assert.AreEqual (0, form.DropControl.DropFiredCount, "A4");
			Assert.AreEqual (0, form.DropControl.DragOverFiredCount, "A5");
			Assert.AreEqual (0, mouse_up_counter, "A6");
		}

		void DropControl_DragDrop (object sender, DragEventArgs e)
		{
			DragDropControl c = (DragDropControl)sender;
			c.ResetEventCounters ();
		}

		int mouse_up_counter;

		void DropControl_MouseUp (object sender, MouseEventArgs e)
		{
			mouse_up_counter++;
		}

		[Test]
		public void SimpleDragDrop ()
		{
			DNDForm form = new DNDForm ();
			form.Text = MethodBase.GetCurrentMethod ().Name;
			form.InstructionsText =
				"Instructions:" + Environment.NewLine + Environment.NewLine +
				"1. Click with left button on the control on the left, holding it." + Environment.NewLine +
				"2. Move mouse pointer to the control in the right. " + Environment.NewLine +
				"3. Drop on the right control." + Environment.NewLine +
				"4. Close the form.";
			form.DragControl.DragData = "SimpleDragDropMessage";
			form.DragControl.AllowedEffects = DragDropEffects.Copy;
			form.DropControl.DropEffect = DragDropEffects.Copy;

			Application.Run (form);

			Assert.AreEqual (DragDropEffects.Copy, form.DragControl.PerformedEffect, "A1");
			Assert.IsTrue (form.DropControl.Data != null, "A2");
			Assert.AreEqual (true, form.DropControl.Data.GetDataPresent (typeof (string)), "A3");
			Assert.AreEqual ("SimpleDragDropMessage", form.DropControl.Data.GetData (typeof (string)), "A4");
		}

		[Test]
		public void SimpleLeave ()
		{
			DNDForm form = new DNDForm ();
			form.Text = MethodBase.GetCurrentMethod ().Name;
			form.InstructionsText =
				"Instructions:" + Environment.NewLine + Environment.NewLine +
				"1. Click with left button on the control on the left, holding it." + Environment.NewLine +
				"2. Move mouse pointer to the control in the right. " + Environment.NewLine +
				"3. Move the mouse pointer outside the drop control." + Environment.NewLine +
				"5. Release mouse button." + Environment.NewLine +
				"6. Close the form.";
			form.DragControl.DragData = "hello";
			form.DragControl.AllowedEffects = DragDropEffects.All;
			form.DropControl.DropEffect = DragDropEffects.Copy;

			Application.Run (form);

			Assert.AreEqual (DragDropEffects.None, form.DragControl.PerformedEffect, "A1");
			Assert.AreEqual (1, form.DropControl.EnterFiredCount, "A2");
			Assert.AreEqual (1, form.DropControl.LeaveFiredCount, "A3");
			Assert.AreEqual (0, form.DropControl.DropFiredCount, "A4");
		}

		[Test]
		public void NotAllowedDropEffect ()
		{
			DNDForm form = new DNDForm ();
			form.Text = MethodBase.GetCurrentMethod ().Name;
			form.InstructionsText =
				"Instructions:" + Environment.NewLine + Environment.NewLine +
				"1. Click with left button on the control on the left, holding it." + Environment.NewLine +
				"2. Move mouse pointer to the control in the right. " + Environment.NewLine +
				"3. Drop on the right control." + Environment.NewLine +
				"4. Close the form.";
			form.DragControl.DragData = "SimpleDragDropMessage";
			form.DragControl.AllowedEffects = DragDropEffects.Copy;
			form.DropControl.DropEffect = DragDropEffects.Move;

			Application.Run (form);

			Assert.AreEqual (DragDropEffects.None, form.DragControl.PerformedEffect, "A1");
			Assert.AreEqual (1, form.DropControl.EnterFiredCount, "A2");
			Assert.AreEqual (0, form.DropControl.DropFiredCount, "A3");
			Assert.AreEqual (1, form.DropControl.LeaveFiredCount, "A4");
		}

		// This test should probably include a 'log' check
		[Test]
		public void SequentialOperations ()
		{
			DNDForm form = new DNDForm ();
			form.Text = MethodBase.GetCurrentMethod ().Name;
			form.InstructionsText =
				"Instructions:" + Environment.NewLine + Environment.NewLine +
				"1. Click with left button on the control on the left, holding it." + Environment.NewLine +
				"2. Drag on the control on the right. " + Environment.NewLine + Environment.NewLine +
				"3. Click with left button on the control on the left again, holding it." + Environment.NewLine +
				"4. Move the mouse pointer to the control in the right." + Environment.NewLine +
				"5. Move the mouse pointer outside the drop control." + Environment.NewLine +
				"6. Release mouse button." + Environment.NewLine + Environment.NewLine +
				"7. Click with left button on the control on the left again, holding it." + Environment.NewLine +
				"8. Drag on the control on the right. " + Environment.NewLine + Environment.NewLine +
				"9. Close the form.";
			form.DragControl.DragData = "SimpleDragDropMessage";
			form.DragControl.AllowedEffects = DragDropEffects.Move;
			form.DropControl.DropEffect = DragDropEffects.Move;

			Application.Run (form);

			Assert.AreEqual (2, form.DropControl.DropFiredCount, "A1");
			Assert.AreEqual (1, form.DropControl.LeaveFiredCount, "A2");
			Assert.AreEqual (DragDropEffects.Move, form.DragControl.PerformedEffect, "A3");
		}

		[Test]
		public void DropWithoutMovement ()
		{
			DNDForm form = new DNDForm ();
			form.Text = MethodBase.GetCurrentMethod ().Name;
			form.InstructionsText =
				"Instructions:" + Environment.NewLine + Environment.NewLine +
				"1. Click with left button on the control on the left, holding it, WITHOUT MOVING IT." + Environment.NewLine +
				"2. Release the button." + Environment.NewLine + 
				"3. Close the form.";
			form.DragControl.DragData = "no movement";
			form.DragControl.AllowDrop = true;
			form.DragControl.AllowedEffects = DragDropEffects.Move;
			form.DragControl.DropEffect = DragDropEffects.Move;

			// Force to automatically do a dnd operation when mouse is pressed,
			// instead of waiting for movement
			form.DragControl.MouseDown += new MouseEventHandler (DragControl_MouseDown);

			Application.Run (form);

			Assert.AreEqual (1, form.DragControl.EnterFiredCount, "A1");
			Assert.AreEqual (0, form.DragControl.LeaveFiredCount, "A2");
			Assert.AreEqual (1, form.DragControl.DropFiredCount, "A3");
			Assert.AreEqual (0, form.DragControl.DragOverFiredCount, "A4");
			Assert.AreEqual (true, form.DragControl.Data != null, "A5");
			// The assertion below is weird: We had a successfully drop, but the returned value is None
			Assert.AreEqual (DragDropEffects.None, form.DragControl.PerformedEffect, "A6");
		}

		void DragControl_MouseDown (object sender, MouseEventArgs e)
		{
			DragDropControl ctrl = (DragDropControl)sender;
			ctrl.DoDragDrop (ctrl.DragData, ctrl.AllowedEffects);
		}

		[Test]
		public void DragDropInSameControl ()
		{
			DNDForm form = new DNDForm ();
			form.Text = MethodBase.GetCurrentMethod ().Name;
			form.InstructionsText =
				"Instructions:" + Environment.NewLine + Environment.NewLine +
				"1. Click with left button on the control on the left, holding it." + Environment.NewLine +
				"2. Move the mouse inside left control. " + Environment.NewLine +
				"3. Drop on left control (same)." + Environment.NewLine + Environment.NewLine +
				"4. Click with left button on the control on the left again, holding it." + Environment.NewLine +
				"5. Move the mouse inside the left control. " + Environment.NewLine +
				"6. Press ESC, release mouse button and move mouse pointer outside control." + Environment.NewLine +
				"7. Close the form.";
			form.DragControl.DragData = "SameControl";
			form.DragControl.AllowDrop = true;
			form.DragControl.AllowedEffects = DragDropEffects.Copy;

			data = null;
			drag_enter_count = drag_leave_count = 0;
			form.DragControl.DragEnter += new DragEventHandler (DragDropInSameControl_DragEnter);
			form.DragControl.DragLeave += new EventHandler (DragDropInSameControl_DragLeave);
			form.DragControl.DragDrop += new DragEventHandler (DragDropInSameControl_DragDrop);

			Application.Run (form);

			Assert.AreEqual (2, drag_enter_count, "A1");
			Assert.AreEqual (1, drag_leave_count, "A2");
			Assert.AreEqual (1, drag_drop_count, "A3");
			Assert.AreEqual ("SameControl", data, "A4");
		}

		int drag_enter_count;
		int drag_leave_count;
		int drag_drop_count;
		object data;

		void DragDropInSameControl_DragDrop (object sender, DragEventArgs e)
		{
			drag_drop_count++;
			data = e.Data.GetData (typeof (string));
		}

		void DragDropInSameControl_DragLeave (object sender, EventArgs e)
		{
			drag_leave_count++;
		}

		void DragDropInSameControl_DragEnter (object sender, DragEventArgs e)
		{
			e.Effect = DragDropEffects.Copy;
			drag_enter_count++;
		}
	}

	public class DNDForm : Form
	{
		DragDropControl drag_control;
		DragDropControl drop_control;
		TextBox instructions_tb;
		Label test_name;

		public DNDForm ()
		{
			test_name = new Label ();
			test_name.Location = new Point (5, 5);
			test_name.AutoSize = true;
			test_name.Font = new Font (Font, FontStyle.Bold | Font.Style);
			test_name.Text = Text;

			instructions_tb = new TextBox ();
			instructions_tb.Multiline = true;
			instructions_tb.ScrollBars = ScrollBars.Vertical;
			instructions_tb.Location = new Point (5, test_name.Bottom + 5);
			instructions_tb.Size = new Size (460, 180);

			drag_control = new DragDropControl ();
			drag_control.Location = new Point (5, instructions_tb.Bottom + 10);
			drag_control.Size = new Size (220, 180);
			drag_control.BackColor = Color.LightYellow;
			drag_control.DragDropColor = Color.Yellow;
			drag_control.Text = "Drag Control";
			drag_control.DoDrag = true;

			drop_control = new DragDropControl ();
			drop_control.Location = new Point (drag_control.Right + 20, instructions_tb.Bottom + 10);
			drop_control.Size = new Size (220, 180);
			drop_control.BackColor = Color.LightGreen;
			drop_control.DragDropColor = Color.Green;
			drop_control.Text = "Drop Control";
			drop_control.AllowDrop = true;

			Controls.AddRange (new Control [] { test_name, instructions_tb, drag_control, drop_control });

			Size = new Size (480, 440);
		}

		public string InstructionsText {
			get {
				return instructions_tb.Text;
			}
			set {
				instructions_tb.Text = value;
			}
		}

		public DragDropControl DragControl {
			get {
				return drag_control;
			}
		}

		public DragDropControl DropControl {
			get {
				return drop_control;
			}
		}

		protected override void OnTextChanged (EventArgs args)
		{
			base.OnTextChanged (args);
			test_name.Text = Text;
		}
	}

	public class DragDropControl : Control
	{
		DragDropEffects effect;
		DragDropEffects allowed_effects;
		DragDropEffects performed_effect;
		object drag_data;
		IDataObject data;
		Color drop_color;
		Color prev_color;
		Rectangle drag_rect;
		bool do_drag;

		int drop_fired_count;
		int leave_fired_count;
		int drag_over_fired_count;
		int enter_fired_count;

		public DragDropControl ()
		{
			drag_rect = new Rectangle (new Point (-1, -1), SystemInformation.DragSize);
		}

		// to call or not DoDragDrop when mouse movement is detected. Only handle dnd events otherwise.
		public bool DoDrag {
			get {
				return do_drag;
			}
			set {
				do_drag = value;
			}
		}

		// Color of the control when an operation is having place
		public Color DragDropColor {
			get {
				return drop_color;
			} 
			set {
				drop_color = value;
			}
		}

		// Data to pass to Control.DoDragDrop 
		public object DragData {
			get {
				return drag_data;
			}
			set {
				drag_data = value;
			}
		}

		// Effects passed to Control.DoDragDrop
		public DragDropEffects AllowedEffects {
			get {
				return allowed_effects;
			}
			set {
				allowed_effects = value;
			}
		}

		// Effect returned by Control.DoDragDrop
		public DragDropEffects PerformedEffect {
			get {
				return performed_effect;
			}
		}

		// The value DragEventArgs.Effect gets in DragEnter event
		public DragDropEffects DropEffect {
			get {
				return effect;
			}
			set {
				effect = value;
			}
		}

		// The value in DragEventArgs.Data
		public IDataObject Data {
			get {
				return data;
			}
		}

		public int DropFiredCount {
			get {
				return drop_fired_count;
			}
		}

		public int EnterFiredCount {
			get {
				return enter_fired_count;
			}
		}

		public int LeaveFiredCount {
			get {
				return leave_fired_count;
			}
		}

		public int DragOverFiredCount {
			get {
				return drag_over_fired_count;
			}
		}

		public void ResetEventCounters ()
		{
			drop_fired_count = enter_fired_count = leave_fired_count = drag_over_fired_count = 0;
		}

		protected override void OnDragEnter (DragEventArgs drgevent)
		{
			if (!do_drag) {
				prev_color = BackColor;
				BackColor = drop_color;
			}

			enter_fired_count++;
			drgevent.Effect = effect;

			base.OnDragEnter (drgevent);
		}

		protected override void OnDragOver (DragEventArgs drgevent)
		{
			drag_over_fired_count++;

			base.OnDragOver (drgevent);
		}

		protected override void OnDragLeave (EventArgs e)
		{
			data = null;
			leave_fired_count++;

			if (!do_drag)
				BackColor = prev_color;

			base.OnDragLeave (e);
		}

		protected override void OnDragDrop (DragEventArgs drgevent)
		{
			data = drgevent.Data;
			drop_fired_count++;

			if (!do_drag)
				BackColor = prev_color;

			base.OnDragDrop (drgevent);
		}

		protected override void OnMouseMove (MouseEventArgs e)
		{
			base.OnMouseMove (e);

			if (!do_drag)
				return;

			if (e.Button == MouseButtons.Left || e.Button == MouseButtons.Right) {

				if (drag_rect.X == -1 && drag_rect.Y == -1)
					drag_rect.Location = new Point (e.X, e.Y);
				else {
					if (!drag_rect.Contains (new Point (e.X, e.Y))) {
						Color prev_color = BackColor;
						BackColor = drop_color;

						performed_effect = DoDragDrop (drag_data, allowed_effects);

						drag_rect.Location = new Point (-1, -1);
						BackColor = prev_color;
					}
				}

			}
		}

		protected override void OnPaint (PaintEventArgs e)
		{
			base.OnPaint (e);

			Graphics g = e.Graphics;

			StringFormat sf = new StringFormat ();
			sf.Alignment = StringAlignment.Center;
			sf.LineAlignment = StringAlignment.Center;

			g.DrawString (Text, SystemFonts.DefaultFont, SystemBrushes.ControlDark,
				ClientRectangle, sf);

			sf.Dispose ();
		}
	}
}

