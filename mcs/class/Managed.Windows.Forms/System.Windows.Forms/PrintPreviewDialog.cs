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
// Copyright (c) 2006 Novell, Inc.
//
// Authors:
//	Jonathan Chambers (jonathan.chambers@ansys.com)
//	Peter Dennis Bartok (pbartok@novell.com)
//

using System;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.ComponentModel.Design.Serialization;
using System.Collections;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Printing;
using System.Reflection;
using System.Runtime.InteropServices;

namespace System.Windows.Forms {
	[DefaultProperty("Document")]
	[Designer("System.ComponentModel.Design.ComponentDesigner, " + Consts.AssemblySystem_Design)]
	[DesignTimeVisible(true)]
	[ToolboxItem(true)]
	[ClassInterface (ClassInterfaceType.AutoDispatch)]
	[ComVisible (true)]
	[ToolboxItemFilter ("System.Windows.Forms.Control.TopLevel", ToolboxItemFilterType.Allow)]
	public class PrintPreviewDialog : Form {
		PrintPreviewControl print_preview;
		MenuItem previous_checked_menu_item;
		Menu mag_menu;
		MenuItem auto_zoom_item;
		NumericUpDown pageUpDown;

		public PrintPreviewDialog()
		{
			this.ClientSize = new Size (400, 300);
			ToolBar toolbar = CreateToolBar ();

			toolbar.Location = new Point (0, 0);
			toolbar.Dock = DockStyle.Top;
			Controls.Add (toolbar);


			print_preview = new PrintPreviewControl();
			print_preview.Location = new Point (0, toolbar.Location.Y + toolbar.Size.Height);
			print_preview.Size = new Size (ClientSize.Width, ClientSize.Height - toolbar.Bottom);
			print_preview.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
			print_preview.TabStop = false;
			Controls.Add (print_preview);
			print_preview.Show ();
		}

