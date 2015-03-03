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
// Copyright (c) 2005 Novell, Inc. (http://www.novell.com)
//
// Authors:
//	Peter Bartok	(pbartok@novell.com)
//
//

// NOT COMPLETE
// Still missing: Tie-in to HTML help when the user presses F1 on the control

using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;

namespace System.Windows.Forms {
	[ToolboxItemFilter("System.Windows.Forms")]
	[ProvideProperty("ShowHelp", "System.Windows.Forms.Control, " + Consts.AssemblySystem_Windows_Forms)]
	[ProvideProperty("HelpNavigator", "System.Windows.Forms.Control, " + Consts.AssemblySystem_Windows_Forms)]
	[ProvideProperty("HelpKeyword", "System.Windows.Forms.Control, " + Consts.AssemblySystem_Windows_Forms)]
	[ProvideProperty("HelpString", "System.Windows.Forms.Control, " + Consts.AssemblySystem_Windows_Forms)]
	public class HelpProvider : Component, IExtenderProvider {
		#region HelpProperty Class
		private class HelpProperty {
			internal string		keyword;
			internal HelpNavigator	navigator;
			internal string		text;
			internal bool		show;
			internal Control	control;
			internal HelpProvider	hp;

			public HelpProperty(HelpProvider hp, Control control) {
				this.control = control;
				this.hp = hp;

				keyword = null;
				navigator = HelpNavigator.AssociateIndex;
				text = null;
				show = false;

				control.HelpRequested += hp.HelpRequestHandler; 
			}

			public string Keyword {
				get { return keyword; }
				set { keyword = value; }
			}

			public HelpNavigator Navigator {
				get { return navigator; }
				set { navigator = value; }
			}

			public string Text {
				get { return text; }
				set { text = value; }
			}

			public bool Show {
				get { return show; }
				set { show = value; }
			}
		}
		#endregion	// HelpProperty Class

		#region Local Variables
		private string			helpnamespace;
		private Hashtable		controls;
		private ToolTip.ToolTipWindow	tooltip;
		private EventHandler		HideToolTipHandler;
		private KeyPressEventHandler	HideToolTipKeyHandler;
		private MouseEventHandler	HideToolTipMouseHandler;
		private HelpEventHandler	HelpRequestHandler;
		private object tag;
		#endregion	// Local Variables

		#region Public Constructors
		public HelpProvider() {
			controls = new Hashtable();
			tooltip = new ToolTip.ToolTipWindow();

			//UIA Framework: Event used to indicate that ToolTip is shown
			tooltip.VisibleChanged += delegate (object sender, EventArgs args) {
				if (tooltip.Visible == true)
					OnUIAHelpRequested (this, new ControlEventArgs (UIAControl));
				else 
					OnUIAHelpUnRequested (this, new ControlEventArgs (UIAControl));
			};

			HideToolTipHandler = new EventHandler(HideToolTip);
			HideToolTipKeyHandler = new KeyPressEventHandler(HideToolTipKey);
			HideToolTipMouseHandler = new MouseEventHandler(HideToolTipMouse);
			HelpRequestHandler = new HelpEventHandler(HelpRequested);
		}
		#endregion	// Public Constructors

		#region Public Instance Properties
		[DefaultValue(null)]
		[Editor ("System.Windows.Forms.Design.HelpNamespaceEditor, " + Consts.AssemblySystem_Design, "System.Drawing.Design.UITypeEditor, " + Consts.AssemblySystem_Drawing)]
		[Localizable(true)]
		public virtual string HelpNamespace {
			get {
				return helpnamespace;
			}

			set {
				helpnamespace = value;
			}
		}
		
		[Localizable (false)]
		[Bindable (true)]
		[TypeConverter (typeof (StringConverter))]
		[DefaultValue (null)]
		[MWFCategory ("Data")]
		public object Tag
		{
			get { return this.tag; }
			set { this.tag = value; }
		}
		#endregion	// Public Instance Properties

		#region Public Instance Methods
		public virtual bool CanExtend(object target) {
			if (!(target is Control)) {
				return false;
			}

			if ((target is Form) || (target is ToolBar)) {
				return false;
			}

			return true;
		}

		[DefaultValue(null)]
		[Localizable(true)]
		public virtual string GetHelpKeyword(Control ctl) {
			return GetHelpProperty(ctl).Keyword;
		}

		[DefaultValue(HelpNavigator.AssociateIndex)]
		[Localizable(true)]
		public virtual HelpNavigator GetHelpNavigator(Control ctl) {
			return GetHelpProperty(ctl).Navigator;
		}

