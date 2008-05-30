//
// VisualStyleElement.cs
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
// Copyright (c) 2006 Novell, Inc.
//
// Authors:
//	Jonathan Pobst (monkey@jpobst.com)
//

namespace System.Windows.Forms.VisualStyles
{
#if NET_2_0
	public
#endif
	class VisualStyleElement
	{
		#region Private Variables
		#region Class name/part/state constants
		private const string BUTTON = "BUTTON";
		private const string CLOCK = "CLOCK";
		private const string COMBOBOX = "COMBOBOX";
		private const string EDIT = "EDIT";
		private const string EXPLORERBAR = "EXPLORERBAR";
		private const string HEADER = "HEADER";
		private const string LISTVIEW = "LISTVIEW";
		private const string MENU = "MENU";
		private const string MENUBAND = "MENUBAND";
		private const string PAGE = "PAGE";
		private const string PROGRESS = "PROGRESS";
		private const string REBAR = "REBAR";
		#region SCROLLBAR
		private const string SCROLLBAR = "SCROLLBAR";
		enum SCROLLBARPARTS
		{
			SBP_ARROWBTN = 1
		}
		enum ARROWBTNSTATES
		{
			ABS_UPHOVER = 17,
			ABS_DOWNHOVER = 18,
			ABS_LEFTHOVER = 19,
			ABS_RIGHTHOVER = 20,
		}
		#endregion
		private const string SPIN = "SPIN";
		private const string STARTPANEL = "STARTPANEL";
		private const string STATUS = "STATUS";
		private const string TAB = "TAB";
		private const string TASKBAND = "TASKBAND";
		private const string TASKBAR = "TASKBAR";
		private const string TOOLBAR = "TOOLBAR";
		private const string TOOLTIP = "TOOLTIP";
		private const string TRACKBAR = "TRACKBAR";
		private const string TRAYNOTIFY = "TRAYNOTIFY";
		private const string TREEVIEW = "TREEVIEW";
		private const string WINDOW = "WINDOW";
		#endregion

		private string class_name;
		private int part;
		private int state;
		#endregion
		
		#region Constructors/Deconstructors
		internal VisualStyleElement (string className, int part, int state)
		{
			this.class_name = className;
			this.part = part;
			this.state = state;
		}
		#endregion

		#region Public Instance Properties
		public string ClassName { get { return this.class_name; } }
		public int Part { get { return this.part; } }
		public int State { get { return this.state; } }
		#endregion
		
		#region Public Static Methods
		public static VisualStyleElement CreateElement (string className, int part, int state)
		{
			return new VisualStyleElement (className, part, state);
		}
		#endregion
		