		ToolBar CreateToolBar ()
		{
			ImageList image_list = new ImageList ();
			image_list.Images.Add (ResourceImageLoader.Get ("32_printer.png"));
			image_list.Images.Add (ResourceImageLoader.Get ("22_page-magnifier.png"));
			image_list.Images.Add (ResourceImageLoader.Get ("1-up.png"));
			image_list.Images.Add (ResourceImageLoader.Get ("2-up.png"));
			image_list.Images.Add (ResourceImageLoader.Get ("3-up.png"));
			image_list.Images.Add (ResourceImageLoader.Get ("4-up.png"));
			image_list.Images.Add (ResourceImageLoader.Get ("6-up.png"));

			MenuItem mi;
			mag_menu = new ContextMenu ();

			ToolBar toolbar = new PrintToolBar();
			ToolBarButton print = new ToolBarButton();
			ToolBarButton zoom = new ToolBarButton();
			ToolBarButton separator1 = new ToolBarButton();

			ToolBarButton one_page = new ToolBarButton();
			ToolBarButton two_page = new ToolBarButton();
			ToolBarButton three_page = new ToolBarButton();
			ToolBarButton four_page = new ToolBarButton();
			ToolBarButton six_page = new ToolBarButton();
			ToolBarButton separator2 = new ToolBarButton();

			Button close = new Button();
			Label label = new Label();
			pageUpDown = new NumericUpDown();

			toolbar.ImageList = image_list;
			toolbar.Size = new Size(792, 26);
			toolbar.Dock = DockStyle.Top;
			toolbar.Appearance = ToolBarAppearance.Flat;
			toolbar.ShowToolTips = true;
			toolbar.DropDownArrows = true;
			toolbar.TabStop = true;
			toolbar.Buttons.AddRange(new ToolBarButton[] { print, zoom, separator1, 
														   one_page, two_page, three_page, four_page, six_page, separator2 });
			toolbar.ButtonClick += new ToolBarButtonClickEventHandler (OnClickToolBarButton);

			/* print button */
			print.ImageIndex = 0;
			print.Tag = 0;
			print.ToolTipText = "Print";

			/* magnify dropdown */
			zoom.ImageIndex = 1;
			zoom.Tag = 1;
			zoom.ToolTipText = "Zoom";
			zoom.Style = ToolBarButtonStyle.DropDownButton;
			zoom.DropDownMenu = mag_menu;
		
			mi = mag_menu.MenuItems.Add ("Auto", new EventHandler (OnClickPageMagnifierItem)); mi.RadioCheck = true;
			mi.Checked = true;
			previous_checked_menu_item = mi;
			auto_zoom_item = mi;

			mi = mag_menu.MenuItems.Add ("500%", new EventHandler (OnClickPageMagnifierItem)); mi.RadioCheck = true;
			mi = mag_menu.MenuItems.Add ("200%", new EventHandler (OnClickPageMagnifierItem)); mi.RadioCheck = true;
			mi = mag_menu.MenuItems.Add ("150%", new EventHandler (OnClickPageMagnifierItem)); mi.RadioCheck = true;
			mi = mag_menu.MenuItems.Add ("100%", new EventHandler (OnClickPageMagnifierItem)); mi.RadioCheck = true;
			mi = mag_menu.MenuItems.Add ("75%", new EventHandler (OnClickPageMagnifierItem)); mi.RadioCheck = true;
			mi = mag_menu.MenuItems.Add ("50%", new EventHandler (OnClickPageMagnifierItem)); mi.RadioCheck = true;
			mi = mag_menu.MenuItems.Add ("25%", new EventHandler (OnClickPageMagnifierItem)); mi.RadioCheck = true;
			mi = mag_menu.MenuItems.Add ("10%", new EventHandler (OnClickPageMagnifierItem)); mi.RadioCheck = true;


			/* separator */
			separator1.Style = ToolBarButtonStyle.Separator;

			/* n-up icons */
			one_page.ImageIndex = 2;
			one_page.Tag = 2;
			one_page.ToolTipText = "One page";

			two_page.ImageIndex = 3;
			two_page.Tag = 3;
			two_page.ToolTipText = "Two pages";

			three_page.ImageIndex = 4;
			three_page.Tag = 4;
			three_page.ToolTipText = "Three pages";
			
			four_page.ImageIndex = 5;
			four_page.Tag = 5;
			four_page.ToolTipText = "Four pages";
			
			six_page.ImageIndex = 6;
			six_page.Tag = 6;
			six_page.ToolTipText = "Six pages";
			

			/* separator */
			separator2.Style = ToolBarButtonStyle.Separator;

			/* Page label */
			label.Text = "Page";
			label.TabStop = false;
			label.Size = new Size(50, 18);
			label.TextAlign = ContentAlignment.MiddleLeft;
			label.Dock = DockStyle.Right;

			/* the page number updown */
			pageUpDown.Dock = DockStyle.Right;
			pageUpDown.TextAlign = HorizontalAlignment.Right;
			pageUpDown.DecimalPlaces = 0;
			pageUpDown.TabIndex = 1;
			pageUpDown.Text = "1";
			pageUpDown.Minimum = 0;
			pageUpDown.Maximum = 1000;
			pageUpDown.Size = new Size(64, 14);
			pageUpDown.Dock = DockStyle.Right;
//			pageUpDown.Location = new Point(568, 0);
			pageUpDown.ValueChanged += new EventHandler (OnPageUpDownValueChanged);

			/* close button */
			close.Location = new Point(196, 2);
			close.Size = new Size(50, 20);
			close.TabIndex = 0;
			close.FlatStyle = FlatStyle.Popup;
			close.Text = "Close";
			close.Click += new EventHandler (CloseButtonClicked);

			toolbar.Controls.Add(label);
			toolbar.Controls.Add(pageUpDown);
			toolbar.Controls.Add(close);

//			close.Location = new Point (b.Rectangle.X + b.Rectangle.Width, toolbar.Height / 2 - close.Height / 2);
//			MinimumSize = new Size (close.Location.X + close.Width + label.Width + pageUpDown.Width, 220);

			return toolbar;
		}

		class PrintToolBar : ToolBar
		{
			bool left_pressed;

			public int GetNext (int pos)
			{
				// Select the next button that is *not* a separator
				while (++pos < items.Length && items [pos].Button.Style == ToolBarButtonStyle.Separator)
					;

				return pos;
			}

