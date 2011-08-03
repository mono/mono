//
// ProfessionalColors.cs
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of color_table software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and color_table permission notice shall be
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
//	Jonathan Pobst (monkey@jpobst.com)
//

using System.Drawing;
using System.Windows.Forms.VisualStyles;

namespace System.Windows.Forms
{
	public sealed class ProfessionalColors
	{
		private static ProfessionalColorTable color_table = new ProfessionalColorTable();

		#region Private Constructor
		private ProfessionalColors () {}
		#endregion
		
		#region Static Properties
		public static Color ButtonCheckedGradientBegin { get { return color_table.ButtonCheckedGradientBegin; } }
		public static Color ButtonCheckedGradientEnd { get { return color_table.ButtonCheckedGradientEnd; } }
		public static Color ButtonCheckedGradientMiddle { get { return color_table.ButtonCheckedGradientMiddle; } }
		public static Color ButtonCheckedHighlight { get { return color_table.ButtonCheckedHighlight; } }
		public static Color ButtonCheckedHighlightBorder { get { return color_table.ButtonCheckedHighlightBorder; } }
		public static Color ButtonPressedBorder { get { return color_table.ButtonPressedBorder; } }
		public static Color ButtonPressedGradientBegin { get { return color_table.ButtonPressedGradientBegin; } }
		public static Color ButtonPressedGradientEnd { get { return color_table.ButtonPressedGradientEnd; } }
		public static Color ButtonPressedGradientMiddle { get { return color_table.ButtonPressedGradientMiddle; } }
		public static Color ButtonPressedHighlight { get { return color_table.ButtonPressedHighlight; } }
		public static Color ButtonPressedHighlightBorder { get { return color_table.ButtonPressedHighlightBorder; } }
		public static Color ButtonSelectedBorder { get { return color_table.ButtonSelectedBorder; } }
		public static Color ButtonSelectedGradientBegin { get { return color_table.ButtonSelectedGradientBegin; } }
		public static Color ButtonSelectedGradientEnd { get { return color_table.ButtonSelectedGradientEnd; } }
		public static Color ButtonSelectedGradientMiddle { get { return color_table.ButtonSelectedGradientMiddle; } }
		public static Color ButtonSelectedHighlight { get { return color_table.ButtonSelectedHighlight; } }
		public static Color ButtonSelectedHighlightBorder { get { return color_table.ButtonSelectedHighlightBorder; } }
		public static Color CheckBackground { get { return color_table.CheckBackground; } }
		public static Color CheckPressedBackground { get { return color_table.CheckPressedBackground; } }
		public static Color CheckSelectedBackground { get { return color_table.CheckSelectedBackground; } }
		public static Color GripDark { get { return color_table.GripDark; } }
		public static Color GripLight { get { return color_table.GripLight; } }
		public static Color ImageMarginGradientBegin { get { return color_table.ImageMarginGradientBegin; } }
		public static Color ImageMarginGradientEnd { get { return color_table.ImageMarginGradientEnd; } }
		public static Color ImageMarginGradientMiddle { get { return color_table.ImageMarginGradientMiddle; } }
		public static Color ImageMarginRevealedGradientBegin { get { return color_table.ImageMarginRevealedGradientBegin; } }
		public static Color ImageMarginRevealedGradientEnd { get { return color_table.ImageMarginRevealedGradientEnd; } }
		public static Color ImageMarginRevealedGradientMiddle { get { return color_table.ImageMarginRevealedGradientMiddle; } }
		public static Color MenuBorder { get { return color_table.MenuBorder; } }
		public static Color MenuItemBorder { get { return color_table.MenuItemBorder; } }
		public static Color MenuItemPressedGradientBegin { get { return color_table.MenuItemPressedGradientBegin; } }
		public static Color MenuItemPressedGradientEnd { get { return color_table.MenuItemPressedGradientEnd; } }
		public static Color MenuItemPressedGradientMiddle { get { return color_table.MenuItemPressedGradientMiddle; } }
		public static Color MenuItemSelected { get { return color_table.MenuItemSelected; } }
		public static Color MenuItemSelectedGradientBegin { get { return color_table.MenuItemSelectedGradientBegin; } }
		public static Color MenuItemSelectedGradientEnd { get { return color_table.MenuItemSelectedGradientEnd; } }
		public static Color MenuStripGradientBegin { get { return color_table.MenuStripGradientBegin; } }
		public static Color MenuStripGradientEnd { get { return color_table.MenuStripGradientEnd; } }
		public static Color OverflowButtonGradientBegin { get { return color_table.OverflowButtonGradientBegin; } }
		public static Color OverflowButtonGradientEnd { get { return color_table.OverflowButtonGradientEnd; } }
		public static Color OverflowButtonGradientMiddle { get { return color_table.OverflowButtonGradientMiddle; } }
		public static Color RaftingContainerGradientBegin { get { return color_table.RaftingContainerGradientBegin; } }
		public static Color RaftingContainerGradientEnd { get { return color_table.RaftingContainerGradientEnd; } }
		public static Color SeparatorDark { get { return color_table.SeparatorDark; } }
		public static Color SeparatorLight { get { return color_table.SeparatorLight; } }
		public static Color StatusStripGradientBegin { get { return color_table.StatusStripGradientBegin; } }
		public static Color StatusStripGradientEnd { get { return color_table.StatusStripGradientEnd; } }
		public static Color ToolStripBorder { get { return color_table.ToolStripBorder; } }
		public static Color ToolStripContentPanelGradientBegin { get { return color_table.ToolStripContentPanelGradientBegin; } }
		public static Color ToolStripContentPanelGradientEnd { get { return color_table.ToolStripContentPanelGradientEnd; } }
		public static Color ToolStripDropDownBackground { get { return color_table.ToolStripDropDownBackground; } }
		public static Color ToolStripGradientBegin { get { return color_table.ToolStripGradientBegin; } }
		public static Color ToolStripGradientEnd { get { return color_table.ToolStripGradientEnd; } }
		public static Color ToolStripGradientMiddle { get { return color_table.ToolStripGradientMiddle; } }
		public static Color ToolStripPanelGradientBegin { get { return color_table.ToolStripPanelGradientBegin; } }
		public static Color ToolStripPanelGradientEnd { get { return color_table.ToolStripPanelGradientEnd; } }
		#endregion
	}
}