		[DefaultValue(null)]
		[Localizable(true)]
		public virtual string GetHelpString(Control ctl) {
			return GetHelpProperty(ctl).Text;
		}

		[Localizable(true)]
		public virtual bool GetShowHelp(Control ctl) {
			return GetHelpProperty(ctl).Show;
		}

		public virtual void ResetShowHelp(Control ctl) {
			HelpProperty	hp;

			hp = GetHelpProperty(ctl);
			
			if ((hp.Keyword != null) || (hp.Text != null)) {
				hp.Show = true;
			} else {
				hp.Show = false;
			}
		}

		public virtual void SetHelpKeyword(Control ctl, string keyword) {
			GetHelpProperty(ctl).Keyword = keyword;
		}

		public virtual void SetHelpNavigator(Control ctl, HelpNavigator navigator) {
			GetHelpProperty(ctl).Navigator = navigator;
		}

		public virtual void SetHelpString(Control ctl, string helpString) {
			GetHelpProperty(ctl).Text = helpString;
		}

		public virtual void SetShowHelp(Control ctl, bool value) {
			GetHelpProperty(ctl).Show = value;
		}

		public override string ToString() {
			return base.ToString() + ", HelpNameSpace: " + helpnamespace;
		}

		#endregion	// Public Instance Methods

		#region Private Methods
		private HelpProperty GetHelpProperty(Control control) {
			HelpProperty hp;

			hp = (HelpProperty)controls[control];
			if (hp == null) {
				hp = new HelpProperty(this, control);
				controls[control] = hp;
			}

			return hp;
		}

		private void HideToolTip(object Sender, EventArgs e) {
			Control control;

			control = (Control)Sender;
			control.LostFocus -= HideToolTipHandler;

			this.tooltip.Visible = false;
		}

		private void HideToolTipKey(object Sender, KeyPressEventArgs e) {
			Control control;

			control = (Control)Sender;
			control.KeyPress -= HideToolTipKeyHandler;

			this.tooltip.Visible = false;
		}

		private void HideToolTipMouse(object Sender, MouseEventArgs e) {
			Control control;

			control = (Control)Sender;
			control.MouseDown -= HideToolTipMouseHandler;

			this.tooltip.Visible = false;
		}


		// This is called when the user does a "what's this" style lookup. It uses the 'text' property
		private void HelpRequested(object sender, HelpEventArgs e) {
			Size	size;
			Point	pt;
			Control	control;

			control = (Control)sender;

			//UIA Framework: Associates requested control with internal variable to generate event
			UIAControl = control;

			if (GetHelpProperty(control).Text == null) {
				return;
			}

			pt = e.MousePos;

			// Display Tip
			tooltip.Text = GetHelpProperty(control).Text;
			size = ThemeEngine.Current.ToolTipSize(tooltip, tooltip.Text);
			tooltip.Width = size.Width;
			tooltip.Height = size.Height;
			pt.X -= size.Width / 2;

			if (pt.X < 0) {
				pt.X += size.Width / 2;
			}

			if ((pt.X + size.Width) < SystemInformation.WorkingArea.Width) {
				tooltip.Left = pt.X;
			} else {
				tooltip.Left = pt.X - size.Width;
			}

			if ((pt.Y + size.Height) < (SystemInformation.WorkingArea.Height - 16)) {
				tooltip.Top = pt.Y;
			} else {
				tooltip.Top = pt.Y - size.Height;
			}

				
			tooltip.Visible = true;
			control.KeyPress += HideToolTipKeyHandler;
			control.MouseDown += HideToolTipMouseHandler;
			control.LostFocus += HideToolTipHandler;
			e.Handled = true;
		}
		#endregion	// Private Methods

		#region UIA Framework: Events, Delegates and Methods
		private Control uia_control;

		private Control UIAControl {
			get { return uia_control; }
			set { uia_control = value; }
		}

		internal static event ControlEventHandler UIAHelpRequested;
		internal static event ControlEventHandler UIAHelpUnRequested;

		internal Rectangle UIAToolTipRectangle {
			get { return tooltip.Bounds; }
		}

		internal static void OnUIAHelpRequested (HelpProvider provider, ControlEventArgs args)
		{
			if (UIAHelpRequested != null)
				UIAHelpRequested (provider, args);
		}

		internal static void OnUIAHelpUnRequested (HelpProvider provider, ControlEventArgs args)
		{
			if (UIAHelpUnRequested != null)
				UIAHelpUnRequested (provider, args);
		}
		#endregion
	}
}
