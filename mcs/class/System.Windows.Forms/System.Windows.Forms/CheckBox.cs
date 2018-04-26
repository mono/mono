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
// Copyright (c) 2004-2005 Novell, Inc.
//
// Authors:
//	Dennis Hayes	dennish@raytek.com
//	Peter Bartok	pbartok@novell.com
//

using System;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;

namespace System.Windows.Forms {
	[DefaultProperty("Checked")]
	[DefaultEvent("CheckedChanged")]
	[ComVisible (true)]
	[ClassInterface (ClassInterfaceType.AutoDispatch)]
	[DefaultBindingProperty ("CheckState")]
	[ToolboxItem ("System.Windows.Forms.Design.AutoSizeToolboxItem," + Consts.AssemblySystem_Design)]
	public class CheckBox : ButtonBase {
		#region Local Variables
		internal Appearance		appearance;
		internal bool			auto_check;
		internal ContentAlignment	check_alignment;
		internal CheckState		check_state;
		internal bool			three_state;
		#endregion	// Local Variables

		#region CheckBoxAccessibleObject Subclass
		[ComVisible(true)]
		public class CheckBoxAccessibleObject : ButtonBaseAccessibleObject {
			#region CheckBoxAccessibleObject Local Variables
			private new CheckBox owner;
			#endregion	// CheckBoxAccessibleObject Local Variables

			#region CheckBoxAccessibleObject Constructors
			public CheckBoxAccessibleObject(Control owner) : base(owner) {
				this.owner = (CheckBox)owner;
			}
			#endregion	// CheckBoxAccessibleObject Constructors

			#region CheckBoxAccessibleObject Properties
			public override string DefaultAction {
				get {
					return "Select";
				}
			}

			public override AccessibleRole Role {
				get {
					return AccessibleRole.CheckButton;
				}
			}

			public override AccessibleStates State {
				get {
					AccessibleStates	retval;

					retval = AccessibleStates.Default;

					if (owner.check_state == CheckState.Checked) {
						retval |= AccessibleStates.Checked;
					}

					if (owner.Focused) {
						retval |= AccessibleStates.Focused;
					}

					if (owner.CanFocus) {
						retval |= AccessibleStates.Focusable;
					}

					return retval;
				}
			}
			#endregion	// CheckBoxAccessibleObject Properties

			#region CheckBoxAccessibleObject Methods
			public override void DoDefaultAction ()
			{
				owner.Checked = !owner.Checked;
			}
			#endregion	// CheckBoxAccessibleObject Methods
		}
		#endregion	// CheckBoxAccessibleObject Sub-class

		#region Public Constructors
		public CheckBox() {
			appearance = Appearance.Normal;
			auto_check = true;
			check_alignment = ContentAlignment.MiddleLeft;
			TextAlign = ContentAlignment.MiddleLeft;
			SetStyle(ControlStyles.StandardDoubleClick, false);
			SetAutoSizeMode (AutoSizeMode.GrowAndShrink);
			can_cache_preferred_size = true;
		}
		#endregion	// Public Constructors

		#region	Internal Methods
		internal override void Draw (PaintEventArgs pe) {
			// FIXME: This should be called every time something that can affect it
			// is changed, not every paint.  Can only change so many things at a time.

			// Figure out where our text and image should go
			Rectangle glyph_rectangle;
			Rectangle text_rectangle;
			Rectangle image_rectangle;

			ThemeEngine.Current.CalculateCheckBoxTextAndImageLayout (this, Point.Empty, out glyph_rectangle, out text_rectangle, out image_rectangle);

			// Draw our button
			if (FlatStyle != FlatStyle.System)
				ThemeEngine.Current.DrawCheckBox (pe.Graphics, this, glyph_rectangle, text_rectangle, image_rectangle, pe.ClipRectangle);
			else
				ThemeEngine.Current.DrawCheckBox (pe.Graphics, this.ClientRectangle, this);
		}

		internal override Size GetPreferredSizeCore (Size proposedSize)
		{
			if (this.AutoSize)
				return ThemeEngine.Current.CalculateCheckBoxAutoSize (this);

			return base.GetPreferredSizeCore (proposedSize);
		}

		internal override void HaveDoubleClick() {
			if (DoubleClick != null) DoubleClick(this, EventArgs.Empty);
		}
		#endregion	// Internal Methods

		#region Public Instance Properties
		[DefaultValue(Appearance.Normal)]
		[Localizable(true)]
		public Appearance Appearance {
			get {
				return appearance;
			}

			set {
				if (value != appearance) {
					appearance = value;
					OnAppearanceChanged (EventArgs.Empty);

					if (Parent != null)
						Parent.PerformLayout (this, "Appearance");
					Invalidate();
				}
			}
		}

		[DefaultValue(true)]
		public bool AutoCheck {
			get {
				return auto_check;
			}

			set {
				auto_check = value;
			}
		}

		[Bindable(true)]
		[Localizable(true)]
		[DefaultValue(ContentAlignment.MiddleLeft)]
		public ContentAlignment CheckAlign {
			get {
				return check_alignment;
			}

			set {
				if (value != check_alignment) {
					check_alignment = value;
					if (Parent != null)
						Parent.PerformLayout (this, "CheckAlign");
					Invalidate();
				}
			}
		}

