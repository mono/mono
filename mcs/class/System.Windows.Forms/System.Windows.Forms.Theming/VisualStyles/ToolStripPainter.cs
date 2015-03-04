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
	class ToolStripPainter : Default.ToolStripPainter
	{
		static bool IsDisabled (ToolStripItem toolStripItem)
		{
			return !toolStripItem.Enabled;
		}
		static bool IsPressed (ToolStripItem toolStripItem)
		{
			return toolStripItem.Pressed;
		}
		static bool IsChecked (ToolStripItem toolStripItem)
		{
			ToolStripButton tool_strip_button = toolStripItem as ToolStripButton;
			if (tool_strip_button == null)
				return false;
			return tool_strip_button.Checked;
		}
		static bool IsHot (ToolStripItem toolStripItem)
		{
			return toolStripItem.Selected;
		}
		public override void OnRenderButtonBackground (ToolStripItemRenderEventArgs e)
		{
			if (!ThemeVisualStyles.RenderClientAreas) {
				base.OnRenderButtonBackground (e);
				return;
			}
			VisualStyleElement element;
			if (IsDisabled (e.Item))
				element = VisualStyleElement.ToolBar.Button.Disabled;
			else if (IsPressed (e.Item))
				element = VisualStyleElement.ToolBar.Button.Pressed;
			else if (IsChecked (e.Item))
				if (IsHot (e.Item))
					element = VisualStyleElement.ToolBar.Button.HotChecked;
				else
					element = VisualStyleElement.ToolBar.Button.Checked;
			else if (IsHot (e.Item))
				element = VisualStyleElement.ToolBar.Button.Hot;
			else
				element = VisualStyleElement.ToolBar.Button.Normal;
			if (!VisualStyleRenderer.IsElementDefined (element)) {
				base.OnRenderButtonBackground (e);
				return;
			}
			new VisualStyleRenderer (element).DrawBackground (e.Graphics, e.Item.Bounds);
		}
		public override void OnRenderDropDownButtonBackground (ToolStripItemRenderEventArgs e)
		{
			if (!ThemeVisualStyles.RenderClientAreas) {
				base.OnRenderDropDownButtonBackground (e);
				return;
			}
			VisualStyleElement element;
			if (IsDisabled (e.Item))
				element = VisualStyleElement.ToolBar.DropDownButton.Disabled;
			else if (IsPressed (e.Item))
				element = VisualStyleElement.ToolBar.DropDownButton.Pressed;
			else if (IsChecked (e.Item))
				if (IsHot (e.Item))
					element = VisualStyleElement.ToolBar.DropDownButton.HotChecked;
				else
					element = VisualStyleElement.ToolBar.DropDownButton.Checked;
			else if (IsHot (e.Item))
				element = VisualStyleElement.ToolBar.DropDownButton.Hot;
			else
				element = VisualStyleElement.ToolBar.DropDownButton.Normal;
			if (!VisualStyleRenderer.IsElementDefined (element)) {
				base.OnRenderDropDownButtonBackground (e);
				return;
			}
			new VisualStyleRenderer (element).DrawBackground (e.Graphics, e.Item.Bounds);
		}
		public override void OnRenderGrip (ToolStripGripRenderEventArgs e)
		{
			if (!ThemeVisualStyles.RenderClientAreas) {
				base.OnRenderGrip (e);
				return;
			}
			if (e.GripStyle == ToolStripGripStyle.Hidden)
				return;
			VisualStyleElement element = e.GripDisplayStyle == ToolStripGripDisplayStyle.Vertical ?
				VisualStyleElement.Rebar.Gripper.Normal :
				VisualStyleElement.Rebar.GripperVertical.Normal;
			if (!VisualStyleRenderer.IsElementDefined (element)) {
				base.OnRenderGrip (e);
				return;
			}
			new VisualStyleRenderer (element).DrawBackground (e.Graphics, e.GripDisplayStyle == ToolStripGripDisplayStyle.Vertical ?
				// GetPartSize seems to return useless values.
				new Rectangle (2, 0, 5, 20) :
				new Rectangle (0, 2, 20, 5));
		}
		public override void OnRenderOverflowButtonBackground (ToolStripItemRenderEventArgs e)
		{
			if (!ThemeVisualStyles.RenderClientAreas) {
				base.OnRenderOverflowButtonBackground (e);
				return;
			}
			VisualStyleElement element = e.ToolStrip.Orientation == Orientation.Horizontal ?
				VisualStyleElement.Rebar.Chevron.Normal :
				VisualStyleElement.Rebar.ChevronVertical.Normal;
			if (!VisualStyleRenderer.IsElementDefined (element)) {
				base.OnRenderOverflowButtonBackground (e);
				return;
			}
			OnRenderButtonBackground (e);
			new VisualStyleRenderer (element).DrawBackground (e.Graphics, e.Item.Bounds);
		}
		public override void OnRenderSeparator (ToolStripSeparatorRenderEventArgs e)
		{
			if (!ThemeVisualStyles.RenderClientAreas) {
				base.OnRenderSeparator (e);
				return;
			}
			VisualStyleElement element = e.ToolStrip.Orientation == Orientation.Horizontal ?
				VisualStyleElement.ToolBar.SeparatorHorizontal.Normal :
				VisualStyleElement.ToolBar.SeparatorVertical.Normal;
			if (!VisualStyleRenderer.IsElementDefined (element)) {
				base.OnRenderSeparator (e);
				return;
			}
			new VisualStyleRenderer (element).DrawBackground (e.Graphics, e.Item.Bounds);
		}
		public override void OnRenderSplitButtonBackground (ToolStripItemRenderEventArgs e)
		{
			if (!ThemeVisualStyles.RenderClientAreas) {
				base.OnRenderSplitButtonBackground (e);
				return;
			}
			VisualStyleElement element, drop_down_element;
			if (IsDisabled (e.Item)) {
				element = VisualStyleElement.ToolBar.SplitButton.Disabled;
				drop_down_element = VisualStyleElement.ToolBar.SplitButtonDropDown.Disabled;;
			} else if (IsPressed (e.Item)) {
				element = VisualStyleElement.ToolBar.SplitButton.Pressed;
				drop_down_element = VisualStyleElement.ToolBar.SplitButtonDropDown.Pressed;
			} else if (IsChecked (e.Item))
				if (IsHot (e.Item)) {
					element = VisualStyleElement.ToolBar.SplitButton.HotChecked;
					drop_down_element = VisualStyleElement.ToolBar.SplitButtonDropDown.HotChecked;
				} else {
					element = VisualStyleElement.ToolBar.Button.Checked;
					drop_down_element = VisualStyleElement.ToolBar.SplitButtonDropDown.Checked;
				}
			else if (IsHot (e.Item)) {
				element = VisualStyleElement.ToolBar.SplitButton.Hot;
				drop_down_element = VisualStyleElement.ToolBar.SplitButtonDropDown.Hot;
			} else {
				element = VisualStyleElement.ToolBar.SplitButton.Normal;
				drop_down_element = VisualStyleElement.ToolBar.SplitButtonDropDown.Normal;
			}
			if (!VisualStyleRenderer.IsElementDefined (element) ||
				!VisualStyleRenderer.IsElementDefined (drop_down_element)) {
				base.OnRenderSplitButtonBackground (e);
				return;
			}
			ToolStripSplitButton tool_strip_split_button = (ToolStripSplitButton)e.Item;
			VisualStyleRenderer renderer = new VisualStyleRenderer (element);
			renderer.DrawBackground (e.Graphics, tool_strip_split_button.ButtonBounds);
			renderer.SetParameters (drop_down_element);
			renderer.DrawBackground (e.Graphics, tool_strip_split_button.DropDownButtonBounds);
		}
		public override void OnRenderToolStripBackground (ToolStripRenderEventArgs e)
		{
			if (e.ToolStrip.BackgroundImage != null)
				return;
				
			if (!ThemeVisualStyles.RenderClientAreas) {
				base.OnRenderToolStripBackground (e);
				return;
			}
			VisualStyleElement element;
			if (e.ToolStrip is StatusStrip)
				element = VisualStyleElement.Status.Bar.Normal;
			else
				element = VisualStyleElement.Rebar.Band.Normal;
			if (!VisualStyleRenderer.IsElementDefined (element)) {
				base.OnRenderToolStripBackground (e);
				return;
			}
			new VisualStyleRenderer (element).DrawBackground (e.Graphics, e.ToolStrip.Bounds, e.AffectedBounds);
		}
	}
}