			public int GetPrev (int pos)
			{
				// Select the previous button that is *not* a separator
				while (--pos > -1 && items [pos].Button.Style == ToolBarButtonStyle.Separator)
					;

				return pos;
			}

			void SelectNextOnParent (bool forward)
			{
				ContainerControl container = Parent as ContainerControl;
				if (container != null && container.ActiveControl != null)
					container.SelectNextControl (container.ActiveControl, forward, true, true, true);
			}

			protected override void OnGotFocus (EventArgs args)
			{
				base.OnGotFocus (args);

				// Select either the last one or the first one, depending on the direction
				CurrentItem = (Control.ModifierKeys & Keys.Shift) != 0 || left_pressed ? GetPrev (items.Length) : 0;
				left_pressed = false;
			}

			// We need to handle Left/Right for our controls by ourselves, as opposed to
			// Tab
			protected override bool ProcessDialogKey (Keys keyData)
			{
				switch ((keyData & Keys.KeyCode)) {
					case Keys.Left:
						left_pressed = true; // Simulate Tab+Alt if focus goes to our buttons
						SelectNextOnParent (false);
						return true;
					case Keys.Right:
						SelectNextOnParent (true);
						return true;
				}

				return base.ProcessDialogKey (keyData);
			}

			void NavigateItems (Keys key)
			{
				bool forward = true;
				switch ((key & Keys.KeyCode)) {
					case Keys.Left:
						forward = false;
						break;
					case Keys.Right:
						forward = true;
						break;
					case Keys.Tab:
						forward = (Control.ModifierKeys & Keys.Shift) == 0;
						break;
				}

				int pos = forward ? GetNext (CurrentItem) : GetPrev (CurrentItem);
				if (pos < 0 || pos >= items.Length) { // go to the prev/next control
					CurrentItem = -1;
					SelectNextOnParent (forward);
					return;
				}
				
				CurrentItem = pos;
			}

			bool OnDropDownButton {
				get {
					return CurrentItem != -1 && items [CurrentItem].Button.Style == ToolBarButtonStyle.DropDownButton;
				}
			}

			internal override bool InternalPreProcessMessage (ref Message msg)
			{
				Keys key = (Keys)msg.WParam.ToInt32 ();
				switch (key) {
					// Up/Down keys are processed only if we are
					// on a dropdown button, and ignored otherwise.
					case Keys.Up:
					case Keys.Down:
						if (OnDropDownButton)
							break; // process base impl.
						return true;
					case Keys.Left:
					case Keys.Right:
					case Keys.Tab:
						if (OnDropDownButton)
							((ContextMenu)(items [CurrentItem].Button.DropDownMenu)).Hide ();

						NavigateItems (key);
						return true;
				}

				return base.InternalPreProcessMessage (ref msg);
			}
		}

		void CloseButtonClicked (object sender, EventArgs e)
		{
			Close ();
		}

		void OnPageUpDownValueChanged (object sender, EventArgs e)
		{
			print_preview.StartPage = (int)pageUpDown.Value;
		}

		void OnClickToolBarButton (object sender, ToolBarButtonClickEventArgs e)
		{
			if (e.Button.Tag == null || !(e.Button.Tag is int))
				return;

			switch ((int)e.Button.Tag)
			{
			case 0:
				Console.WriteLine ("do print here");
				break;
			case 1:
				OnClickPageMagnifierItem (auto_zoom_item, EventArgs.Empty);
				break;
			case 2:
				print_preview.Rows = 0;
				print_preview.Columns = 1;
				break;
			case 3:
				print_preview.Rows = 0;
				print_preview.Columns = 2;
				break;
			case 4:
				print_preview.Rows = 0;
				print_preview.Columns = 3;
				break;
			case 5:
				print_preview.Rows = 1;
				print_preview.Columns = 2;
				break;
			case 6:
				print_preview.Rows = 1;
				print_preview.Columns = 3;
				break;
			}
		}

