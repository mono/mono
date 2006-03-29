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

namespace System.Windows.Forms {
	[DefaultProperty("Document")]
	[Designer("System.ComponentModel.Design.ComponentDesigner, System.Design, Version=1.0.5000.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
	[DesignTimeVisible(true)]
	[ToolboxItem(true)]
	public class PrintPreviewDialog : Form {
		#region Local variables
		PrintPreviewControl print_preview;
		#endregion // Local variables

		#region Public Constructors
		[MonoTODO("Finish setting up a proper dialog to actually make this thing work")]
		public PrintPreviewDialog() {
			print_preview = new PrintPreviewControl();
		}
		#endregion // Public Constructors

		#region Public Instance Properties
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public IButtonControl AcceptButton {
			get {
				return base.AcceptButton;
			}

			set {
				base.AcceptButton = value;
			}
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public string AccessibleDescription {
			get {
				return base.AccessibleDescription;
			}

			set {
				base.AccessibleDescription = value;
			}
		}
 
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public string AccessibleName {
			get {
				return base.AccessibleName;
			}

			set {
				base.AccessibleName = value;
			}
		}
 
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public AccessibleRole AccessibleRole {
			get {
				return base.AccessibleRole;
			}

			set {
				base.AccessibleRole = value;
			}
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
 
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool AutoScale {
			get {
				return base.AutoScale;
			}

			set {
				base.AutoScale = value;
			}
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
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
 
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public Size AutoScrollMargin {
			get {
				return base.AutoScrollMargin;
			}

			set {
				base.AutoScrollMargin = value;
			}
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public Size AutoScrollMinSize {
			get {
				return base.AutoScrollMinSize;
			}

			set {
				base.AutoScrollMinSize = value;
			}
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
 
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public IButtonControl CancelButton {
			get {
				return base.CancelButton;
			}

			set {
				base.CancelButton = value;
			}
		}
 
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool CausesValidation {
			get {
				return base.CausesValidation;
			}

			set {
				base.CausesValidation = value;
			}
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
 
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ControlBox {
			get {
				return base.ControlBox;
			}

			set {
				base.ControlBox = value;
			}
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
 
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public ControlBindingsCollection DataBindings {
			get {
				return base.DataBindings;
			}
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
 
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public DockPaddingEdges DockPadding {
			get {
				return base.DockPadding;
			}
		}
 
		
		[DefaultValue(null)]
		public PrintDocument Document {
			get {
				return print_preview.Document;
			}

			set {
				print_preview.Document = value;
			}
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool Enabled {
			get {
				return base.Enabled;
			}

			set {
				base.Enabled = value;
			}
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
 
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public FormBorderStyle FormBorderStyle {
			get {
				return base.FormBorderStyle;
			}

			set {
				base.FormBorderStyle = value;
			}
		}
 
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool HelpButton {
			get {
				return base.HelpButton;
			}

			set {
				base.HelpButton = value;
			}
		}
 
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public Icon Icon {
			get {
				return base.Icon;
			}

			set {
				base.Icon = value;
			}
		}
 
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public ImeMode ImeMode {
			get {
				return base.ImeMode;
			}

			set {
				base.ImeMode = value;
			}
		}
 
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool IsMdiContainer {
			get {
				return base.IsMdiContainer;
			}

			set {
				base.IsMdiContainer = value;
			}
		}
 
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool KeyPreview {
			get {
				return base.KeyPreview;
			}

			set {
				base.KeyPreview = value;
			}
		}
 
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public Point Location {
			get {
				return base.Location;
			}

			set {
				base.Location = value;
			}
		}
 
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool MaximizeBox {
			get {
				return base.MaximizeBox;
			}

			set {
				base.MaximizeBox = value;
			}
		}
 
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public Size MaximumSize {
			get {
				return base.MaximumSize;
			}

			set {
				base.MaximumSize = value;
			}
		}
 
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public MainMenu Menu {
			get {
				return base.Menu;
			}

			set {
				base.Menu = value;
			}
		}
 
		[Browsable(false)]
		[DefaultValue(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool MinimizeBox {
			get {
				return base.MinimizeBox;
			}

			set {
				base.MinimizeBox = value;
			}
		}
 
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public Size MinimumSize {
			get {
				return base.MinimumSize;
			}

			set {
				base.MinimumSize = value;
			}
		}
 
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		public double Opacity {
			get {
				return base.Opacity;
			}

			set {
				base.Opacity = value;
			}
		}
 
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public PrintPreviewControl PrintPreviewControl {
			get {
				return this.print_preview;
			}
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
 
		[Browsable(false)]
		[DefaultValue(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShowInTaskbar {
			get {
				return base.ShowInTaskbar;
			}

			set {
				base.ShowInTaskbar = value;
			}
		}
 
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public Size Size {
			get {
				return base.Size;
			}

			set {
				base.Size = value;
			}
		}
 
		[Browsable(false)]
		[DefaultValue(SizeGripStyle.Hide)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public SizeGripStyle SizeGripStyle {
			get {
				return base.SizeGripStyle;
			}

			set {
				base.SizeGripStyle = value;
			}
		}
 
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public FormStartPosition StartPosition {
			get {
				return base.StartPosition;
			}

			set {
				base.StartPosition = value;
			}
		}
 
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool TabStop {
			get {
				return base.TabStop;
			}

			set {
				base.TabStop = value;
			}
		}
 
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public object Tag {
			get {
				return base.Tag;
			}

			set {
				base.Tag = value;
			}
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
 
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool TopMost {
			get {
				return base.TopMost;
			}

			set {
				base.TopMost = value;
			}
		}
 
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public Color TransparencyKey {
			get {
				return base.TransparencyKey;
			}

			set {
				base.TransparencyKey = value;
			}
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

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool Visible {
			get {
				return base.Visible;
			}

			set {
				base.Visible = value;
			}
		}
 
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public FormWindowState WindowState {
			get {
				return base.WindowState;
			}

			set {
				base.WindowState = value;
			}
		}
 		#endregion // Public Instance Properties

		#region Protected Instance Properties
		protected override void CreateHandle() {
			base.CreateHandle ();
		}

		protected override void OnClosing(CancelEventArgs e) {
			base.OnClosing (e);
		}
		#endregion // Protected Instance Methods

		#region Events
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public event EventHandler BackColorChanged {
			add {
				base.BackColorChanged += value;
			}

			remove {
				base.BackColorChanged -= value;
			}
		}
 
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public event EventHandler BackgroundImageChanged {
			add {
				base.BackgroundImageChanged += value;
			}

			remove {
				base.BackgroundImageChanged -= value;
			}
		}
 
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public event EventHandler CausesValidationChanged {
			add {
				base.CausesValidationChanged += value;
			}

			remove {
				base.CausesValidationChanged -= value;
			}
		}
 
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public event EventHandler ContextMenuChanged {
			add {
				base.ContextMenuChanged += value;
			}

			remove {
				base.ContextMenuChanged -= value;
			}
		}
 
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public event EventHandler CursorChanged {
			add {
				base.CursorChanged += value;
			}

			remove {
				base.CursorChanged -= value;
			}
		}
 
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public event EventHandler DockChanged {
			add {
				base.DockChanged += value;
			}

			remove {
				base.DockChanged -= value;
			}
		}
 
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public event EventHandler EnabledChanged {
			add {
				base.EnabledChanged += value;
			}

			remove {
				base.EnabledChanged -= value;
			}
		}
 
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public event EventHandler FontChanged {
			add {
				base.FontChanged += value;
			}

			remove {
				base.FontChanged -= value;
			}
		}
 
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public event EventHandler ForeColorChanged {
			add {
				base.ForeColorChanged += value;
			}

			remove {
				base.ForeColorChanged -= value;
			}
		}
 
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public event EventHandler ImeModeChanged {
			add {
				base.ImeModeChanged += value;
			}

			remove {
				base.ImeModeChanged -= value;
			}
		}
 
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public event EventHandler LocationChanged {
			add {
				base.LocationChanged += value;
			}

			remove {
				base.LocationChanged -= value;
			}
		}
 
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public event EventHandler MaximumSizeChanged {
			add {
				base.MaximumSizeChanged += value;
			}

			remove {
				base.MaximumSizeChanged -= value;
			}
		}
 
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public event EventHandler MinimumSizeChanged {
			add {
				base.MinimumSizeChanged += value;
			}

			remove {
				base.MinimumSizeChanged -= value;
			}
		}
 
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public event EventHandler RightToLeftChanged {
			add {
				base.RightToLeftChanged += value;
			}

			remove {
				base.RightToLeftChanged -= value;
			}
		}
 
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public event EventHandler SizeChanged {
			add {
				base.SizeChanged += value;
			}

			remove {
				base.SizeChanged -= value;
			}
		}
 
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public event EventHandler TabStopChanged {
			add {
				base.TabStopChanged += value;
			}

			remove {
				base.TabStopChanged -= value;
			}
		}
 
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public event EventHandler TextChanged {
			add {
				base.TextChanged += value;
			}

			remove {
				base.TextChanged -= value;
			}
		}
 
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public event EventHandler VisibleChanged {
			add {
				base.VisibleChanged += value;
			}

			remove {
				base.VisibleChanged -= value;
			}
		}
		#endregion	// Events
	}
}
