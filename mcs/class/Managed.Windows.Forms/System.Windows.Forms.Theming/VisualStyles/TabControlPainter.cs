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
// Copyright (c) 2008 George Giolfan
//
// Authors:
//	George Giolfan (georgegiolfan@yahoo.com)

using System.Drawing;
using System.Windows.Forms.VisualStyles;
namespace System.Windows.Forms.Theming.VisualStyles
{
	class TabControlPainter : Default.TabControlPainter
	{
		static bool ShouldPaint (TabControl tabControl) {
			return tabControl.Alignment == TabAlignment.Top &&
				tabControl.DrawMode == TabDrawMode.Normal;
		}
		public override void Draw (Graphics dc, Rectangle area, TabControl tab) {
			if (!ShouldPaint (tab)) {
				base.Draw (dc, area, tab);
				return;
			}

			VisualStyleElement element = VisualStyleElement.Tab.Pane.Normal;
			if (!VisualStyleRenderer.IsElementDefined (element)) {
				base.Draw (dc, area, tab);
				return;
			}
			Rectangle panel_rectangle = GetTabPanelRect (tab);
			if (panel_rectangle.IntersectsWith (area))
				new VisualStyleRenderer (element).DrawBackground (dc, panel_rectangle, area);

			//TODO: The base method should be refactored to allow this class to override only what it needs to.
			int start = 0;
			int end = tab.TabPages.Count;
			int delta = 1;

			if (tab.Alignment == TabAlignment.Top) {
				start = end;
				end = 0;
				delta = -1;
			}
			int counter = start;
			for (; counter != end; counter += delta) {
				for (int i = tab.SliderPos; i < tab.TabPages.Count; i++) {
					if (i == tab.SelectedIndex)
						continue;
					if (counter != tab.TabPages[i].Row)
						continue;
					Rectangle rect = tab.GetTabRect (i);
					if (!rect.IntersectsWith (area))
						continue;
					DrawTab (dc, tab.TabPages[i], tab, rect, false);
				}
			}

			if (tab.SelectedIndex != -1 && tab.SelectedIndex >= tab.SliderPos) {
				Rectangle rect = tab.GetTabRect (tab.SelectedIndex);
				if (rect.IntersectsWith (area))
					DrawTab (dc, tab.TabPages[tab.SelectedIndex], tab, rect, true);
			}

			if (tab.ShowSlider) {
				Rectangle right = GetRightScrollRect (tab);
				Rectangle left = GetLeftScrollRect (tab);
				ControlPaint.DrawScrollButton (dc, right, ScrollButton.Right, tab.RightSliderState);
				ControlPaint.DrawScrollButton (dc, left, ScrollButton.Left, tab.LeftSliderState);
			}
		}
		protected override int DrawTab (Graphics dc, TabPage page, TabControl tab, Rectangle bounds, bool is_selected)
		{
			if (!ShouldPaint (tab))
				return base.DrawTab (dc, page, tab, bounds, is_selected);
			VisualStyleElement element = GetVisualStyleElement (tab, page, is_selected);
			if (!VisualStyleRenderer.IsElementDefined (element))
				return base.DrawTab (dc, page, tab, bounds, is_selected);
			new VisualStyleRenderer (element).DrawBackground (dc, bounds);
			bounds.Inflate (
				-(FocusRectSpacing.X + BorderThickness.X),
				-(FocusRectSpacing.Y + BorderThickness.Y));
			Rectangle text_area = bounds;
			if (tab.ImageList != null && page.ImageIndex >= 0 && page.ImageIndex < tab.ImageList.Images.Count) {
				int image_y = bounds.Y + (bounds.Height - tab.ImageList.ImageSize.Height) / 2;
				tab.ImageList.Draw (dc, new Point (bounds.X, image_y), page.ImageIndex);
				int image_occupied_space = tab.ImageList.ImageSize.Width + 2;
				text_area.X += image_occupied_space;
				text_area.Width -= image_occupied_space;
			}
			if (page.Text != null)
				dc.DrawString (page.Text, page.Font, SystemBrushes.ControlText, text_area, DefaultFormatting);
			if (tab.Focused && is_selected)
				ControlPaint.DrawFocusRectangle (dc, bounds);
			return 0;
		}
		static VisualStyleElement GetVisualStyleElement (TabControl tabControl, TabPage tabPage, bool selected)
		{
			bool top_edge = tabPage.Row == tabControl.RowCount;
			int tab_page_index = tabControl.TabPages.IndexOf (tabPage);
			bool left_edge = true;
			int index;
			for (index = tabControl.SliderPos; index < tab_page_index; index++)
				if (tabControl.TabPages [index].Row == tabPage.Row) {
					left_edge = false;
					break;
				}
			bool right_edge = true;
			for (index = tab_page_index; index < tabControl.TabCount; index++)
				if (tabControl.TabPages [index].Row == tabPage.Row) {
					right_edge = false;
					break;
				}
			if (!tabPage.Enabled)
				#region Disabled
				if (top_edge)
					if (left_edge)
						if (right_edge)
							return VisualStyleElement.Tab.TopTabItem.Disabled;
						else
							return VisualStyleElement.Tab.TopTabItemLeftEdge.Disabled;
					else
						if (right_edge)
							return VisualStyleElement.Tab.TopTabItemRightEdge.Disabled;
						else
							return VisualStyleElement.Tab.TopTabItem.Disabled;
				else
					if (left_edge)
						if (right_edge)
							return VisualStyleElement.Tab.TabItem.Disabled;
						else
							return VisualStyleElement.Tab.TabItemLeftEdge.Disabled;
					else
						if (right_edge)
							return VisualStyleElement.Tab.TabItemRightEdge.Disabled;
						else
							return VisualStyleElement.Tab.TabItem.Disabled;
				#endregion
			/*TODO Repaint the tab when the mouse enters or leaves its area.
			else if (hot)
				#region Hot
				if (top_edge)
					if (left_edge)
						if (right_edge)
							return VisualStyleElement.Tab.TopTabItem.Hot;
						else
							return VisualStyleElement.Tab.TopTabItemLeftEdge.Hot;
					else
						if (right_edge)
							return VisualStyleElement.Tab.TopTabItemRightEdge.Hot;
						else
							return VisualStyleElement.Tab.TopTabItem.Hot;
				else
					if (left_edge)
						if (right_edge)
							return VisualStyleElement.Tab.TabItem.Hot;
						else
							return VisualStyleElement.Tab.TabItemLeftEdge.Hot;
					else
						if (right_edge)
							return VisualStyleElement.Tab.TabItemRightEdge.Hot;
						else
							return VisualStyleElement.Tab.TabItem.Hot;
				#endregion
			*/
			else if (selected)
				#region Pressed
				if (top_edge)
					if (left_edge)
						if (right_edge)
							return VisualStyleElement.Tab.TopTabItem.Pressed;
						else
							return VisualStyleElement.Tab.TopTabItemLeftEdge.Pressed;
					else
						if (right_edge)
							return VisualStyleElement.Tab.TopTabItemRightEdge.Pressed;
						else
							return VisualStyleElement.Tab.TopTabItem.Pressed;
				else
					if (left_edge)
						if (right_edge)
							return VisualStyleElement.Tab.TabItem.Pressed;
						else
							return VisualStyleElement.Tab.TabItemLeftEdge.Pressed;
					else
						if (right_edge)
							return VisualStyleElement.Tab.TabItemRightEdge.Pressed;
						else
							return VisualStyleElement.Tab.TabItem.Pressed;
				#endregion
			else
				#region Normal
				if (top_edge)
					if (left_edge)
						if (right_edge)
							return VisualStyleElement.Tab.TopTabItemBothEdges.Normal;
						else
							return VisualStyleElement.Tab.TopTabItemLeftEdge.Normal;
					else
						if (right_edge)
							return VisualStyleElement.Tab.TopTabItemRightEdge.Normal;
						else
							return VisualStyleElement.Tab.TopTabItem.Normal;
				else
					if (left_edge)
						if (right_edge)
							return VisualStyleElement.Tab.TabItemBothEdges.Normal;
						else
							return VisualStyleElement.Tab.TabItemLeftEdge.Normal;
					else
						if (right_edge)
							return VisualStyleElement.Tab.TabItemRightEdge.Normal;
						else
							return VisualStyleElement.Tab.TabItem.Normal;
				#endregion
		}
	}
}