		void OnClickPageMagnifierItem (object sender, EventArgs e)
		{
			MenuItem clicked_item = (MenuItem)sender;

			previous_checked_menu_item.Checked = false;

			switch (clicked_item.Index) {
			case 0:
				print_preview.AutoZoom = true;
				break;
			case 1:
				print_preview.AutoZoom = false;
				print_preview.Zoom = 5.0;
				break;
			case 2:
				print_preview.AutoZoom = false;
				print_preview.Zoom = 2.0;
				break;
			case 3:
				print_preview.AutoZoom = false;
				print_preview.Zoom = 1.5;
				break;
			case 4:
				print_preview.AutoZoom = false;
				print_preview.Zoom = 1.0;
				break;
			case 5:
				print_preview.AutoZoom = false;
				print_preview.Zoom = 0.75;
				break;
			case 6:
				print_preview.AutoZoom = false;
				print_preview.Zoom = 0.50;
				break;
			case 7:
				print_preview.AutoZoom = false;
				print_preview.Zoom = 0.25;
				break;
			case 8:
				print_preview.AutoZoom = false;
				print_preview.Zoom = 0.10;
				break;
			}

			clicked_item.Checked = true;
			previous_checked_menu_item = clicked_item;
		}

		// new property so we can set EditorBrowsable to never.
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new IButtonControl AcceptButton {
			get { return base.AcceptButton; }
			set { base.AcceptButton = value; }
		}

		// new property so we can set EditorBrowsable to never.
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new string AccessibleDescription {
			get { return base.AccessibleDescription; }
			set { base.AccessibleDescription = value; }
		}
 
		// new property so we can set EditorBrowsable to never.
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new string AccessibleName {
			get { return base.AccessibleName; }
			set { base.AccessibleName = value; }
		}
 
		// new property so we can set EditorBrowsable to never.
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new AccessibleRole AccessibleRole {
			get { return base.AccessibleRole; }
			set { base.AccessibleRole = value; }
		}
 
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public override bool AllowDrop {
			get {
				return base.AllowDrop;
			}

			set {
				base.AllowDrop = value;
			}
		}
 
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public override AnchorStyles Anchor {
			get {
				return base.Anchor;
			}

			set {
				base.Anchor = value;
			}
		}

		// new property so we can set EditorBrowsable to never.
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new bool AutoScale {
			get { return base.AutoScale; }
			set { base.AutoScale = value; }
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete ("This property has been deprecated.  Use AutoScaleDimensions instead.")]
		public override Size AutoScaleBaseSize {
			get {
				return base.AutoScaleBaseSize;
			}

			set {
				base.AutoScaleBaseSize = value;
			}
		}
 
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public override bool AutoScroll {
			get {
				return base.AutoScroll;
			}

			set {
				base.AutoScroll = value;
			}
		}
 
		// new property so we can set EditorBrowsable to never.
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new Size AutoScrollMargin {
			get { return base.AutoScrollMargin; }
			set { base.AutoScrollMargin = value; }
		}