		#region Static Classes
		#region Button
		public static class Button
		{
			public static class CheckBox
			{
				public static VisualStyleElement CheckedDisabled { get { return VisualStyleElement.CreateElement (VisualStyleElement.BUTTON, 3, 8); } }
				public static VisualStyleElement CheckedHot { get { return VisualStyleElement.CreateElement (VisualStyleElement.BUTTON, 3, 6); } }
				public static VisualStyleElement CheckedNormal { get { return VisualStyleElement.CreateElement (VisualStyleElement.BUTTON, 3, 5); } }
				public static VisualStyleElement CheckedPressed { get { return VisualStyleElement.CreateElement (VisualStyleElement.BUTTON, 3, 7); } }
				public static VisualStyleElement MixedDisabled { get { return VisualStyleElement.CreateElement (VisualStyleElement.BUTTON, 3, 12); } }
				public static VisualStyleElement MixedHot { get { return VisualStyleElement.CreateElement (VisualStyleElement.BUTTON, 3, 10); } }
				public static VisualStyleElement MixedNormal { get { return VisualStyleElement.CreateElement (VisualStyleElement.BUTTON, 3, 9); } }
				public static VisualStyleElement MixedPressed { get { return VisualStyleElement.CreateElement (VisualStyleElement.BUTTON, 3, 11); } }
				public static VisualStyleElement UncheckedDisabled { get { return VisualStyleElement.CreateElement (VisualStyleElement.BUTTON, 3, 4); } }
				public static VisualStyleElement UncheckedHot { get { return VisualStyleElement.CreateElement (VisualStyleElement.BUTTON, 3, 2); } }
				public static VisualStyleElement UncheckedNormal { get { return VisualStyleElement.CreateElement (VisualStyleElement.BUTTON, 3, 1); } }
				public static VisualStyleElement UncheckedPressed { get { return VisualStyleElement.CreateElement (VisualStyleElement.BUTTON, 3, 3); } }
			}
			public static class GroupBox
			{
				public static VisualStyleElement Disabled { get { return VisualStyleElement.CreateElement (VisualStyleElement.BUTTON, 4, 2); } }
				public static VisualStyleElement Normal { get { return VisualStyleElement.CreateElement (VisualStyleElement.BUTTON, 4, 1); } }
			}
			public static class PushButton
			{
				public static VisualStyleElement Default { get { return VisualStyleElement.CreateElement (VisualStyleElement.BUTTON, 1, 5); } }
				public static VisualStyleElement Disabled { get { return VisualStyleElement.CreateElement (VisualStyleElement.BUTTON, 1, 4); } }
				public static VisualStyleElement Hot { get { return VisualStyleElement.CreateElement (VisualStyleElement.BUTTON, 1, 2); } }
				public static VisualStyleElement Normal { get { return VisualStyleElement.CreateElement (VisualStyleElement.BUTTON, 1, 1); } }
				public static VisualStyleElement Pressed { get { return VisualStyleElement.CreateElement (VisualStyleElement.BUTTON, 1, 3); } }
			}
			public static class RadioButton
			{
				public static VisualStyleElement CheckedDisabled { get { return VisualStyleElement.CreateElement (VisualStyleElement.BUTTON, 2, 8); } }
				public static VisualStyleElement CheckedHot { get { return VisualStyleElement.CreateElement (VisualStyleElement.BUTTON, 2, 6); } }
				public static VisualStyleElement CheckedNormal { get { return VisualStyleElement.CreateElement (VisualStyleElement.BUTTON, 2, 5); } }
				public static VisualStyleElement CheckedPressed { get { return VisualStyleElement.CreateElement (VisualStyleElement.BUTTON, 2, 7); } }
				public static VisualStyleElement UncheckedDisabled { get { return VisualStyleElement.CreateElement (VisualStyleElement.BUTTON, 2, 4); } }
				public static VisualStyleElement UncheckedHot { get { return VisualStyleElement.CreateElement (VisualStyleElement.BUTTON, 2, 2); } }
				public static VisualStyleElement UncheckedNormal { get { return VisualStyleElement.CreateElement (VisualStyleElement.BUTTON, 2, 1); } }
				public static VisualStyleElement UncheckedPressed { get { return VisualStyleElement.CreateElement (VisualStyleElement.BUTTON, 2, 7); } }
			}
			public static class UserButton
			{
				public static VisualStyleElement Normal { get { return VisualStyleElement.CreateElement (VisualStyleElement.BUTTON, 5, 0); } }
			}
		}
		#endregion
		#region ComboBox
		public static class ComboBox
		{
			public static class DropDownButton
			{
				public static VisualStyleElement Disabled { get { return VisualStyleElement.CreateElement (VisualStyleElement.COMBOBOX, 1, 4); } }
				public static VisualStyleElement Hot { get { return VisualStyleElement.CreateElement (VisualStyleElement.COMBOBOX, 1, 2); } }
				public static VisualStyleElement Normal { get { return VisualStyleElement.CreateElement (VisualStyleElement.COMBOBOX, 1, 1); } }
				public static VisualStyleElement Pressed { get { return VisualStyleElement.CreateElement (VisualStyleElement.COMBOBOX, 1, 3); } }
			}
		}
		#endregion
		#region ExplorerBar
		public static class ExplorerBar
		{
			public static class HeaderBackground
			{
				public static VisualStyleElement Normal { get { return VisualStyleElement.CreateElement (VisualStyleElement.EXPLORERBAR, 1, 0); } }
			}
			public static class HeaderClose
			{
				public static VisualStyleElement Hot { get { return VisualStyleElement.CreateElement (VisualStyleElement.EXPLORERBAR, 2, 1); } }
				public static VisualStyleElement Normal { get { return VisualStyleElement.CreateElement (VisualStyleElement.EXPLORERBAR, 2, 2); } }
				public static VisualStyleElement Pressed { get { return VisualStyleElement.CreateElement (VisualStyleElement.EXPLORERBAR, 2, 3); } }
			}
			public static class HeaderPin
			{
				public static VisualStyleElement Hot { get { return VisualStyleElement.CreateElement (VisualStyleElement.EXPLORERBAR, 3, 2); } }
				public static VisualStyleElement Normal { get { return VisualStyleElement.CreateElement (VisualStyleElement.EXPLORERBAR, 3, 1); } }
				public static VisualStyleElement Pressed { get { return VisualStyleElement.CreateElement (VisualStyleElement.EXPLORERBAR, 3, 3); } }
				public static VisualStyleElement SelectedHot { get { return VisualStyleElement.CreateElement (VisualStyleElement.EXPLORERBAR, 3, 5); } }
				public static VisualStyleElement SelectedNormal { get { return VisualStyleElement.CreateElement (VisualStyleElement.EXPLORERBAR, 3, 4); } }
				public static VisualStyleElement SelectedPressed { get { return VisualStyleElement.CreateElement (VisualStyleElement.EXPLORERBAR, 3, 6); } }
			}
			public static class IEBarMenu
			{
				public static VisualStyleElement Hot { get { return VisualStyleElement.CreateElement (VisualStyleElement.EXPLORERBAR, 4, 2); } }
				public static VisualStyleElement Normal { get { return VisualStyleElement.CreateElement (VisualStyleElement.EXPLORERBAR, 4, 1); } }
				public static VisualStyleElement Pressed { get { return VisualStyleElement.CreateElement (VisualStyleElement.EXPLORERBAR, 4, 3); } }
			}
			public static class NormalGroupBackground
			{
				public static VisualStyleElement Normal { get { return VisualStyleElement.CreateElement (VisualStyleElement.EXPLORERBAR, 5, 0); } }
			}
			public static class NormalGroupCollapse
			{
				public static VisualStyleElement Hot { get { return VisualStyleElement.CreateElement (VisualStyleElement.EXPLORERBAR, 6, 2); } }
				public static VisualStyleElement Normal { get { return VisualStyleElement.CreateElement (VisualStyleElement.EXPLORERBAR, 6, 1); } }
				public static VisualStyleElement Pressed { get { return VisualStyleElement.CreateElement (VisualStyleElement.EXPLORERBAR, 6, 3); } }
			}
			public static class NormalGroupExpand
			{
				public static VisualStyleElement Hot { get { return VisualStyleElement.CreateElement (VisualStyleElement.EXPLORERBAR, 7, 2); } }
				public static VisualStyleElement Normal { get { return VisualStyleElement.CreateElement (VisualStyleElement.EXPLORERBAR, 7, 1); } }
				public static VisualStyleElement Pressed { get { return VisualStyleElement.CreateElement (VisualStyleElement.EXPLORERBAR, 7, 3); } }
			}
			public static class NormalGroupHead
			{
				public static VisualStyleElement Normal { get { return VisualStyleElement.CreateElement (VisualStyleElement.EXPLORERBAR, 8, 0); } }
			}
			public static class SpecialGroupBackground
			{
				public static VisualStyleElement Normal { get { return VisualStyleElement.CreateElement (VisualStyleElement.EXPLORERBAR, 9, 0); } }
			}
			public static class SpecialGroupCollapse
			{
				public static VisualStyleElement Hot { get { return VisualStyleElement.CreateElement (VisualStyleElement.EXPLORERBAR, 10, 2); } }
				public static VisualStyleElement Normal { get { return VisualStyleElement.CreateElement (VisualStyleElement.EXPLORERBAR, 10, 1); } }
				public static VisualStyleElement Pressed { get { return VisualStyleElement.CreateElement (VisualStyleElement.EXPLORERBAR, 10, 3); } }
			}
			public static class SpecialGroupExpand
			{
				public static VisualStyleElement Hot { get { return VisualStyleElement.CreateElement (VisualStyleElement.EXPLORERBAR, 11, 2); } }
				public static VisualStyleElement Normal { get { return VisualStyleElement.CreateElement (VisualStyleElement.EXPLORERBAR, 11, 1); } }
				public static VisualStyleElement Pressed { get { return VisualStyleElement.CreateElement (VisualStyleElement.EXPLORERBAR, 11, 3); } }
			}
			public static class SpecialGroupHead
			{
				public static VisualStyleElement Normal { get { return VisualStyleElement.CreateElement (VisualStyleElement.EXPLORERBAR, 12, 0); } }
			}
		}
		#endregion
		#region Header
		public static class Header
		{
			public static class Item
			{
				public static VisualStyleElement Hot { get { return VisualStyleElement.CreateElement (VisualStyleElement.HEADER, 1, 2); } }
				public static VisualStyleElement Normal { get { return VisualStyleElement.CreateElement (VisualStyleElement.HEADER, 1, 1); } }
				public static VisualStyleElement Pressed { get { return VisualStyleElement.CreateElement (VisualStyleElement.HEADER, 1, 3); } }
			}
			public static class ItemLeft
			{
				public static VisualStyleElement Hot { get { return VisualStyleElement.CreateElement (VisualStyleElement.HEADER, 2, 2); } }
				public static VisualStyleElement Normal { get { return VisualStyleElement.CreateElement (VisualStyleElement.HEADER, 2, 1); } }
				public static VisualStyleElement Pressed { get { return VisualStyleElement.CreateElement (VisualStyleElement.HEADER, 2, 3); } }
			}
			public static class ItemRight
			{
				public static VisualStyleElement Hot { get { return VisualStyleElement.CreateElement (VisualStyleElement.HEADER, 3, 2); } }
				public static VisualStyleElement Normal { get { return VisualStyleElement.CreateElement (VisualStyleElement.HEADER, 3, 1); } }
				public static VisualStyleElement Pressed { get { return VisualStyleElement.CreateElement (VisualStyleElement.HEADER, 3, 3); } }
			}
			public static class SortArrow
			{
				public static VisualStyleElement SortedDown { get { return VisualStyleElement.CreateElement (VisualStyleElement.HEADER, 4, 2); } }
				public static VisualStyleElement SortedUp { get { return VisualStyleElement.CreateElement (VisualStyleElement.HEADER, 4, 1); } }
			}
		}
		#endregion
		#region ListView
		public static class ListView
		{
			public static class Detail
			{
				public static VisualStyleElement Normal { get { return VisualStyleElement.CreateElement (VisualStyleElement.LISTVIEW, 3, 0); } }
			}
			public static class EmptyText
			{
				public static VisualStyleElement Normal { get { return VisualStyleElement.CreateElement (VisualStyleElement.LISTVIEW, 5, 0); } }
			}
			public static class Group
			{
				public static VisualStyleElement Normal { get { return VisualStyleElement.CreateElement (VisualStyleElement.LISTVIEW, 2, 0); } }
			}
			public static class Item
			{
				public static VisualStyleElement Disabled { get { return VisualStyleElement.CreateElement (VisualStyleElement.LISTVIEW, 1, 4); } }
				public static VisualStyleElement Hot { get { return VisualStyleElement.CreateElement (VisualStyleElement.LISTVIEW, 1, 2); } }
				public static VisualStyleElement Normal { get { return VisualStyleElement.CreateElement (VisualStyleElement.LISTVIEW, 1, 1); } }
				public static VisualStyleElement Selected { get { return VisualStyleElement.CreateElement (VisualStyleElement.LISTVIEW, 1, 3); } }
				public static VisualStyleElement SelectedNotFocus { get { return VisualStyleElement.CreateElement (VisualStyleElement.LISTVIEW, 1, 5); } }
			}
			public static class SortedDetail
			{
				public static VisualStyleElement Normal { get { return VisualStyleElement.CreateElement (VisualStyleElement.LISTVIEW, 4, 0); } }
			}
		}
		#endregion
		#region Menu
		public static class Menu
		{
			public static class BarDropDown
			{
				public static VisualStyleElement Normal { get { return VisualStyleElement.CreateElement (VisualStyleElement.MENU, 4, 0); } }
			}
			public static class BarItem
			{
				public static VisualStyleElement Normal { get { return VisualStyleElement.CreateElement (VisualStyleElement.MENU, 3, 0); } }
			}
			public static class Chevron
			{
				public static VisualStyleElement Normal { get { return VisualStyleElement.CreateElement (VisualStyleElement.MENU, 5, 0); } }
			}
			public static class DropDown
			{
				public static VisualStyleElement Normal { get { return VisualStyleElement.CreateElement (VisualStyleElement.MENU, 2, 0); } }
			}
			public static class Item
			{
				public static VisualStyleElement Demoted { get { return VisualStyleElement.CreateElement (VisualStyleElement.MENU, 1, 3); } }
				public static VisualStyleElement Normal { get { return VisualStyleElement.CreateElement (VisualStyleElement.MENU, 1, 1); } }
				public static VisualStyleElement Selected { get { return VisualStyleElement.CreateElement (VisualStyleElement.MENU, 1, 2); } }
			}
			public static class Separator
			{
				public static VisualStyleElement Normal { get { return VisualStyleElement.CreateElement (VisualStyleElement.MENU, 6, 0); } }
			}
		}
		#endregion
		#region MenuBand
		public static class MenuBand
		{
			public static class NewApplicationButton
			{
				public static VisualStyleElement Checked { get { return VisualStyleElement.CreateElement (VisualStyleElement.MENUBAND, 1, 5); } }
				public static VisualStyleElement Disabled { get { return VisualStyleElement.CreateElement (VisualStyleElement.MENUBAND, 1, 4); } }
				public static VisualStyleElement Hot { get { return VisualStyleElement.CreateElement (VisualStyleElement.MENUBAND, 1, 2); } }
				public static VisualStyleElement HotChecked { get { return VisualStyleElement.CreateElement (VisualStyleElement.MENUBAND, 1, 6); } }
				public static VisualStyleElement Normal { get { return VisualStyleElement.CreateElement (VisualStyleElement.MENUBAND, 1, 1); } }
				public static VisualStyleElement Pressed { get { return VisualStyleElement.CreateElement (VisualStyleElement.MENUBAND, 1, 3); } }
			}
			public static class Separator
			{
				public static VisualStyleElement Normal { get { return VisualStyleElement.CreateElement (VisualStyleElement.MENUBAND, 2, 0); } }
			}
		}
		#endregion
		#region Page
		public static class Page
		{
			public static class Down
			{
				public static VisualStyleElement Disabled { get { return VisualStyleElement.CreateElement (VisualStyleElement.PAGE, 2, 4); } }
				public static VisualStyleElement Hot { get { return VisualStyleElement.CreateElement (VisualStyleElement.PAGE, 2, 2); } }
				public static VisualStyleElement Normal { get { return VisualStyleElement.CreateElement (VisualStyleElement.PAGE, 2, 3); } }
				public static VisualStyleElement Pressed { get { return VisualStyleElement.CreateElement (VisualStyleElement.PAGE, 2, 1); } }
			}
			public static class DownHorizontal
			{
				public static VisualStyleElement Disabled { get { return VisualStyleElement.CreateElement (VisualStyleElement.PAGE, 4, 4); } }
				public static VisualStyleElement Hot { get { return VisualStyleElement.CreateElement (VisualStyleElement.PAGE, 4, 2); } }
				public static VisualStyleElement Normal { get { return VisualStyleElement.CreateElement (VisualStyleElement.PAGE, 4, 1); } }
				public static VisualStyleElement Pressed { get { return VisualStyleElement.CreateElement (VisualStyleElement.PAGE, 4, 3); } }
			}
			public static class Up
			{
				public static VisualStyleElement Disabled { get { return VisualStyleElement.CreateElement (VisualStyleElement.PAGE, 1, 4); } }
				public static VisualStyleElement Hot { get { return VisualStyleElement.CreateElement (VisualStyleElement.PAGE, 1, 2); } }
				public static VisualStyleElement Normal { get { return VisualStyleElement.CreateElement (VisualStyleElement.PAGE, 1, 1); } }
				public static VisualStyleElement Pressed { get { return VisualStyleElement.CreateElement (VisualStyleElement.PAGE, 1, 3); } }
			}
			public static class UpHorizontal
			{
				public static VisualStyleElement Disabled { get { return VisualStyleElement.CreateElement (VisualStyleElement.PAGE, 3, 4); } }
				public static VisualStyleElement Hot { get { return VisualStyleElement.CreateElement (VisualStyleElement.PAGE, 3, 2); } }
				public static VisualStyleElement Normal { get { return VisualStyleElement.CreateElement (VisualStyleElement.PAGE, 3, 1); } }
				public static VisualStyleElement Pressed { get { return VisualStyleElement.CreateElement (VisualStyleElement.PAGE, 3, 3); } }
			}
		}
		#endregion
		#region ProgressBar
		public static class ProgressBar
		{
			public static class Bar
			{
				public static VisualStyleElement Normal { get { return VisualStyleElement.CreateElement (VisualStyleElement.PROGRESS, 1, 0); } }
			}
			public static class BarVertical
			{
				public static VisualStyleElement Normal { get { return VisualStyleElement.CreateElement (VisualStyleElement.PROGRESS, 2, 0); } }
			}
			public static class Chunk
			{
				public static VisualStyleElement Normal { get { return VisualStyleElement.CreateElement (VisualStyleElement.PROGRESS, 3, 0); } }
			}
			public static class ChunkVertical
			{
				public static VisualStyleElement Normal { get { return VisualStyleElement.CreateElement (VisualStyleElement.PROGRESS, 4, 0); } }
			}
		}
		#endregion
		#region Rebar
		public static class Rebar
		{
			public static class Band
			{
				public static VisualStyleElement Normal { get { return VisualStyleElement.CreateElement (VisualStyleElement.REBAR, 3, 0); } }
			}
			public static class Chevron
			{
				public static VisualStyleElement Hot { get { return VisualStyleElement.CreateElement (VisualStyleElement.REBAR, 4, 2); } }
				public static VisualStyleElement Normal { get { return VisualStyleElement.CreateElement (VisualStyleElement.REBAR, 4, 1); } }
				public static VisualStyleElement Pressed { get { return VisualStyleElement.CreateElement (VisualStyleElement.REBAR, 4, 3); } }
			}
			public static class ChevronVertical
			{
				public static VisualStyleElement Hot { get { return VisualStyleElement.CreateElement (VisualStyleElement.REBAR, 5, 2); } }
				public static VisualStyleElement Normal { get { return VisualStyleElement.CreateElement (VisualStyleElement.REBAR, 5, 1); } }
				public static VisualStyleElement Pressed { get { return VisualStyleElement.CreateElement (VisualStyleElement.REBAR, 5, 3); } }
			}
			public static class Gripper
			{
				public static VisualStyleElement Normal { get { return VisualStyleElement.CreateElement (VisualStyleElement.REBAR, 1, 0); } }
			}
			public static class GripperVertical
			{
				public static VisualStyleElement Normal { get { return VisualStyleElement.CreateElement (VisualStyleElement.REBAR, 2, 0); } }
			}
		}
		#endregion
		#region ScrollBar
		public static class ScrollBar
		{
			public static class ArrowButton
			{
				public static VisualStyleElement DownDisabled { get { return VisualStyleElement.CreateElement (VisualStyleElement.SCROLLBAR, 1, 8); } }
				public static VisualStyleElement DownHot { get { return VisualStyleElement.CreateElement (VisualStyleElement.SCROLLBAR, 1, 6); } }
				public static VisualStyleElement DownNormal { get { return VisualStyleElement.CreateElement (VisualStyleElement.SCROLLBAR, 1, 5); } }
				public static VisualStyleElement DownPressed { get { return VisualStyleElement.CreateElement (VisualStyleElement.SCROLLBAR, 1, 7); } }
				public static VisualStyleElement LeftDisabled { get { return VisualStyleElement.CreateElement (VisualStyleElement.SCROLLBAR, 1, 12); } }
				public static VisualStyleElement LeftHot { get { return VisualStyleElement.CreateElement (VisualStyleElement.SCROLLBAR, 1, 10); } }
				public static VisualStyleElement LeftNormal { get { return VisualStyleElement.CreateElement (VisualStyleElement.SCROLLBAR, 1, 9); } }
				public static VisualStyleElement LeftPressed { get { return VisualStyleElement.CreateElement (VisualStyleElement.SCROLLBAR, 1, 11); } }
				public static VisualStyleElement RightDisabled { get { return VisualStyleElement.CreateElement (VisualStyleElement.SCROLLBAR, 1, 16); } }
				public static VisualStyleElement RightHot { get { return VisualStyleElement.CreateElement (VisualStyleElement.SCROLLBAR, 1, 14); } }
				public static VisualStyleElement RightNormal { get { return VisualStyleElement.CreateElement (VisualStyleElement.SCROLLBAR, 1, 13); } }
				public static VisualStyleElement RightPressed { get { return VisualStyleElement.CreateElement (VisualStyleElement.SCROLLBAR, 1, 15); } }
				public static VisualStyleElement UpDisabled { get { return VisualStyleElement.CreateElement (VisualStyleElement.SCROLLBAR, 1, 4); } }
				public static VisualStyleElement UpHot { get { return VisualStyleElement.CreateElement (VisualStyleElement.SCROLLBAR, 1, 2); } }
				public static VisualStyleElement UpNormal { get { return VisualStyleElement.CreateElement (VisualStyleElement.SCROLLBAR, 1, 1); } }
				public static VisualStyleElement UpPressed { get { return VisualStyleElement.CreateElement (VisualStyleElement.SCROLLBAR, 1, 3); } }
				internal static VisualStyleElement DownHover {
					get {
						return new VisualStyleElement (
							SCROLLBAR,
							(int)SCROLLBARPARTS.SBP_ARROWBTN,
							(int)ARROWBTNSTATES.ABS_DOWNHOVER);
					}
				}
				internal static VisualStyleElement LeftHover {
					get {
						return new VisualStyleElement (
							SCROLLBAR,
							(int)SCROLLBARPARTS.SBP_ARROWBTN,
							(int)ARROWBTNSTATES.ABS_LEFTHOVER);
					}
				}
				internal static VisualStyleElement RightHover {
					get {
						return new VisualStyleElement (
							SCROLLBAR,
							(int)SCROLLBARPARTS.SBP_ARROWBTN,
							(int)ARROWBTNSTATES.ABS_RIGHTHOVER);
					}
				}
				internal static VisualStyleElement UpHover {
					get {
						return new VisualStyleElement (
							SCROLLBAR,
							(int)SCROLLBARPARTS.SBP_ARROWBTN,
							(int)ARROWBTNSTATES.ABS_UPHOVER);
					}
				}
			}
			public static class GripperHorizontal
			{
				public static VisualStyleElement Normal { get { return VisualStyleElement.CreateElement (VisualStyleElement.SCROLLBAR, 8, 0); } }
			}
			public static class GripperVertical
			{
				public static VisualStyleElement Normal { get { return VisualStyleElement.CreateElement (VisualStyleElement.SCROLLBAR, 9, 0); } }
			}
			public static class LeftTrackHorizontal
			{
				public static VisualStyleElement Disabled { get { return VisualStyleElement.CreateElement (VisualStyleElement.SCROLLBAR, 5, 4); } }
				public static VisualStyleElement Hot { get { return VisualStyleElement.CreateElement (VisualStyleElement.SCROLLBAR, 5, 2); } }
				public static VisualStyleElement Normal { get { return VisualStyleElement.CreateElement (VisualStyleElement.SCROLLBAR, 5, 1); } }
				public static VisualStyleElement Pressed { get { return VisualStyleElement.CreateElement (VisualStyleElement.SCROLLBAR, 5, 3); } }
			}
			public static class LowerTrackVertical
			{
				public static VisualStyleElement Disabled { get { return VisualStyleElement.CreateElement (VisualStyleElement.SCROLLBAR, 6, 4); } }
				public static VisualStyleElement Hot { get { return VisualStyleElement.CreateElement (VisualStyleElement.SCROLLBAR, 6, 2); } }
				public static VisualStyleElement Normal { get { return VisualStyleElement.CreateElement (VisualStyleElement.SCROLLBAR, 6, 1); } }
				public static VisualStyleElement Pressed { get { return VisualStyleElement.CreateElement (VisualStyleElement.SCROLLBAR, 6, 3); } }
			}
			public static class RightTrackHorizontal
			{
				public static VisualStyleElement Disabled { get { return VisualStyleElement.CreateElement (VisualStyleElement.SCROLLBAR, 4, 4); } }
				public static VisualStyleElement Hot { get { return VisualStyleElement.CreateElement (VisualStyleElement.SCROLLBAR, 4, 2); } }
				public static VisualStyleElement Normal { get { return VisualStyleElement.CreateElement (VisualStyleElement.SCROLLBAR, 4, 1); } }
				public static VisualStyleElement Pressed { get { return VisualStyleElement.CreateElement (VisualStyleElement.SCROLLBAR, 4, 3); } }
			}
			public static class SizeBox
			{
				public static VisualStyleElement LeftAlign { get { return VisualStyleElement.CreateElement (VisualStyleElement.SCROLLBAR, 10, 2); } }
				public static VisualStyleElement RightAlign { get { return VisualStyleElement.CreateElement (VisualStyleElement.SCROLLBAR, 10, 1); } }
			}
			public static class ThumbButtonHorizontal
			{
				public static VisualStyleElement Disabled { get { return VisualStyleElement.CreateElement (VisualStyleElement.SCROLLBAR, 2, 4); } }
				public static VisualStyleElement Hot { get { return VisualStyleElement.CreateElement (VisualStyleElement.SCROLLBAR, 2, 2); } }
				public static VisualStyleElement Normal { get { return VisualStyleElement.CreateElement (VisualStyleElement.SCROLLBAR, 2, 1); } }
				public static VisualStyleElement Pressed { get { return VisualStyleElement.CreateElement (VisualStyleElement.SCROLLBAR, 2, 3); } }
			}
			public static class ThumbButtonVertical
			{
				public static VisualStyleElement Disabled { get { return VisualStyleElement.CreateElement (VisualStyleElement.SCROLLBAR, 3, 4); } }
				public static VisualStyleElement Hot { get { return VisualStyleElement.CreateElement (VisualStyleElement.SCROLLBAR, 3, 2); } }
				public static VisualStyleElement Normal { get { return VisualStyleElement.CreateElement (VisualStyleElement.SCROLLBAR, 3, 1); } }
				public static VisualStyleElement Pressed { get { return VisualStyleElement.CreateElement (VisualStyleElement.SCROLLBAR, 3, 3); } }
			}
			public static class UpperTrackVertical
			{
				public static VisualStyleElement Disabled { get { return VisualStyleElement.CreateElement (VisualStyleElement.SCROLLBAR, 7, 4); } }
				public static VisualStyleElement Hot { get { return VisualStyleElement.CreateElement (VisualStyleElement.SCROLLBAR, 7, 2); } }
				public static VisualStyleElement Normal { get { return VisualStyleElement.CreateElement (VisualStyleElement.SCROLLBAR, 7, 1); } }
				public static VisualStyleElement Pressed { get { return VisualStyleElement.CreateElement (VisualStyleElement.SCROLLBAR, 7, 3); } }
			}
		}
		#endregion
		#region Spin
		public static class Spin
		{
			public static class Down
			{
				public static VisualStyleElement Disabled { get { return VisualStyleElement.CreateElement (VisualStyleElement.SPIN, 2, 4); } }
				public static VisualStyleElement Hot { get { return VisualStyleElement.CreateElement (VisualStyleElement.SPIN, 2, 2); } }
				public static VisualStyleElement Normal { get { return VisualStyleElement.CreateElement (VisualStyleElement.SPIN, 2, 1); } }
				public static VisualStyleElement Pressed { get { return VisualStyleElement.CreateElement (VisualStyleElement.SPIN, 2, 3); } }
			}
			public static class DownHorizontal
			{
				public static VisualStyleElement Disabled { get { return VisualStyleElement.CreateElement (VisualStyleElement.SPIN, 4, 4); } }
				public static VisualStyleElement Hot { get { return VisualStyleElement.CreateElement (VisualStyleElement.SPIN, 4, 2); } }
				public static VisualStyleElement Normal { get { return VisualStyleElement.CreateElement (VisualStyleElement.SPIN, 4, 1); } }
				public static VisualStyleElement Pressed { get { return VisualStyleElement.CreateElement (VisualStyleElement.SPIN, 4, 3); } }
			}
			public static class Up
			{
				public static VisualStyleElement Disabled { get { return VisualStyleElement.CreateElement (VisualStyleElement.SPIN, 1, 4); } }
				public static VisualStyleElement Hot { get { return VisualStyleElement.CreateElement (VisualStyleElement.SPIN, 1, 2); } }
				public static VisualStyleElement Normal { get { return VisualStyleElement.CreateElement (VisualStyleElement.SPIN, 1, 1); } }
				public static VisualStyleElement Pressed { get { return VisualStyleElement.CreateElement (VisualStyleElement.SPIN, 1, 3); } }
			}
			public static class UpHorizontal
			{
				public static VisualStyleElement Disabled { get { return VisualStyleElement.CreateElement (VisualStyleElement.SPIN, 3, 4); } }
				public static VisualStyleElement Hot { get { return VisualStyleElement.CreateElement (VisualStyleElement.SPIN, 3, 2); } }
				public static VisualStyleElement Normal { get { return VisualStyleElement.CreateElement (VisualStyleElement.SPIN, 3, 1); } }
				public static VisualStyleElement Pressed { get { return VisualStyleElement.CreateElement (VisualStyleElement.SPIN, 3, 3); } }
			}
		}
		#endregion
		#region StartPanel
		public static class StartPanel
		{
			public static class LogOff
			{
				public static VisualStyleElement Normal { get { return VisualStyleElement.CreateElement (VisualStyleElement.STARTPANEL, 8, 0); } }
			}
			public static class LogOffButtons
			{
				public static VisualStyleElement Hot { get { return VisualStyleElement.CreateElement (VisualStyleElement.STARTPANEL, 9, 2); } }
				public static VisualStyleElement Normal { get { return VisualStyleElement.CreateElement (VisualStyleElement.STARTPANEL, 9, 1); } }
				public static VisualStyleElement Pressed { get { return VisualStyleElement.CreateElement (VisualStyleElement.STARTPANEL, 9, 3); } }
			}
			public static class MorePrograms
			{
				public static VisualStyleElement Normal { get { return VisualStyleElement.CreateElement (VisualStyleElement.STARTPANEL, 2, 0); } }
			}
			public static class MoreProgramsArrow
			{
				public static VisualStyleElement Hot { get { return VisualStyleElement.CreateElement (VisualStyleElement.STARTPANEL, 3, 2); } }
				public static VisualStyleElement Normal { get { return VisualStyleElement.CreateElement (VisualStyleElement.STARTPANEL, 3, 1); } }
				public static VisualStyleElement Pressed { get { return VisualStyleElement.CreateElement (VisualStyleElement.STARTPANEL, 3, 3); } }
			}
			public static class PlaceList
			{
				public static VisualStyleElement Normal { get { return VisualStyleElement.CreateElement (VisualStyleElement.STARTPANEL, 6, 0); } }
			}
			public static class PlaceListSeparator
			{
				public static VisualStyleElement Normal { get { return VisualStyleElement.CreateElement (VisualStyleElement.STARTPANEL, 7, 0); } }
			}
			public static class Preview
			{
				public static VisualStyleElement Normal { get { return VisualStyleElement.CreateElement (VisualStyleElement.STARTPANEL, 11, 0); } }
			}
			public static class ProgList
			{
				public static VisualStyleElement Normal { get { return VisualStyleElement.CreateElement (VisualStyleElement.STARTPANEL, 4, 0); } }
			}
			public static class ProgListSeparator
			{
				public static VisualStyleElement Normal { get { return VisualStyleElement.CreateElement (VisualStyleElement.STARTPANEL, 5, 0); } }
			}
			public static class UserPane
			{
				public static VisualStyleElement Normal { get { return VisualStyleElement.CreateElement (VisualStyleElement.STARTPANEL, 1, 0); } }
			}
			public static class UserPicture
			{
				public static VisualStyleElement Normal { get { return VisualStyleElement.CreateElement (VisualStyleElement.STARTPANEL, 10, 0); } }
			}
		}
		#endregion
		#region Status
		public static class Status
		{
			public static class Bar
			{
				public static VisualStyleElement Normal { get { return VisualStyleElement.CreateElement (VisualStyleElement.STATUS, 0, 0); } }
			}
			public static class Gripper
			{
				public static VisualStyleElement Normal { get { return VisualStyleElement.CreateElement (VisualStyleElement.STATUS, 3, 0); } }
			}
			public static class GripperPane
			{
				public static VisualStyleElement Normal { get { return VisualStyleElement.CreateElement (VisualStyleElement.STATUS, 2, 0); } }
			}
			public static class Pane
			{
				public static VisualStyleElement Normal { get { return VisualStyleElement.CreateElement (VisualStyleElement.STATUS, 1, 0); } }
			}
		}
		#endregion
		#region Tab
		public static class Tab
		{
			public static class Body
			{
				public static VisualStyleElement Normal { get { return VisualStyleElement.CreateElement (VisualStyleElement.TAB, 10, 0); } }
			}
			public static class Pane
			{
				public static VisualStyleElement Normal { get { return VisualStyleElement.CreateElement (VisualStyleElement.TAB, 9, 0); } }
			}
			public static class TabItem
			{
				public static VisualStyleElement Disabled { get { return VisualStyleElement.CreateElement (VisualStyleElement.TAB, 1, 4); } }
				public static VisualStyleElement Hot { get { return VisualStyleElement.CreateElement (VisualStyleElement.TAB, 1, 2); } }
				public static VisualStyleElement Normal { get { return VisualStyleElement.CreateElement (VisualStyleElement.TAB, 1, 1); } }
				public static VisualStyleElement Pressed { get { return VisualStyleElement.CreateElement (VisualStyleElement.TAB, 1, 3); } }
			}
			public static class TabItemBothEdges
			{
				public static VisualStyleElement Normal { get { return VisualStyleElement.CreateElement (VisualStyleElement.TAB, 4, 0); } }
			}
			public static class TabItemLeftEdge
			{
				public static VisualStyleElement Disabled { get { return VisualStyleElement.CreateElement (VisualStyleElement.TAB, 2, 4); } }
				public static VisualStyleElement Hot { get { return VisualStyleElement.CreateElement (VisualStyleElement.TAB, 2, 2); } }
				public static VisualStyleElement Normal { get { return VisualStyleElement.CreateElement (VisualStyleElement.TAB, 2, 1); } }
				public static VisualStyleElement Pressed { get { return VisualStyleElement.CreateElement (VisualStyleElement.TAB, 2, 3); } }
			}
			public static class TabItemRightEdge
			{
				public static VisualStyleElement Disabled { get { return VisualStyleElement.CreateElement (VisualStyleElement.TAB, 3, 4); } }
				public static VisualStyleElement Hot { get { return VisualStyleElement.CreateElement (VisualStyleElement.TAB, 3, 2); } }
				public static VisualStyleElement Normal { get { return VisualStyleElement.CreateElement (VisualStyleElement.TAB, 3, 1); } }
				public static VisualStyleElement Pressed { get { return VisualStyleElement.CreateElement (VisualStyleElement.TAB, 3, 3); } }
			}
			public static class TopTabItem
			{
				public static VisualStyleElement Disabled { get { return VisualStyleElement.CreateElement (VisualStyleElement.TAB, 5, 4); } }
				public static VisualStyleElement Hot { get { return VisualStyleElement.CreateElement (VisualStyleElement.TAB, 5, 2); } }
				public static VisualStyleElement Normal { get { return VisualStyleElement.CreateElement (VisualStyleElement.TAB, 5, 1); } }
				public static VisualStyleElement Pressed { get { return VisualStyleElement.CreateElement (VisualStyleElement.TAB, 5, 3); } }
			}
			public static class TopTabItemBothEdges
			{
				public static VisualStyleElement Normal { get { return VisualStyleElement.CreateElement (VisualStyleElement.TAB, 8, 0); } }
			}
			public static class TopTabItemLeftEdge
			{
				public static VisualStyleElement Disabled { get { return VisualStyleElement.CreateElement (VisualStyleElement.TAB, 6, 4); } }
				public static VisualStyleElement Hot { get { return VisualStyleElement.CreateElement (VisualStyleElement.TAB, 6, 2); } }
				public static VisualStyleElement Normal { get { return VisualStyleElement.CreateElement (VisualStyleElement.TAB, 6, 1); } }
				public static VisualStyleElement Pressed { get { return VisualStyleElement.CreateElement (VisualStyleElement.TAB, 6, 3); } }
			}
			public static class TopTabItemRightEdge
			{
				public static VisualStyleElement Disabled { get { return VisualStyleElement.CreateElement (VisualStyleElement.TAB, 7, 4); } }
				public static VisualStyleElement Hot { get { return VisualStyleElement.CreateElement (VisualStyleElement.TAB, 7, 2); } }
				public static VisualStyleElement Normal { get { return VisualStyleElement.CreateElement (VisualStyleElement.TAB, 7, 1); } }
				public static VisualStyleElement Pressed { get { return VisualStyleElement.CreateElement (VisualStyleElement.TAB, 7, 3); } }
			}
		}
		#endregion
		#region TaskBand
		public static class TaskBand
		{
			public static class FlashButton
			{
				public static VisualStyleElement Normal { get { return VisualStyleElement.CreateElement (VisualStyleElement.TASKBAND, 2, 0); } }
			}
			public static class FlashButtonGroupMenu
			{
				public static VisualStyleElement Normal { get { return VisualStyleElement.CreateElement (VisualStyleElement.TASKBAND, 3, 0); } }
			}
			public static class GroupCount
			{
				public static VisualStyleElement Normal { get { return VisualStyleElement.CreateElement (VisualStyleElement.TASKBAND, 1, 0); } }
			}
		}
		#endregion
		#region TaskBar
		public static class Taskbar
		{
			public static class BackgroundBottom
			{
				public static VisualStyleElement Normal { get { return VisualStyleElement.CreateElement (VisualStyleElement.TASKBAR, 1, 0); } }
			}
			public static class BackgroundLeft
			{
				public static VisualStyleElement Normal { get { return VisualStyleElement.CreateElement (VisualStyleElement.TASKBAR, 4, 0); } }
			}
			public static class BackgroundRight
			{
				public static VisualStyleElement Normal { get { return VisualStyleElement.CreateElement (VisualStyleElement.TASKBAR, 2, 0); } }
			}
			public static class BackgroundTop
			{
				public static VisualStyleElement Normal { get { return VisualStyleElement.CreateElement (VisualStyleElement.TASKBAR, 3, 0); } }
			}
			public static class SizingBarBottom
			{
				public static VisualStyleElement Normal { get { return VisualStyleElement.CreateElement (VisualStyleElement.TASKBAR, 5, 0); } }
			}
			public static class SizingBarLeft
			{
				public static VisualStyleElement Normal { get { return VisualStyleElement.CreateElement (VisualStyleElement.TASKBAR, 8, 0); } }
			}
			public static class SizingBarRight
			{
				public static VisualStyleElement Normal { get { return VisualStyleElement.CreateElement (VisualStyleElement.TASKBAR, 6, 0); } }
			}
			public static class SizingBarTop
			{
				public static VisualStyleElement Normal { get { return VisualStyleElement.CreateElement (VisualStyleElement.TASKBAR, 7, 0); } }
			}
		}
		#endregion
		#region TaskBarClock
		public static class TaskbarClock
		{
			public static class Time
			{
				public static VisualStyleElement Normal { get { return VisualStyleElement.CreateElement (VisualStyleElement.CLOCK, 1, 1); } }
			}
		}
		#endregion
		#region TextBox
		public static class TextBox
		{
			public static class Caret
			{
				public static VisualStyleElement Normal { get { return VisualStyleElement.CreateElement (VisualStyleElement.EDIT, 2, 0); } }
			}
			public static class TextEdit
			{
				public static VisualStyleElement Assist { get { return VisualStyleElement.CreateElement (VisualStyleElement.EDIT, 1, 7); } }
				public static VisualStyleElement Disabled { get { return VisualStyleElement.CreateElement (VisualStyleElement.EDIT, 1, 4); } }
				public static VisualStyleElement Focused { get { return VisualStyleElement.CreateElement (VisualStyleElement.EDIT, 1, 5); } }
				public static VisualStyleElement Hot { get { return VisualStyleElement.CreateElement (VisualStyleElement.EDIT, 1, 2); } }
				public static VisualStyleElement Normal { get { return VisualStyleElement.CreateElement (VisualStyleElement.EDIT, 1, 1); } }
				public static VisualStyleElement ReadOnly { get { return VisualStyleElement.CreateElement (VisualStyleElement.EDIT, 1, 6); } }
				public static VisualStyleElement Selected { get { return VisualStyleElement.CreateElement (VisualStyleElement.EDIT, 1, 3); } }
			}
		}
		#endregion
		#region ToolBar
		public static class ToolBar
		{
			public static class Button
			{
				public static VisualStyleElement Checked { get { return VisualStyleElement.CreateElement (VisualStyleElement.TOOLBAR, 1, 5); } }
				public static VisualStyleElement Disabled { get { return VisualStyleElement.CreateElement (VisualStyleElement.TOOLBAR, 1, 4); } }
				public static VisualStyleElement Hot { get { return VisualStyleElement.CreateElement (VisualStyleElement.TOOLBAR, 1, 2); } }
				public static VisualStyleElement HotChecked { get { return VisualStyleElement.CreateElement (VisualStyleElement.TOOLBAR, 1, 6); } }
				public static VisualStyleElement Normal { get { return VisualStyleElement.CreateElement (VisualStyleElement.TOOLBAR, 1, 1); } }
				public static VisualStyleElement Pressed { get { return VisualStyleElement.CreateElement (VisualStyleElement.TOOLBAR, 1, 3); } }
			}
			public static class DropDownButton
			{
				public static VisualStyleElement Checked { get { return VisualStyleElement.CreateElement (VisualStyleElement.TOOLBAR, 2, 5); } }
				public static VisualStyleElement Disabled { get { return VisualStyleElement.CreateElement (VisualStyleElement.TOOLBAR, 2, 4); } }
				public static VisualStyleElement Hot { get { return VisualStyleElement.CreateElement (VisualStyleElement.TOOLBAR, 2, 2); } }
				public static VisualStyleElement HotChecked { get { return VisualStyleElement.CreateElement (VisualStyleElement.TOOLBAR, 2, 6); } }
				public static VisualStyleElement Normal { get { return VisualStyleElement.CreateElement (VisualStyleElement.TOOLBAR, 2, 1); } }
				public static VisualStyleElement Pressed { get { return VisualStyleElement.CreateElement (VisualStyleElement.TOOLBAR, 2, 3); } }
			}
			public static class SeparatorHorizontal
			{
				public static VisualStyleElement Normal { get { return VisualStyleElement.CreateElement (VisualStyleElement.TOOLBAR, 5, 0); } }
			}
			public static class SeparatorVertical
			{
				public static VisualStyleElement Normal { get { return VisualStyleElement.CreateElement (VisualStyleElement.TOOLBAR, 6, 0); } }
			}
			public static class SplitButton
			{
				public static VisualStyleElement Checked { get { return VisualStyleElement.CreateElement (VisualStyleElement.TOOLBAR, 3, 5); } }
				public static VisualStyleElement Disabled { get { return VisualStyleElement.CreateElement (VisualStyleElement.TOOLBAR, 3, 4); } }
				public static VisualStyleElement Hot { get { return VisualStyleElement.CreateElement (VisualStyleElement.TOOLBAR, 3, 2); } }
				public static VisualStyleElement HotChecked { get { return VisualStyleElement.CreateElement (VisualStyleElement.TOOLBAR, 3, 6); } }
				public static VisualStyleElement Normal { get { return VisualStyleElement.CreateElement (VisualStyleElement.TOOLBAR, 3, 1); } }
				public static VisualStyleElement Pressed { get { return VisualStyleElement.CreateElement (VisualStyleElement.TOOLBAR, 3, 3); } }
			}
			public static class SplitButtonDropDown
			{
				public static VisualStyleElement Checked { get { return VisualStyleElement.CreateElement (VisualStyleElement.TOOLBAR, 4, 5); } }
				public static VisualStyleElement Disabled { get { return VisualStyleElement.CreateElement (VisualStyleElement.TOOLBAR, 4, 4); } }
				public static VisualStyleElement Hot { get { return VisualStyleElement.CreateElement (VisualStyleElement.TOOLBAR, 4, 2); } }
				public static VisualStyleElement HotChecked { get { return VisualStyleElement.CreateElement (VisualStyleElement.TOOLBAR, 4, 6); } }
				public static VisualStyleElement Normal { get { return VisualStyleElement.CreateElement (VisualStyleElement.TOOLBAR, 4, 1); } }
				public static VisualStyleElement Pressed { get { return VisualStyleElement.CreateElement (VisualStyleElement.TOOLBAR, 4, 3); } }
			}
		}
		#endregion
		#region ToolTip
		public static class ToolTip
		{
			public static class Balloon
			{
				public static VisualStyleElement Link { get { return VisualStyleElement.CreateElement (VisualStyleElement.TOOLTIP, 3, 2); } }
				public static VisualStyleElement Normal { get { return VisualStyleElement.CreateElement (VisualStyleElement.TOOLTIP, 3, 1); } }
			}
			public static class BalloonTitle
			{
				public static VisualStyleElement Normal { get { return VisualStyleElement.CreateElement (VisualStyleElement.TOOLTIP, 4, 0); } }
			}
			public static class Close
			{
				public static VisualStyleElement Hot { get { return VisualStyleElement.CreateElement (VisualStyleElement.TOOLTIP, 5, 2); } }
				public static VisualStyleElement Normal { get { return VisualStyleElement.CreateElement (VisualStyleElement.TOOLTIP, 5, 1); } }
				public static VisualStyleElement Pressed { get { return VisualStyleElement.CreateElement (VisualStyleElement.TOOLTIP, 5, 3); } }
			}
			public static class Standard
			{
				public static VisualStyleElement Link { get { return VisualStyleElement.CreateElement (VisualStyleElement.TOOLTIP, 1, 2); } }
				public static VisualStyleElement Normal { get { return VisualStyleElement.CreateElement (VisualStyleElement.TOOLTIP, 1, 1); } }
			}
			public static class StandardTitle
			{
				public static VisualStyleElement Normal { get { return VisualStyleElement.CreateElement (VisualStyleElement.TOOLTIP, 2, 0); } }
			}
		}
		#endregion
		#region TrackBar
		public static class TrackBar
		{
			public static class Thumb
			{
				public static VisualStyleElement Disabled { get { return VisualStyleElement.CreateElement (VisualStyleElement.TRACKBAR, 3, 5); } }
				public static VisualStyleElement Focused { get { return VisualStyleElement.CreateElement (VisualStyleElement.TRACKBAR, 3, 4); } }
				public static VisualStyleElement Hot { get { return VisualStyleElement.CreateElement (VisualStyleElement.TRACKBAR, 3, 2); } }
				public static VisualStyleElement Normal { get { return VisualStyleElement.CreateElement (VisualStyleElement.TRACKBAR, 3, 1); } }
				public static VisualStyleElement Pressed { get { return VisualStyleElement.CreateElement (VisualStyleElement.TRACKBAR, 3, 3); } }
			}
			public static class ThumbBottom
			{
				public static VisualStyleElement Disabled { get { return VisualStyleElement.CreateElement (VisualStyleElement.TRACKBAR, 4, 5); } }
				public static VisualStyleElement Focused { get { return VisualStyleElement.CreateElement (VisualStyleElement.TRACKBAR, 4, 4); } }
				public static VisualStyleElement Hot { get { return VisualStyleElement.CreateElement (VisualStyleElement.TRACKBAR, 4, 2); } }
				public static VisualStyleElement Normal { get { return VisualStyleElement.CreateElement (VisualStyleElement.TRACKBAR, 4, 1); } }
				public static VisualStyleElement Pressed { get { return VisualStyleElement.CreateElement (VisualStyleElement.TRACKBAR, 4, 3); } }
			}
			public static class ThumbLeft
			{
				public static VisualStyleElement Disabled { get { return VisualStyleElement.CreateElement (VisualStyleElement.TRACKBAR, 7, 5); } }
				public static VisualStyleElement Focused { get { return VisualStyleElement.CreateElement (VisualStyleElement.TRACKBAR, 7, 4); } }
				public static VisualStyleElement Hot { get { return VisualStyleElement.CreateElement (VisualStyleElement.TRACKBAR, 7, 2); } }
				public static VisualStyleElement Normal { get { return VisualStyleElement.CreateElement (VisualStyleElement.TRACKBAR, 7, 1); } }
				public static VisualStyleElement Pressed { get { return VisualStyleElement.CreateElement (VisualStyleElement.TRACKBAR, 7, 3); } }
			}
			public static class ThumbRight
			{
				public static VisualStyleElement Disabled { get { return VisualStyleElement.CreateElement (VisualStyleElement.TRACKBAR, 8, 5); } }
				public static VisualStyleElement Focused { get { return VisualStyleElement.CreateElement (VisualStyleElement.TRACKBAR, 8, 4); } }
				public static VisualStyleElement Hot { get { return VisualStyleElement.CreateElement (VisualStyleElement.TRACKBAR, 8, 2); } }
				public static VisualStyleElement Normal { get { return VisualStyleElement.CreateElement (VisualStyleElement.TRACKBAR, 8, 1); } }
				public static VisualStyleElement Pressed { get { return VisualStyleElement.CreateElement (VisualStyleElement.TRACKBAR, 8, 3); } }
			}
			public static class ThumbTop
			{
				public static VisualStyleElement Disabled { get { return VisualStyleElement.CreateElement (VisualStyleElement.TRACKBAR, 5, 5); } }
				public static VisualStyleElement Focused { get { return VisualStyleElement.CreateElement (VisualStyleElement.TRACKBAR, 5, 4); } }
				public static VisualStyleElement Hot { get { return VisualStyleElement.CreateElement (VisualStyleElement.TRACKBAR, 5, 2); } }
				public static VisualStyleElement Normal { get { return VisualStyleElement.CreateElement (VisualStyleElement.TRACKBAR, 5, 1); } }
				public static VisualStyleElement Pressed { get { return VisualStyleElement.CreateElement (VisualStyleElement.TRACKBAR, 5, 3); } }
			}
			public static class ThumbVertical
			{
				public static VisualStyleElement Disabled { get { return VisualStyleElement.CreateElement (VisualStyleElement.TRACKBAR, 6, 5); } }
				public static VisualStyleElement Focused { get { return VisualStyleElement.CreateElement (VisualStyleElement.TRACKBAR, 6, 4); } }
				public static VisualStyleElement Hot { get { return VisualStyleElement.CreateElement (VisualStyleElement.TRACKBAR, 6, 2); } }
				public static VisualStyleElement Normal { get { return VisualStyleElement.CreateElement (VisualStyleElement.TRACKBAR, 6, 1); } }
				public static VisualStyleElement Pressed { get { return VisualStyleElement.CreateElement (VisualStyleElement.TRACKBAR, 6, 3); } }
			}
			public static class Ticks
			{
				public static VisualStyleElement Normal { get { return VisualStyleElement.CreateElement (VisualStyleElement.TRACKBAR, 9, 1); } }
			}
			public static class TicksVertical
			{
				public static VisualStyleElement Normal { get { return VisualStyleElement.CreateElement (VisualStyleElement.TRACKBAR, 10, 1); } }
			}
			public static class Track
			{
				public static VisualStyleElement Normal { get { return VisualStyleElement.CreateElement (VisualStyleElement.TRACKBAR, 1, 1); } }
			}
			public static class TrackVertical
			{
				public static VisualStyleElement Normal { get { return VisualStyleElement.CreateElement (VisualStyleElement.TRACKBAR, 2, 1); } }
			}
		}
		#endregion
		#region TrayNotify
		public static class TrayNotify
		{
			public static class AnimateBackground
			{
				public static VisualStyleElement Normal { get { return VisualStyleElement.CreateElement (VisualStyleElement.TRAYNOTIFY, 2, 0); } }
			}
			public static class Background
			{
				public static VisualStyleElement Normal { get { return VisualStyleElement.CreateElement (VisualStyleElement.TRAYNOTIFY, 1, 0); } }
			}
		}
		#endregion
		#region TreeView
		public static class TreeView
		{
			public static class Branch
			{
				public static VisualStyleElement Normal { get { return VisualStyleElement.CreateElement (VisualStyleElement.TREEVIEW, 3, 0); } }
			}
			public static class Glyph
			{
				public static VisualStyleElement Closed { get { return VisualStyleElement.CreateElement (VisualStyleElement.TREEVIEW, 2, 1); } }
				public static VisualStyleElement Opened { get { return VisualStyleElement.CreateElement (VisualStyleElement.TREEVIEW, 2, 2); } }
			}
			public static class Item
			{
				public static VisualStyleElement Disabled { get { return VisualStyleElement.CreateElement (VisualStyleElement.TREEVIEW, 1, 4); } }
				public static VisualStyleElement Hot { get { return VisualStyleElement.CreateElement (VisualStyleElement.TREEVIEW, 1, 2); } }
				public static VisualStyleElement Normal { get { return VisualStyleElement.CreateElement (VisualStyleElement.TREEVIEW, 1, 1); } }
				public static VisualStyleElement Selected { get { return VisualStyleElement.CreateElement (VisualStyleElement.TREEVIEW, 1, 3); } }
				public static VisualStyleElement SelectedNotFocus { get { return VisualStyleElement.CreateElement (VisualStyleElement.TREEVIEW, 1, 5); } }
			}
		}
		#endregion
		#region Window
		public static class Window
		{
			public static class Caption
			{
				public static VisualStyleElement Active { get { return VisualStyleElement.CreateElement (VisualStyleElement.WINDOW, 1, 1); } }
				public static VisualStyleElement Disabled { get { return VisualStyleElement.CreateElement (VisualStyleElement.WINDOW, 1, 3); } }
				public static VisualStyleElement Inactive { get { return VisualStyleElement.CreateElement (VisualStyleElement.WINDOW, 1, 2); } }
			}
			public static class CaptionSizingTemplate
			{
				public static VisualStyleElement Normal { get { return VisualStyleElement.CreateElement (VisualStyleElement.WINDOW, 30, 0); } }
			}
			public static class CloseButton
			{
				public static VisualStyleElement Disabled { get { return VisualStyleElement.CreateElement (VisualStyleElement.WINDOW, 18, 4); } }
				public static VisualStyleElement Hot { get { return VisualStyleElement.CreateElement (VisualStyleElement.WINDOW, 18, 2); } }
				public static VisualStyleElement Normal { get { return VisualStyleElement.CreateElement (VisualStyleElement.WINDOW, 18, 1); } }
				public static VisualStyleElement Pressed { get { return VisualStyleElement.CreateElement (VisualStyleElement.WINDOW, 18, 3); } }
			}
			public static class Dialog
			{
				public static VisualStyleElement Normal { get { return VisualStyleElement.CreateElement (VisualStyleElement.WINDOW, 29, 0); } }
			}
			public static class FrameBottom
			{
				public static VisualStyleElement Active { get { return VisualStyleElement.CreateElement (VisualStyleElement.WINDOW, 9, 1); } }
				public static VisualStyleElement Inactive { get { return VisualStyleElement.CreateElement (VisualStyleElement.WINDOW, 9, 2); } }
			}
			public static class FrameBottomSizingTemplate
			{
				public static VisualStyleElement Normal { get { return VisualStyleElement.CreateElement (VisualStyleElement.WINDOW, 36, 0); } }
			}
			public static class FrameLeft
			{
				public static VisualStyleElement Active { get { return VisualStyleElement.CreateElement (VisualStyleElement.WINDOW, 7, 1); } }
				public static VisualStyleElement Inactive { get { return VisualStyleElement.CreateElement (VisualStyleElement.WINDOW, 7, 2); } }
			}
			public static class FrameLeftSizingTemplate
			{
				public static VisualStyleElement Normal { get { return VisualStyleElement.CreateElement (VisualStyleElement.WINDOW, 32, 0); } }
			}
			public static class FrameRight
			{
				public static VisualStyleElement Active { get { return VisualStyleElement.CreateElement (VisualStyleElement.WINDOW, 8, 1); } }
				public static VisualStyleElement Inactive { get { return VisualStyleElement.CreateElement (VisualStyleElement.WINDOW, 8, 2); } }
			}
			public static class FrameRightSizingTemplate
			{
				public static VisualStyleElement Normal { get { return VisualStyleElement.CreateElement (VisualStyleElement.WINDOW, 34, 0); } }
			}
			public static class HelpButton
			{
				public static VisualStyleElement Disabled { get { return VisualStyleElement.CreateElement (VisualStyleElement.WINDOW, 23, 4); } }
				public static VisualStyleElement Hot { get { return VisualStyleElement.CreateElement (VisualStyleElement.WINDOW, 23, 2); } }
				public static VisualStyleElement Normal { get { return VisualStyleElement.CreateElement (VisualStyleElement.WINDOW, 23, 1); } }
				public static VisualStyleElement Pressed { get { return VisualStyleElement.CreateElement (VisualStyleElement.WINDOW, 23, 3); } }
			}
			public static class HorizontalScroll
			{
				public static VisualStyleElement Disabled { get { return VisualStyleElement.CreateElement (VisualStyleElement.WINDOW, 25, 4); } }
				public static VisualStyleElement Hot { get { return VisualStyleElement.CreateElement (VisualStyleElement.WINDOW, 25, 2); } }
				public static VisualStyleElement Normal { get { return VisualStyleElement.CreateElement (VisualStyleElement.WINDOW, 25, 1); } }
				public static VisualStyleElement Pressed { get { return VisualStyleElement.CreateElement (VisualStyleElement.WINDOW, 25, 3); } }
			}
			public static class HorizontalThumb
			{
				public static VisualStyleElement Disabled { get { return VisualStyleElement.CreateElement (VisualStyleElement.WINDOW, 26, 4); } }
				public static VisualStyleElement Hot { get { return VisualStyleElement.CreateElement (VisualStyleElement.WINDOW, 26, 2); } }
				public static VisualStyleElement Normal { get { return VisualStyleElement.CreateElement (VisualStyleElement.WINDOW, 26, 1); } }
				public static VisualStyleElement Pressed { get { return VisualStyleElement.CreateElement (VisualStyleElement.WINDOW, 26, 3); } }
			}
			public static class MaxButton
			{
				public static VisualStyleElement Disabled { get { return VisualStyleElement.CreateElement (VisualStyleElement.WINDOW, 17, 4); } }
				public static VisualStyleElement Hot { get { return VisualStyleElement.CreateElement (VisualStyleElement.WINDOW, 17, 2); } }
				public static VisualStyleElement Normal { get { return VisualStyleElement.CreateElement (VisualStyleElement.WINDOW, 17, 1); } }
				public static VisualStyleElement Pressed { get { return VisualStyleElement.CreateElement (VisualStyleElement.WINDOW, 17, 3); } }
			}
			public static class MaxCaption
			{
				public static VisualStyleElement Active { get { return VisualStyleElement.CreateElement (VisualStyleElement.WINDOW, 5, 1); } }
				public static VisualStyleElement Disabled { get { return VisualStyleElement.CreateElement (VisualStyleElement.WINDOW, 5, 3); } }
				public static VisualStyleElement Inactive { get { return VisualStyleElement.CreateElement (VisualStyleElement.WINDOW, 5, 2); } }
			}
			public static class MdiCloseButton
			{
				public static VisualStyleElement Disabled { get { return VisualStyleElement.CreateElement (VisualStyleElement.WINDOW, 20, 4); } }
				public static VisualStyleElement Hot { get { return VisualStyleElement.CreateElement (VisualStyleElement.WINDOW, 20, 2); } }
				public static VisualStyleElement Normal { get { return VisualStyleElement.CreateElement (VisualStyleElement.WINDOW, 20, 1); } }
				public static VisualStyleElement Pressed { get { return VisualStyleElement.CreateElement (VisualStyleElement.WINDOW, 20, 3); } }
			}
			public static class MdiHelpButton
			{
				public static VisualStyleElement Disabled { get { return VisualStyleElement.CreateElement (VisualStyleElement.WINDOW, 24, 4); } }
				public static VisualStyleElement Hot { get { return VisualStyleElement.CreateElement (VisualStyleElement.WINDOW, 24, 2); } }
				public static VisualStyleElement Normal { get { return VisualStyleElement.CreateElement (VisualStyleElement.WINDOW, 24, 1); } }
				public static VisualStyleElement Pressed { get { return VisualStyleElement.CreateElement (VisualStyleElement.WINDOW, 24, 3); } }
			}
			public static class MdiMinButton
			{
				public static VisualStyleElement Disabled { get { return VisualStyleElement.CreateElement (VisualStyleElement.WINDOW, 16, 4); } }
				public static VisualStyleElement Hot { get { return VisualStyleElement.CreateElement (VisualStyleElement.WINDOW, 16, 2); } }
				public static VisualStyleElement Normal { get { return VisualStyleElement.CreateElement (VisualStyleElement.WINDOW, 16, 1); } }
				public static VisualStyleElement Pressed { get { return VisualStyleElement.CreateElement (VisualStyleElement.WINDOW, 16, 3); } }
			}
			public static class MdiRestoreButton
			{
				public static VisualStyleElement Disabled { get { return VisualStyleElement.CreateElement (VisualStyleElement.WINDOW, 22, 4); } }
				public static VisualStyleElement Hot { get { return VisualStyleElement.CreateElement (VisualStyleElement.WINDOW, 22, 2); } }
				public static VisualStyleElement Normal { get { return VisualStyleElement.CreateElement (VisualStyleElement.WINDOW, 22, 1); } }
				public static VisualStyleElement Pressed { get { return VisualStyleElement.CreateElement (VisualStyleElement.WINDOW, 22, 3); } }
			}
			public static class MdiSysButton
			{
				public static VisualStyleElement Disabled { get { return VisualStyleElement.CreateElement (VisualStyleElement.WINDOW, 14, 4); } }
				public static VisualStyleElement Hot { get { return VisualStyleElement.CreateElement (VisualStyleElement.WINDOW, 14, 2); } }
				public static VisualStyleElement Normal { get { return VisualStyleElement.CreateElement (VisualStyleElement.WINDOW, 14, 1); } }
				public static VisualStyleElement Pressed { get { return VisualStyleElement.CreateElement (VisualStyleElement.WINDOW, 14, 3); } }
			}
			public static class MinButton
			{
				public static VisualStyleElement Disabled { get { return VisualStyleElement.CreateElement (VisualStyleElement.WINDOW, 15, 4); } }
				public static VisualStyleElement Hot { get { return VisualStyleElement.CreateElement (VisualStyleElement.WINDOW, 15, 2); } }
				public static VisualStyleElement Normal { get { return VisualStyleElement.CreateElement (VisualStyleElement.WINDOW, 15, 1); } }
				public static VisualStyleElement Pressed { get { return VisualStyleElement.CreateElement (VisualStyleElement.WINDOW, 15, 3); } }
			}
			public static class MinCaption
			{
				public static VisualStyleElement Active { get { return VisualStyleElement.CreateElement (VisualStyleElement.WINDOW, 3, 1); } }
				public static VisualStyleElement Disabled { get { return VisualStyleElement.CreateElement (VisualStyleElement.WINDOW, 3, 3); } }
				public static VisualStyleElement Inactive { get { return VisualStyleElement.CreateElement (VisualStyleElement.WINDOW, 3, 2); } }
			}
			public static class RestoreButton
			{
				public static VisualStyleElement Disabled { get { return VisualStyleElement.CreateElement (VisualStyleElement.WINDOW, 21, 4); } }
				public static VisualStyleElement Hot { get { return VisualStyleElement.CreateElement (VisualStyleElement.WINDOW, 21, 2); } }
				public static VisualStyleElement Normal { get { return VisualStyleElement.CreateElement (VisualStyleElement.WINDOW, 21, 1); } }
				public static VisualStyleElement Pressed { get { return VisualStyleElement.CreateElement (VisualStyleElement.WINDOW, 21, 3); } }
			}
			public static class SmallCaption
			{
				public static VisualStyleElement Active { get { return VisualStyleElement.CreateElement (VisualStyleElement.WINDOW, 2, 1); } }
				public static VisualStyleElement Disabled { get { return VisualStyleElement.CreateElement (VisualStyleElement.WINDOW, 2, 3); } }
				public static VisualStyleElement Inactive { get { return VisualStyleElement.CreateElement (VisualStyleElement.WINDOW, 2, 2); } }
			}
			public static class SmallCaptionSizingTemplate
			{
				public static VisualStyleElement Normal { get { return VisualStyleElement.CreateElement (VisualStyleElement.WINDOW, 31, 0); } }
			}
			public static class SmallCloseButton
			{
				public static VisualStyleElement Disabled { get { return VisualStyleElement.CreateElement (VisualStyleElement.WINDOW, 19, 4); } }
				public static VisualStyleElement Hot { get { return VisualStyleElement.CreateElement (VisualStyleElement.WINDOW, 19, 2); } }
				public static VisualStyleElement Normal { get { return VisualStyleElement.CreateElement (VisualStyleElement.WINDOW, 19, 1); } }
				public static VisualStyleElement Pressed { get { return VisualStyleElement.CreateElement (VisualStyleElement.WINDOW, 19, 3); } }
			}
			public static class SmallFrameBottom
			{
				public static VisualStyleElement Active { get { return VisualStyleElement.CreateElement (VisualStyleElement.WINDOW, 12, 1); } }
				public static VisualStyleElement Inactive { get { return VisualStyleElement.CreateElement (VisualStyleElement.WINDOW, 12, 2); } }
			}
			public static class SmallFrameBottomSizingTemplate
			{
				public static VisualStyleElement Normal { get { return VisualStyleElement.CreateElement (VisualStyleElement.WINDOW, 37, 0); } }
			}
			public static class SmallFrameLeft
			{
				public static VisualStyleElement Active { get { return VisualStyleElement.CreateElement (VisualStyleElement.WINDOW, 10, 1); } }
				public static VisualStyleElement Inactive { get { return VisualStyleElement.CreateElement (VisualStyleElement.WINDOW, 10, 2); } }
			}
			public static class SmallFrameLeftSizingTemplate
			{
				public static VisualStyleElement Normal { get { return VisualStyleElement.CreateElement (VisualStyleElement.WINDOW, 33, 0); } }
			}
			public static class SmallFrameRight
			{
				public static VisualStyleElement Active { get { return VisualStyleElement.CreateElement (VisualStyleElement.WINDOW, 11, 1); } }
				public static VisualStyleElement Inactive { get { return VisualStyleElement.CreateElement (VisualStyleElement.WINDOW, 11, 2); } }
			}
			public static class SmallFrameRightSizingTemplate
			{
				public static VisualStyleElement Normal { get { return VisualStyleElement.CreateElement (VisualStyleElement.WINDOW, 35, 0); } }
			}
			public static class SmallMaxCaption
			{
				public static VisualStyleElement Active { get { return VisualStyleElement.CreateElement (VisualStyleElement.WINDOW, 6, 1); } }
				public static VisualStyleElement Disabled { get { return VisualStyleElement.CreateElement (VisualStyleElement.WINDOW, 6, 3); } }
				public static VisualStyleElement Inactive { get { return VisualStyleElement.CreateElement (VisualStyleElement.WINDOW, 6, 2); } }
			}
			public static class SmallMinCaption
			{
				public static VisualStyleElement Active { get { return VisualStyleElement.CreateElement (VisualStyleElement.WINDOW, 4, 1); } }
				public static VisualStyleElement Disabled { get { return VisualStyleElement.CreateElement (VisualStyleElement.WINDOW, 4, 3); } }
				public static VisualStyleElement Inactive { get { return VisualStyleElement.CreateElement (VisualStyleElement.WINDOW, 4, 2); } }
			}
			public static class SysButton
			{
				public static VisualStyleElement Disabled { get { return VisualStyleElement.CreateElement (VisualStyleElement.WINDOW, 13, 4); } }
				public static VisualStyleElement Hot { get { return VisualStyleElement.CreateElement (VisualStyleElement.WINDOW, 13, 2); } }
				public static VisualStyleElement Normal { get { return VisualStyleElement.CreateElement (VisualStyleElement.WINDOW, 13, 1); } }
				public static VisualStyleElement Pressed { get { return VisualStyleElement.CreateElement (VisualStyleElement.WINDOW, 13, 3); } }
			}
			public static class VerticalScroll
			{
				public static VisualStyleElement Disabled { get { return VisualStyleElement.CreateElement (VisualStyleElement.WINDOW, 27, 4); } }
				public static VisualStyleElement Hot { get { return VisualStyleElement.CreateElement (VisualStyleElement.WINDOW, 27, 2); } }
				public static VisualStyleElement Normal { get { return VisualStyleElement.CreateElement (VisualStyleElement.WINDOW, 27, 1); } }
				public static VisualStyleElement Pressed { get { return VisualStyleElement.CreateElement (VisualStyleElement.WINDOW, 27, 3); } }
			}
			public static class VerticalThumb
			{
				public static VisualStyleElement Disabled { get { return VisualStyleElement.CreateElement (VisualStyleElement.WINDOW, 28, 4); } }
				public static VisualStyleElement Hot { get { return VisualStyleElement.CreateElement (VisualStyleElement.WINDOW, 28, 2); } }
				public static VisualStyleElement Normal { get { return VisualStyleElement.CreateElement (VisualStyleElement.WINDOW, 28, 1); } }
				public static VisualStyleElement Pressed { get { return VisualStyleElement.CreateElement (VisualStyleElement.WINDOW, 28, 3); } }
			}
		}
		#endregion
		#endregion
	}
}