		[Bindable(true)]
		[RefreshProperties(RefreshProperties.All)]
		[DefaultValue(false)]
		[SettingsBindable (true)]
		public bool Checked {
			get {
				if (check_state != CheckState.Unchecked) {
					return true;
				}
				return false;
			}

			set {
				if (value && (check_state != CheckState.Checked)) {
					check_state = CheckState.Checked;
					Invalidate();
					OnCheckedChanged(EventArgs.Empty);
				} else if (!value && (check_state != CheckState.Unchecked)) {
					check_state = CheckState.Unchecked;
					Invalidate();
					OnCheckedChanged(EventArgs.Empty);
				}
			}
		}

		[DefaultValue(CheckState.Unchecked)]
		[RefreshProperties(RefreshProperties.All)]
		[Bindable(true)]
		public CheckState CheckState {
			get {
				return check_state;
			}

			set {
				if (value != check_state) {
					bool	was_checked = (check_state != CheckState.Unchecked);

					check_state = value;

					if (was_checked != (check_state != CheckState.Unchecked)) {
						OnCheckedChanged(EventArgs.Empty);
					}

					OnCheckStateChanged(EventArgs.Empty);
					Invalidate();
				}
			}
		}

		[DefaultValue(ContentAlignment.MiddleLeft)]
		[Localizable(true)]
		public override ContentAlignment TextAlign {
			get { return base.TextAlign; }
			set { base.TextAlign = value; }
		}


		[DefaultValue(false)]
		public bool ThreeState {
			get {
				return three_state;
			}

			set {
				three_state = value;
			}
		}
		#endregion	// Public Instance Properties

		#region Protected Instance Properties
		protected override CreateParams CreateParams {
			get {
				return base.CreateParams;
			}
		}

		protected override Size DefaultSize {
			get {
				return new Size(104, 24);
			}
		}
		#endregion	// Protected Instance Properties

		#region Public Instance Methods
		public override string ToString() {
			return base.ToString() + ", CheckState: " + (int)check_state;
		}
		#endregion	// Public Instance Methods

		#region Protected Instance Methods
		protected override AccessibleObject CreateAccessibilityInstance() {
			AccessibleObject	ao;

			ao = base.CreateAccessibilityInstance ();
			ao.role = AccessibleRole.CheckButton;

			return ao;
		}

		protected virtual void OnAppearanceChanged(EventArgs e) {
			EventHandler eh = (EventHandler)(Events [AppearanceChangedEvent]);
			if (eh != null)
				eh (this, e);
		}

		protected virtual void OnCheckedChanged(EventArgs e) {
			EventHandler eh = (EventHandler)(Events [CheckedChangedEvent]);
			if (eh != null)
				eh (this, e);
		}

		protected virtual void OnCheckStateChanged(EventArgs e) {
			EventHandler eh = (EventHandler)(Events [CheckStateChangedEvent]);
			if (eh != null)
				eh (this, e);
		}

		protected override void OnClick(EventArgs e) {
			if (auto_check) {
				switch(check_state) {
					case CheckState.Unchecked: {
						if (three_state) {
							CheckState = CheckState.Indeterminate;
						} else {
							CheckState = CheckState.Checked;
						}
						break;
					}

					case CheckState.Indeterminate: {
						CheckState = CheckState.Checked;
						break;
					}

					case CheckState.Checked: {
						CheckState = CheckState.Unchecked;
						break;
					}
				}
			}
			
			base.OnClick (e);
		}

		protected override void OnHandleCreated(EventArgs e) {
			base.OnHandleCreated (e);
		}

		protected override void OnKeyDown (KeyEventArgs e)
		{
			base.OnKeyDown (e);
		}

		protected override void OnMouseUp(MouseEventArgs mevent) {
			base.OnMouseUp (mevent);
		}

		protected override bool ProcessMnemonic(char charCode) {
			if (IsMnemonic(charCode, Text) == true) {
				Select();
				OnClick(EventArgs.Empty);
				return true;
			}
			
			return base.ProcessMnemonic(charCode);
		}
		#endregion	// Protected Instance Methods

		#region Events
		static object AppearanceChangedEvent = new object ();
		static object CheckedChangedEvent = new object ();
		static object CheckStateChangedEvent = new object ();

		public event EventHandler AppearanceChanged {
			add { Events.AddHandler (AppearanceChangedEvent, value); }
			remove { Events.RemoveHandler (AppearanceChangedEvent, value); }
		}

		public event EventHandler CheckedChanged {
			add { Events.AddHandler (CheckedChangedEvent, value); }
			remove { Events.RemoveHandler (CheckedChangedEvent, value); }
		}

		public event EventHandler CheckStateChanged {
			add { Events.AddHandler (CheckStateChangedEvent, value); }
			remove { Events.RemoveHandler (CheckStateChangedEvent, value); }
		}
		
		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event MouseEventHandler MouseDoubleClick {
			add { base.MouseDoubleClick += value; }
			remove { base.MouseDoubleClick -= value; }
		}
		#endregion	// Events

		#region Events
		// XXX have a look at this and determine if it
		// manipulates base.DoubleClick, and see if
		// HaveDoubleClick can just call OnDoubleClick.
		[Browsable(false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event EventHandler DoubleClick;
		#endregion	// Events
	}
}
