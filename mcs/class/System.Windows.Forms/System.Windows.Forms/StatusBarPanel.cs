//
// System.Windows.Forms.StatusBarPanel
//
// Author:
//   stubbed out by Richard Baumann (biochem333@nyc.rr.com)
//   Dennis Hayes (dennish@Raytek.com)
//   Aleksey Ryabchuk (ryabchuk@yahoo.com)
//
// (C) Ximian, Inc., 2002/3
//

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

using System;
using System.ComponentModel;
using System.Drawing;
namespace System.Windows.Forms {

	/// <summary>
	///	Represents a panel in a StatusBar control.
	/// </summary>
	public class StatusBarPanel : Component, ISupportInitialize {

		//
		//  --- Private Fields
		//
		private HorizontalAlignment alignment;
		private StatusBarPanelAutoSize autoSize;
		private StatusBarPanelBorderStyle borderStyle;
		private Icon icon;
		private int minWidth;
		private StatusBar parent;
		private StatusBarPanelStyle style;
		private string text;
		private string toolTipText;
		private int width;
		private bool suppressUpdates;

		//
		//  --- Constructors/Destructors
		//
		public StatusBarPanel() : base()
		{
			alignment = HorizontalAlignment.Left;
			autoSize = StatusBarPanelAutoSize.None;
			borderStyle = StatusBarPanelBorderStyle.Sunken;
			minWidth = 10;
			style = StatusBarPanelStyle.Text;
			width = 100;
			suppressUpdates = false;
		}

		//
		//  --- Public Methods
		//
		public void BeginInit()
		{
			suppressUpdates = true;
		}

		public void EndInit()
		{
			suppressUpdates = false;
			UpdateParent( true, true, null );
		}

		public override string ToString()
		{
			return "StatusBarPanel: {" + Text + "}";
		}
		//
		//  --- Public Properties
		//
		public HorizontalAlignment Alignment {
			get { return alignment; }
			set {
				if ( !Enum.IsDefined ( typeof(HorizontalAlignment), value ) )
					throw new InvalidEnumArgumentException( "Alignment",
						(int)value,
						typeof(HorizontalAlignment));

				alignment = value; 
				UpdateParent ( false, true, this );
			}
		}

		public StatusBarPanelAutoSize AutoSize {
			get { return autoSize; }
			set
			{
				if ( !Enum.IsDefined ( typeof(StatusBarPanelAutoSize), value ) )
					throw new InvalidEnumArgumentException( "AutoSize",
										(int)value,
										typeof(StatusBarPanelAutoSize));
				autoSize = value;
				UpdateParent ( true, false, null );
			}
		}

		public StatusBarPanelBorderStyle BorderStyle {
			get { return borderStyle; }
			set { 
				if ( !Enum.IsDefined ( typeof(StatusBarPanelBorderStyle), value ) )
					throw new InvalidEnumArgumentException( "BorderStyle",
						(int)value,
						typeof(StatusBarPanelBorderStyle));

				borderStyle = value;
				UpdateParent ( false, true, this );
			}
		}

		public Icon Icon {
			get { return icon; }
			set { 
				icon = value; 
				UpdateParent (  true, false, null );
			}
		}

		public int MinWidth 
		{
			get { return minWidth; }
			set { 
				if ( value < 0 )
					throw new ArgumentException(
					string.Format("'{0}' is not a valid value for 'value'. 'value' must be greater than or equal to 0.",
							value ) ) ;
				minWidth = value;
				UpdateParent ( true, false, null );
			}
		}

		public StatusBar Parent {
			get { return parent; }
		}

		public StatusBarPanelStyle Style {
			get { return style; }
			set { 
				if ( !Enum.IsDefined ( typeof(StatusBarPanelStyle), value ) )
					throw new InvalidEnumArgumentException( "Style",
						(int)value,
						typeof(StatusBarPanelStyle));
				style = value; 
				UpdateParent ( false, true, this );
			}
		}

		public string Text {
			get { return text; }
			set { 
				text = value;
				UpdateParent ( AutoSize == StatusBarPanelAutoSize.Contents, true, this );
			}
		}

		public string ToolTipText 
		{
			get { return toolTipText; }
			set { 
				toolTipText = value;
				UpdateTooltips ( this );
			}
		}

		public int Width {
			get { return width; }
			set { 
				// According to MS documentation this method
				// should throw ArgumentException if value < MinWidth,
				// but it does not actually happens.
				if ( value < MinWidth )
					width = MinWidth; 
				else
					width = value;
				UpdateParent ( true, false, null );
			}
		}

		public int GetContentWidth ( ) {
			if( Parent != null) {
				int cxsize = 0;
				if ( Text != null )
					cxsize = Win32.GetTextExtent( Parent.Handle, Text ).cx;
				return cxsize < MinWidth ? MinWidth : cxsize;
			}
			return Width;
		}

		[MonoTODO]
		protected override void Dispose(bool disposing) {
			// FIXME:
			base.Dispose(disposing);
		}

		private void UpdateParent (  bool UpdateParts, bool UpdateText, StatusBarPanel p ) {
			if ( Parent != null && suppressUpdates != true)
				Parent.UpdatePanels ( UpdateParts, UpdateText, p );
		}

		private void UpdateTooltips ( StatusBarPanel p ) {
			if ( Parent != null && suppressUpdates != true)
				Parent.UpdateToolTips ( p );
		}

		internal void SetParent ( StatusBar prnt ) {
			parent = prnt;
		}
	}
}