		// new property so we can set EditorBrowsable to never.
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new Size AutoScrollMinSize {
			get { return base.AutoScrollMinSize; }
			set { base.AutoScrollMinSize = value; }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public override bool AutoSize {
			get { return base.AutoSize; }
			set { base.AutoSize = value; }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public override AutoValidate AutoValidate {
			get { return base.AutoValidate; }
			set { base.AutoValidate = value; }
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public override Color BackColor {
			get {
				return base.BackColor;
			}

			set {
				base.BackColor = value;
			}
		}
 
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public override Image BackgroundImage {
			get {
				return base.BackgroundImage;
			}

			set {
				base.BackgroundImage = value;
			}
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public override ImageLayout BackgroundImageLayout {
			get { return base.BackgroundImageLayout; }
			set { base.BackgroundImageLayout = value; }
		}

		// new property so we can set EditorBrowsable to never.
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new IButtonControl CancelButton {
			get { return base.CancelButton; }
			set { base.CancelButton = value; }
		}
 
		// new property so we can set EditorBrowsable to never.
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new bool CausesValidation {
			get { return base.CausesValidation; }
			set { base.CausesValidation = value; }
		}
 
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public override ContextMenu ContextMenu {
			get {
				return base.ContextMenu;
			}

			set {
				base.ContextMenu = value;
			}
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public override ContextMenuStrip ContextMenuStrip {
			get { return base.ContextMenuStrip; }
			set { base.ContextMenuStrip = value; }
		}

		// new property so we can set EditorBrowsable to never.
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new bool ControlBox {
			get { return base.ControlBox; }
			set { base.ControlBox = value; }
		}
 
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public override Cursor Cursor {
			get {
				return base.Cursor;
			}

			set {
				base.Cursor = value;
			}
		}
 
		// new property so we can set EditorBrowsable to never.
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new ControlBindingsCollection DataBindings {
			get { return base.DataBindings; }
		}

		protected override Size DefaultMinimumSize {
			get { return new Size (370, 300); }
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public override DockStyle Dock {
			get {
				return base.Dock;
			}

			set {
				base.Dock = value;
			}
		}
 
		// new property so we can set EditorBrowsable to never.
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new DockPaddingEdges DockPadding {
			get { return base.DockPadding; }
		}
 
		
		[DefaultValue(null)]
		public PrintDocument Document {
			get { return print_preview.Document; }
			set {
				print_preview.Document = value;
			}
		}

		// new property so we can set EditorBrowsable to never.
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new bool Enabled {
			get { return base.Enabled; }
			set { base.Enabled = value; }
		}
 
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public override Font Font {
			get {
				return base.Font;
			}

			set {
				base.Font = value;
			}
		}
 
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public override Color ForeColor {
			get {
				return base.ForeColor;
			}

			set {
				base.ForeColor = value;
			}
		}
 
		// new property so we can set EditorBrowsable to never.
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new FormBorderStyle FormBorderStyle {
			get { return base.FormBorderStyle; }
			set { base.FormBorderStyle = value; }
		}
 
		// new property so we can set EditorBrowsable to never.
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new bool HelpButton {
			get { return base.HelpButton; }
			set { base.HelpButton = value; }
		}

		// new property so we can set EditorBrowsable to never.
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new Icon Icon {
			get { return base.Icon; }
			set { base.Icon = value; }
		}
 
		// new property so we can set EditorBrowsable to never.
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new ImeMode ImeMode {
			get { return base.ImeMode; }
			set { base.ImeMode = value; }
		}
 
		// new property so we can set EditorBrowsable to never.
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new bool IsMdiContainer {
			get { return base.IsMdiContainer; }
			set { base.IsMdiContainer = value; }
		}
 
		// new property so we can set EditorBrowsable to never.
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new bool KeyPreview {
			get { return base.KeyPreview; }
			set { base.KeyPreview = value; }
		}
 
		// new property so we can set EditorBrowsable to never.
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public new Point Location {
			get { return base.Location; }
			set { base.Location = value; }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new Padding Margin {
			get { return base.Margin; }
			set { base.Margin = value; }
		}

		// new property so we can set EditorBrowsable to never.
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new bool MaximizeBox {
			get { return base.MaximizeBox; }
			set { base.MaximizeBox = value; }
		}
 
		// new property so we can set EditorBrowsable to never.
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new Size MaximumSize {
			get { return base.MaximumSize; }
			set { base.MaximumSize = value; }
		}
 
		// new property so we can set EditorBrowsable to never.
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new MainMenu Menu {
			get { return base.Menu; }
			set { base.Menu = value; }
		}
 
		// new property so we can set EditorBrowsable to never.
		[Browsable(false)]
		[DefaultValue(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new bool MinimizeBox {
			get { return base.MinimizeBox; }
			set { base.MinimizeBox = value; }
		}
 
		// new property so we can set EditorBrowsable to never.
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public new Size MinimumSize {
			get { return base.MinimumSize; }
			set { base.MinimumSize = value; }
		}
 
		// new property so we can set Browsable/EditorBrowsable.
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		public new double Opacity {
			get { return base.Opacity; }
			set { base.Opacity = value; }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new Padding Padding {
			get { return base.Padding; }
			set { base.Padding = value; }
		}
 
		[Browsable(false)]
		public PrintPreviewControl PrintPreviewControl {
			get { return print_preview; }
		}
 
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public override RightToLeft RightToLeft {
			get {
				return base.RightToLeft;
			}

			set {
				base.RightToLeft = value;
			}
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public override bool RightToLeftLayout {
			get { return base.RightToLeftLayout; }
			set { base.RightToLeftLayout = value; }
		}

		// new property so we can set Browsable/EditorBrowsable
		[Browsable(false)]
		[DefaultValue(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new bool ShowInTaskbar {
			get { return base.ShowInTaskbar; }
			set { base.ShowInTaskbar = value; }
		}
 
		// new property so we can set Browsable/EditorBrowsable
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new Size Size {
			get { return base.Size; }
			set { base.Size = value; }
		}
 
		// new property so we can set Browsable/EditorBrowsable
		[Browsable(false)]
		[DefaultValue(SizeGripStyle.Hide)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new SizeGripStyle SizeGripStyle {
			get { return base.SizeGripStyle; }
			set { base.SizeGripStyle = value; }
		}
 
		// new property so we can set Browsable/EditorBrowsable
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new FormStartPosition StartPosition {
			get { return base.StartPosition; }
			set { base.StartPosition = value; }
		}
 
		// new property so we can set Browsable/EditorBrowsable
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new bool TabStop {
			get { return base.TabStop; }
			set { base.TabStop = value; }
		}
 
		// new property so we can set Browsable/EditorBrowsable
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new object Tag {
			get { return base.Tag; }
			set { base.Tag = value; }
		}
 
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public override string Text {
			get {
				return base.Text;
			}

			set {
				base.Text = value;
			}
		}
 
		// new property so we can set Browsable/EditorBrowsable
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new bool TopMost {
			get { return base.TopMost; }
			set { base.TopMost = value; }
		}
 
		// new property so we can set Browsable/EditorBrowsable
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new Color TransparencyKey {
			get { return base.TransparencyKey; }
			set { base.TransparencyKey = value; }
		}
 
		[DefaultValue(false)]
		public bool UseAntiAlias {
			get {
				return print_preview.UseAntiAlias;
			}

			set {
				print_preview.UseAntiAlias = value;
			}
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new bool UseWaitCursor {
			get { return base.UseWaitCursor; }
			set { base.UseWaitCursor = value; }
		}

		// new property so we can set Browsable/EditorBrowsable
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new bool Visible {
			get { return base.Visible; }
			set { base.Visible = value; }
		}
 
		// new property so we can set Browsable/EditorBrowsable
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new FormWindowState WindowState {
			get { return base.WindowState; }
			set { base.WindowState = value; }
		}

		[MonoInternalNote ("Throw InvalidPrinterException")]
		protected override void CreateHandle() {

//			if (this.Document != null && !this.Document.PrinterSettings.IsValid) {
//				throw new InvalidPrinterException(this.Document.PrinterSettings);
//			}
			base.CreateHandle ();
		}

		protected override void OnClosing(CancelEventArgs e) {
			print_preview.InvalidatePreview ();
			base.OnClosing (e);
		}

		protected override bool ProcessDialogKey (Keys keyData)
		{
			switch (keyData) {
				case Keys.Up:
				case Keys.Down:
				case Keys.Right:
				case Keys.Left:
					return false;
			}
			
			return base.ProcessDialogKey (keyData);
		}

		protected override bool ProcessTabKey (bool forward)
		{
			return base.ProcessTabKey (forward);
		}
		
		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event EventHandler AutoSizeChanged {
			add { base.AutoSizeChanged += value; }
			remove { base.AutoSizeChanged -= value; }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event EventHandler AutoValidateChanged {
			add { base.AutoValidateChanged += value; }
			remove { base.AutoValidateChanged -= value; }
		}

		// new event so we can set Browsable/EditorBrowsable
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new event EventHandler BackColorChanged {
			add { base.BackColorChanged += value; }
			remove { base.BackColorChanged -= value; }
		}
 
		// new event so we can set Browsable/EditorBrowsable
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new event EventHandler BackgroundImageChanged {
			add { base.BackgroundImageChanged += value; }
			remove { base.BackgroundImageChanged -= value; }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event EventHandler BackgroundImageLayoutChanged {
			add { base.BackgroundImageLayoutChanged += value; }
			remove { base.BackgroundImageLayoutChanged -= value; }
		}

		// new event so we can set Browsable/EditorBrowsable
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new event EventHandler CausesValidationChanged {
			add { base.CausesValidationChanged += value; }
			remove { base.CausesValidationChanged -= value; }
		}
 
		// new event so we can set Browsable/EditorBrowsable
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new event EventHandler ContextMenuChanged {
			add { base.ContextMenuChanged += value; }
			remove { base.ContextMenuChanged -= value; }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event EventHandler ContextMenuStripChanged {
			add { base.ContextMenuStripChanged += value; }
			remove { base.ContextMenuStripChanged -= value; }
		}

		// new event so we can set Browsable/EditorBrowsable
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new event EventHandler CursorChanged {
			add { base.CursorChanged += value; }
			remove { base.CursorChanged -= value; }
		}
 
		// new event so we can set Browsable/EditorBrowsable
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new event EventHandler DockChanged {
			add { base.DockChanged += value; }
			remove { base.DockChanged -= value; }
		}
 
		// new event so we can set Browsable/EditorBrowsable
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new event EventHandler EnabledChanged {
			add { base.EnabledChanged += value; }
			remove { base.EnabledChanged -= value; }
		}
 
		// new event so we can set Browsable/EditorBrowsable
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new event EventHandler FontChanged {
			add { base.FontChanged += value; }
			remove { base.FontChanged -= value; }
		}
 
		// new event so we can set Browsable/EditorBrowsable
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new event EventHandler ForeColorChanged {
			add { base.ForeColorChanged += value; }
			remove { base.ForeColorChanged -= value; }
		}
 
		// new event so we can set Browsable/EditorBrowsable
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new event EventHandler ImeModeChanged {
			add { base.ImeModeChanged += value; }
			remove { base.ImeModeChanged -= value; }
		}
 
		// new event so we can set Browsable/EditorBrowsable
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new event EventHandler LocationChanged {
			add { base.LocationChanged += value; }
			remove { base.LocationChanged -= value; }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event EventHandler MarginChanged {
			add { base.MarginChanged += value; }
			remove { base.MarginChanged -= value; }
		}

		// new event so we can set Browsable/EditorBrowsable
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new event EventHandler MaximumSizeChanged {
			add { base.MaximumSizeChanged += value; }
			remove { base.MaximumSizeChanged -= value; }
		}
 
		// new event so we can set Browsable/EditorBrowsable
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new event EventHandler MinimumSizeChanged {
			add { base.MinimumSizeChanged += value; }
			remove { base.MinimumSizeChanged -= value; }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event EventHandler PaddingChanged {
			add { base.PaddingChanged += value; }
			remove { base.PaddingChanged -= value; }
		}

		// new event so we can set Browsable/EditorBrowsable
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new event EventHandler RightToLeftChanged {
			add { base.RightToLeftChanged += value; }
			remove { base.RightToLeftChanged -= value; }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event EventHandler RightToLeftLayoutChanged {
			add { base.RightToLeftLayoutChanged += value; }
			remove { base.RightToLeftLayoutChanged -= value; }
		}

		// new event so we can set Browsable/EditorBrowsable
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new event EventHandler SizeChanged {
			add { base.SizeChanged += value; }
			remove { base.SizeChanged -= value; }
		}
 
		// new event so we can set Browsable/EditorBrowsable
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new event EventHandler TabStopChanged {
			add { base.TabStopChanged += value; }
			remove { base.TabStopChanged -= value; }
		}
 
		// new event so we can set Browsable/EditorBrowsable
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new event EventHandler TextChanged {
			add { base.TextChanged += value; }
			remove { base.TextChanged -= value; }
		}
 
		// new event so we can set Browsable/EditorBrowsable
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new event EventHandler VisibleChanged {
			add { base.VisibleChanged += value; }
			remove { base.VisibleChanged -= value; }
		}
	}
}
