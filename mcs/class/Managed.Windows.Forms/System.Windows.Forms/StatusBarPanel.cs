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
//	Jackson Harper (jackson@ximian.com)


using System;
using System.Drawing;
using System.ComponentModel;

namespace System.Windows.Forms {

	public class StatusBarPanel : Component, ISupportInitialize {

		private StatusBar parent;

		private string text = String.Empty;
		private string tool_tip_text = String.Empty;

		private Icon icon;
		private HorizontalAlignment alignment = HorizontalAlignment.Left;
		private StatusBarPanelAutoSize auto_size = StatusBarPanelAutoSize.None;
		private StatusBarPanelBorderStyle border_style = StatusBarPanelBorderStyle.Sunken;
		private StatusBarPanelStyle style;
		private int width = 100;
		private int min_width = 10;
		
		public StatusBarPanel ()
		{
		}

		public HorizontalAlignment Alignment {
			get { return alignment; }
			set { alignment = value; }
		}

		public StatusBarPanelAutoSize AutoSize {
			get { return auto_size; }
			set { auto_size = value; }
		}

		public StatusBarPanelBorderStyle BorderStyle {
			get { return border_style; }
			set { border_style = value; }
		}

		public Icon Icon {
			get { return icon; }
			set { icon = value; }
		}

		public int MinWidth {
			get {
				if (AutoSize == StatusBarPanelAutoSize.None)
					return Width;
				return min_width;
			}
			set {
				if (value < 0)
					throw new ArgumentException ("value");
				min_width = value;
			}
		}
		
		public int Width {
			get { return width; }
			set {
				if (value < 0)
					throw new ArgumentException ("value");
				width = value;
			}
		}
		
		public StatusBarPanelStyle Style {
			get { return style; }
			set { style = value; }
		}

		public string Text {
			get { return text; }
			set { text = value; }
		}

		public string ToolTipText {
			get { return tool_tip_text; }
			set { tool_tip_text = value; }
		}

		public StatusBar Parent {
			get { return parent; }
		}

		internal void SetParent (StatusBar parent)
		{
			this.parent = parent;
		}

		public override string ToString ()
		{
			return "StatusBarPanel: {" + Text +"}";
		}

		[MonoTODO]
		protected override void Dispose (bool disposing)
		{
		}

		[MonoTODO]
		public virtual void BeginInit()
		{
		}

		[MonoTODO]
		public virtual void EndInit()
		{
		}
	}
}


