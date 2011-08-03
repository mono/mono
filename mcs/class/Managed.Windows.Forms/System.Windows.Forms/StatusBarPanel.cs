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
//	Jackson Harper (jackson@ximian.com)

// COMPLETE

using System;
using System.Drawing;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Runtime.InteropServices;

namespace System.Windows.Forms {
	[ToolboxItem (false)]
	[DefaultProperty("Text")]
	[DesignTimeVisible(false)]
	public class StatusBarPanel : Component, ISupportInitialize {
		#region Local Variables
		private StatusBar parent;

		private bool initializing;
		private string text = String.Empty;
		private string tool_tip_text = String.Empty;

		private Icon icon;
		private HorizontalAlignment alignment = HorizontalAlignment.Left;
		private StatusBarPanelAutoSize auto_size = StatusBarPanelAutoSize.None;
		private StatusBarPanelBorderStyle border_style = StatusBarPanelBorderStyle.Sunken;
		private StatusBarPanelStyle style = StatusBarPanelStyle.Text;
		private int width = 100;
		private int min_width = 10;
		internal int X;
		
		private string name;
		private object tag;
		#endregion	// Local Variables

		#region UIA Framework Events
		static object UIATextChangedEvent = new object ();

		internal event EventHandler UIATextChanged {
			add { Events.AddHandler (UIATextChangedEvent, value); }
			remove { Events.RemoveHandler (UIATextChangedEvent, value); }
		}

		internal void OnUIATextChanged (EventArgs e)
		{
			EventHandler eh = (EventHandler) Events [UIATextChangedEvent];
			if (eh != null)
				eh (this, e);
		}
		#endregion

		#region Constructors
		public StatusBarPanel ()
		{
		}
		#endregion	// Constructors

		[DefaultValue(HorizontalAlignment.Left)]
		[Localizable(true)]
		public HorizontalAlignment Alignment {
			get { return alignment; }
			set { 
				alignment = value; 
				InvalidateContents ();
			}
		}

		[RefreshProperties (RefreshProperties.All)]
		[DefaultValue(StatusBarPanelAutoSize.None)]
		public StatusBarPanelAutoSize AutoSize {
			get { return auto_size; }
			set { 
				auto_size = value; 
				Invalidate ();
			}
		}

		[DefaultValue(StatusBarPanelBorderStyle.Sunken)]
		[DispId(-504)]
		public StatusBarPanelBorderStyle BorderStyle {
			get { return border_style; }
			set { 
				border_style = value; 
				Invalidate ();
			}
		}

		[DefaultValue(null)]
		[Localizable(true)]
		public Icon Icon {
			get { return icon; }
			set { 
				icon = value; 
				InvalidateContents ();
			}
		}

		[DefaultValue(10)]
		[Localizable(true)]
		[RefreshProperties(RefreshProperties.All)]
		public int MinWidth {
			get {
			/*
				MSDN says that when AutoSize = None then MinWidth is automatically
				set to Width, but neither v1.1 nor v2.0 behave that way.
			*/
				return min_width;
			}
			set {
				if (value < 0)
					throw new ArgumentOutOfRangeException ("value");

				min_width = value;
				if (min_width > width)
					width = min_width;
				
				Invalidate ();
			}
		}

		[Localizable (true)]
		public string Name {
			get {
				if (name == null)
					return string.Empty;
				return name;
			}
			set {
				name = value;
			}
		}
		
		[DefaultValue(100)]
		[Localizable(true)]
		public int Width {
			get { return width; }
			set {
				if (value < 0)
					throw new ArgumentException ("value");

				if (initializing)
					width = value;
				else
					SetWidth(value);
				
				Invalidate ();
			}
		}
		
		[DefaultValue(StatusBarPanelStyle.Text)]
		public StatusBarPanelStyle Style {
			get { return style; }
			set { 
				style = value; 
				Invalidate ();
			}
		}

		[TypeConverter (typeof (StringConverter))]
		[Localizable (false)]
		[Bindable (true)]
		[DefaultValue (null)]
		public object Tag {
			get {
				return tag;
			}
			set {
				tag = value;
			}
		}

		[DefaultValue("")]
		[Localizable(true)]
		public string Text {
			get { return text; }
			set { 
				text = value; 
				InvalidateContents ();

				// UIA Framework Event: Text Changed
				OnUIATextChanged (EventArgs.Empty);
			}
		}

		[DefaultValue("")]
		[Localizable(true)]
		public string ToolTipText {
			get { return tool_tip_text; }
			set { tool_tip_text = value; }
		}

		[Browsable(false)]
		public StatusBar Parent {
			get { return parent; }
		}

		private void Invalidate ()
		{
			if (parent == null)
				return;
			parent.UpdatePanel (this);
		}

		private void InvalidateContents ()
		{
			if (parent == null)
				return;
			parent.UpdatePanelContents (this);
		}

		internal void SetParent (StatusBar parent)
		{
			this.parent = parent;
		}

		internal void SetWidth (int width)
		{
			this.width = width;
			if (min_width > this.width)
				this.width = min_width;
		}

		public override string ToString ()
		{
			return "StatusBarPanel: {" + Text +"}";
		}

		protected override void Dispose (bool disposing)
		{
		}

		public void BeginInit ()
		{
			initializing = true;
		}

		public void EndInit ()
		{
			if (!initializing)
				return;
			
			if (min_width > width)
				width = min_width;
			
			initializing = false;
		}
	}
}


