//
// System.Windows.Forms.StatusBarPanel
//
// Author:
//   stubbed out by Richard Baumann (biochem333@nyc.rr.com)
//   Dennis Hayes (dennish@Raytek.com)
//
// (C) Ximian, Inc., 2002
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

		//
		//  --- Constructors/Destructors
		//
		StatusBarPanel() : base()
		{
			alignment = HorizontalAlignment.Left;
			autoSize = StatusBarPanelAutoSize.None;
			borderStyle = StatusBarPanelBorderStyle.Sunken;
			icon = null;
			minWidth = 10;
			style = StatusBarPanelStyle.Text;
			text = "";
			toolTipText = "";
			width = 100;
		}

		//
		//  --- Public Methods
		//
		[MonoTODO]
		public void BeginInit()
		{
			//FIXME:
		}
		[MonoTODO]
		public void EndInit()
		{
			//FIXME:
		}
		public override string ToString()
		{
			return text;
		}
		//
		//  --- Public Properties
		//
		public HorizontalAlignment Alignment {

			get { return alignment; }
			set { alignment = value; }
		}
		public StatusBarPanelAutoSize AutoSize {

			get { return autoSize; }
			set
			{
				if  (value != StatusBarPanelAutoSize.None && value != StatusBarPanelAutoSize.Contents && value != StatusBarPanelAutoSize.Spring) {

					throw new InvalidEnumArgumentException(
						"System.Windows.Forms.StatusBarPanel::set_AutoSize(StatusBarPanelAutoSize) " +
						"value is not a valid StatusBarPanelAutoSize value");
				}
				autoSize = value;
			}
		}
		public StatusBarPanelBorderStyle BorderStyle {

			get { return borderStyle; }
			set { borderStyle = value; }
		}
		public Icon Icon {

			get { return icon; }
			set { icon = value; }
		}
		public int MinWidth {

			get { return minWidth; }
			set { minWidth = value; }
		}
		public StatusBar Parent {

			get { return parent; }
			set { parent = value; }
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

			get { return toolTipText; }
			set { toolTipText = value; }
		}
		public int Width {

			get { return width; }
			set { width = value; }
		}
	}
}
