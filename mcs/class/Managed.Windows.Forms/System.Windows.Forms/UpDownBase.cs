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
// Copyright (c) 2004 Novell, Inc.
//
// Authors:
//	Miguel de Icaza (miguel@novell.com).
//
//
/*

TODO:

	- Actually paint the border, could not get it to work.

	- Implement ContextMenu property.
	
	- Force the size of the entry: it can not be resized vertically
	  ever, the size is set by the size of the font

	- Add defaults with [DefautValue ]

	- Audit every place where ChangingText is used, whose meaning is
	  `Me, the library is changing the text'.  Kind of hack to deal with
	  loops on events.

	- Hook up the keyboard events.

*/
using System;
using System.Drawing;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace System.Windows.Forms {
	public abstract class UpDownBase : ContainerControl {

		internal class Spinner : Control, IDisposable {
			UpDownBase updownbase;
			Rectangle up, down, pressed;
			Timer timer;
			bool up_pressed, down_pressed;
			bool captured, mouse_in;

			//
			// Interval values
			//
			const int StartInterval = 1000;
			const int RepeatInterval = 400;
			const int ChangeInterval = 75;
			const int MinimumInterval = 100;
			
			internal Spinner (UpDownBase updownbase)
			{
				this.updownbase = updownbase;
			}

			protected override void OnPaint (PaintEventArgs args)
			{
				Draw (args.ClipRectangle);
				args.Graphics.DrawImage (ImageBuffer, 0, 0);
			}

			protected override void OnLayout (LayoutEventArgs args)
			{
				base.OnLayout (args);
				Rectangle bounds = Bounds;

				up = new Rectangle (0, 0, bounds.Width, bounds.Height/2);
				down = new Rectangle (0, bounds.Height/2, bounds.Width, bounds.Height/2);
			}

			protected override void OnMouseDown (MouseEventArgs args)
			{
				base.OnMouseDown (args);

				if (args.Button != MouseButtons.Left)
					return;

				if (up.Contains (args.X, args.Y)){
					up_pressed = true;
					pressed = up;
				} else if (down.Contains (args.X, args.Y)){
					down_pressed = true;
					pressed = down;
				} else
					return;

				Click ();
				Invalidate (pressed);
				Capture = true;
				InitTimer ();
				
				mouse_in = down_pressed | up_pressed;
			}

			protected override void OnMouseUp (MouseEventArgs args)
			{
				if (Capture){
					if (up_pressed){
						up_pressed = false;
						Invalidate (up);
					}
					if (down_pressed){
						down_pressed = false;
						Invalidate (down);
					}
				}
				Capture = false;
				timer.Enabled = false;
			}

			//
			// Sets up the auto-repeat timer, we give a one second
			// delay, and then we use the keyboard settings for auto-repeat.
			//
			void InitTimer ()
			{
				if (timer != null){
					timer.Interval = StartInterval;
					timer.Enabled = true;
					return;
				}
				
				timer = new Timer ();
				int kd = SystemInformation.KeyboardDelay;
				kd = kd < 0 ? 0 : (kd > 4 ? 4 : kd);
				timer.Interval = StartInterval;
				timer.Tick += new EventHandler (ClockTick);
				timer.Enabled = true;
			}

			void ClockTick (object o, EventArgs a)
			{
				if (timer == null)
					throw new Exception ("The timer that owns this callback is null!");
				
				int interval = timer.Interval;

				if (interval == StartInterval)
					interval = RepeatInterval;
				else
					interval -= ChangeInterval;

				if (interval < MinimumInterval)
					interval = MinimumInterval;
				timer.Interval = interval;

				Click ();
			}

			void Click ()
			{
				if (up_pressed)
					updownbase.UpButton ();
				if (down_pressed)
					updownbase.DownButton ();
			}

			protected override void OnMouseMove (MouseEventArgs args)
			{
				base.OnMouseMove (args);
				if (Capture){
					bool old = mouse_in;

					if (pressed.Contains (args.X, args.Y)){
						if (timer == null)
							InitTimer ();
						mouse_in = true;
					} else {
						if (timer != null){
							timer.Enabled = false;
							timer.Dispose ();
							timer = null;
						}
						mouse_in = false;
					}
					if (mouse_in ^ old){
						Console.WriteLine ("STATE CHANGE");
						if (mouse_in)
							Click ();
						Invalidate (pressed);
					}
				}
			}
			
			void DrawUp ()
			{
				ButtonState bs;

				bs = mouse_in && up_pressed ? ButtonState.Pushed : ButtonState.Normal;
				ThemeEngine.Current.CPDrawScrollButton (DeviceContext, up, ScrollButton.Up, bs);
			}
			
			void DrawDown ()
			{
				ButtonState bs;

				bs = mouse_in && down_pressed ? ButtonState.Pushed : ButtonState.Normal;
				ThemeEngine.Current.CPDrawScrollButton (DeviceContext, down, ScrollButton.Down, bs);
			}

			void Draw (Rectangle clip)
			{
				if (clip.Contains (up))
					DrawUp ();
				if (clip.Contains (down))
					DrawDown ();
			}

			void IDisposable.Dispose ()
			{
				if (timer != null){
					timer.Stop ();
					timer.Dispose ();
				}
				timer = null;
				base.Dispose ();
			}
		}

		BorderStyle border_style = BorderStyle.Fixed3D;
		int desired_height = 0;
		TextBox entry;
		Spinner spinner;
		int border;
		int scrollbar_button_size = ThemeEngine.Current.ScrollBarButtonSize;
		LeftRightAlignment updown_align = LeftRightAlignment.Right;
		bool changing_text = false;
		bool user_edit = false;
		
		public UpDownBase () : base ()
		{
			SuspendLayout ();

			entry = new TextBox ();
			entry.Font = Font;
			entry.Size = new Size (120, Font.Height);
			entry.LostFocus += new EventHandler (EntryOnLostFocus);
			entry.TextChanged += new EventHandler (OnTextBoxTextChanged);
			entry.ReadOnly = false;
			Controls.Add (entry);

			spinner = new Spinner (this);
			Controls.Add (spinner);

			ComputeSizeAndLocation ();
			ResumeLayout ();
		}

		void EntryOnLostFocus (object sender, EventArgs e)
		{
			OnLostFocus (e);
		}

		void ComputeSizeAndLocation ()
		{
			if (BorderStyle == BorderStyle.Fixed3D)
				border = 2;
			else if (BorderStyle == BorderStyle.FixedSingle)
				border = 1;
			else
				border = 0;
				
			Size = (entry.Size + spinner.Size + new Size (border, border));
			
			entry.Location = new Point (border, border);
		}
		

#region UpDownBase overwritten methods
		
		protected override void OnMouseWheel (MouseEventArgs args)
		{
			base.OnMouseWheel (args);

			if (args.Delta > 0)
				UpButton ();
			else if (args.Delta < 0)
				DownButton ();
		}

		protected virtual void OnChanged (object source, EventArgs e)
		{
			// Not clear, the docs state that this will raise the
			// Changed event, but that event is not listed anywhere.
		}

		protected override void OnFontChanged (EventArgs e)
		{
			base.OnFontChanged (e);

			entry.Font = Font;
			desired_height = entry.Height;
			Height = desired_height;
		}

		protected override void OnHandleCreated (EventArgs e)
		{
			base.OnHandleCreated (e);
			desired_height = entry.Height;
		}
				
		protected override void OnLayout (LayoutEventArgs args)
		{
			base.OnLayout (args);

			Rectangle bounds = Bounds;
			int entry_width = bounds.Right - scrollbar_button_size - 1;

			entry.SetBounds (bounds.X, bounds.Y, entry_width, bounds.Height);
			spinner.SetBounds (entry_width + 1, bounds.Y, scrollbar_button_size, bounds.Height);
		}

#if NET_2_0
		protected override void OnPaint (PaintEventArgs e)
		{
			base.OnPaint (e);
			ThemeEngine.Current.CPDrawBorderStyle (e.Graphics, ClientRectangle, BorderStyle.Fixed3D);
			e.Graphics.DrawImage (ImageBuffer, 0, 0);
		}

		protected override void SetVisibleCore (bool state)
		{
			base.SetVisibleCore (state);
		}
#endif

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected override void WndProc (ref Message m)
		{
			base.WndProc (ref m);
		}

		protected override void SetBoundsCore (int x, int y, int width, int height, BoundsSpecified specified)
		{
			//
			// Force the size to be our height.
			//
			base.SetBoundsCore (x, y, width, desired_height, specified);
		}
		
		protected override void Dispose (bool disposing)
		{
			if (spinner != null){
				if (disposing){
					spinner.Dispose ();
					entry.Dispose ();
				}
			}
			spinner = null;
			entry = null;
			base.Dispose (true);
		}
		
#endregion
		
#region UpDownBase virtual methods
		//
		// These are hooked up to the various events from the Entry line that
		// we do not have yet, and implement the keyboard behavior (use a different
		// widget to test)
		//
		protected virtual void OnTextBoxKeyDown (object source, KeyEventArgs e)
		{
		}
		
		protected virtual void OnTextBoxKeyPress (object source, KeyPressEventArgs e)
		{
		}

		protected virtual void OnTextBoxLostFocus (object source, EventArgs e)
		{
		}

		protected virtual void OnTextBoxResize (object source, EventArgs e)
		{
		}

		protected virtual void OnTextBoxTextChanged (object source, EventArgs e)
		{
#if false
			if (changing_text)
				return;
			changing_text = false;
			user_edit = true;
			OnChanged (source, e);
#endif
		}

#endregion

#region UpDownBase Properties

		/* FIXME: Do not know what Autoscroll should do */
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		[Browsable(false)]
		public virtual bool AutoScroll {
			get {
				return base.AutoScroll;
			}

			set {
				base.AutoScroll = value;
			}
		}

		/* FIXME: Do not know what AutoscrollMargin does */
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		[Browsable(false)]
		public new Size AutoScrollMargin {
			get {
				return base.AutoScrollMargin;
			}

			set {
				base.AutoScrollMargin = value;
			}
		}

		/* FIXME: Do not know what AutoscrollMinSize does */
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		[Browsable(false)]
		public new Size AutoScrollMinSize {
			get {
				return base.AutoScrollMinSize;
			}

			set {
				base.AutoScrollMinSize = value;
			}
		}

		public override Color BackColor {
			get {
				return base.BackColor;
			}

			set {
				entry.BackColor = value;
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		[Browsable(false)]
		public override Image BackgroundImage {
			get {
				return entry.BackgroundImage;
			}

			set {
				entry.BackgroundImage = value;
			}
		}

		[DefaultValue(BorderStyle.Fixed3D)]
		[DispId(-504)]
		public BorderStyle BorderStyle {
			get {
				return border_style;
			}

			set {
				border_style = value;

				SuspendLayout ();
				ComputeSizeAndLocation ();
				ResumeLayout ();
			}
		}

		//
		// Used internally to flag when the derivative classes are changing
		// the Text property as opposed to the user
		//
		protected bool ChangingText {
			get {
				return changing_text;
			}

			set {
				changing_text = value;
			}
		}

		// TODO: What should this do?
		public override ContextMenu ContextMenu {
			get {
				return null;
			}

			set {
				/* */
			}
		}

		protected override CreateParams CreateParams {
			get {
				return base.CreateParams;
			}
		}

		protected bool UserEdit {
			get {
				return user_edit;
			}

			set {
				user_edit = value;
			}
		}

		public override string Text {
			get {
				if (entry == null)
					return String.Empty;
				
				return entry.Text;
			}

			set {
				//
				// The documentation is conflicts with itself, we can
				// not call UpdateEditText, as this will call Text to
				// set the value.
				//
				entry.Text = value;
				
				if (UserEdit)
					ValidateEditText ();
				
				if (ChangingText)
					ChangingText = false;
			}
		}

		public LeftRightAlignment UpDownAlign
		{
			get {
				return updown_align;
			}
			
			set {
				updown_align = value;
                        }
                }

		[DefaultValue(false)]
		public bool ReadOnly {
			get {
				return entry.ReadOnly;
			}

			set {
				entry.ReadOnly = value;
			}
		}
		
#endregion
		
#region UpDownBase standard methods
		public void Select (int start, int length)
		{
			entry.Select (start, length);
		}

		protected virtual void ValidateEditText ()
		{
			// 
		}
#endregion

#region Events
		//
		// All these events are just a proxy to the base class,
		// we must overwrite them for API compatibility
		//
		
		public new event EventHandler MouseEnter {
			add {
				base.MouseEnter += value;
			}

			remove {
				base.MouseEnter -= value;
			}
		}

		public new event EventHandler MouseHover {
			add {
				base.MouseHover += value;
			}

			remove {
				base.MouseHover -= value;
			}
		}

		public new event EventHandler MouseLeave {
			add {
				base.MouseLeave += value;
			}

			remove {
				base.MouseLeave -= value;
			}
		}

		public new event EventHandler BackgroundImageChanged {
			add {
				base.BackgroundImageChanged += value;
			}

			remove {
				base.BackgroundImageChanged -= value;
			}
		}
#endregion
		
#region Abstract methods
		public abstract void DownButton ();
		public abstract void UpButton ();
		public abstract void UpdateEditText ();
#endregion
	}
}
