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
	public class VisualStyleElement
	{
		#region Private Variables
		#region Class name/part/state constants
		private const string BUTTON = "BUTTON";
		private const string CLOCK = "CLOCK";
		private const string COMBOBOX = "COMBOBOX";
		#region DATEPICKER
		const string DATEPICKER = "DATEPICKER";
		enum DATEPICKERPARTS
		{
			DP_DATEBORDER = 2,
			DP_SHOWCALENDARBUTTONRIGHT
		}
		enum DATEBORDERSTATES
		{
			DPDB_NORMAL = 1,
			DPDB_HOT,
			DPDB_FOCUSED,
			DPDB_DISABLED
		}
		enum SHOWCALENDARBUTTONRIGHTSTATES
		{
			DPSCBR_NORMAL = 1,
			DPSCBR_HOT,
			DPSCBR_PRESSED,
			DPSCBR_DISABLED
		}
		#endregion
		private const string EDIT = "EDIT";
		private const string EXPLORERBAR = "EXPLORERBAR";
		private const string HEADER = "HEADER";
		private const string LISTVIEW = "LISTVIEW";
		private const string MENU = "MENU";
		private const string MENUBAND = "MENUBAND";
		private const string PAGE = "PAGE";
		private const string PROGRESS = "PROGRESS";
		private const string REBAR = "REBAR";
		private const string SCROLLBAR = "SCROLLBAR";
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
				public static VisualStyleElement CheckedDisabled {
					get {
						return VisualStyleElement.CreateElement (
							BUTTON,
							(int)BUTTONPARTS.BP_CHECKBOX,
							(int)CHECKBOXSTATES.CBS_CHECKEDDISABLED);
					}
				}
				public static VisualStyleElement CheckedHot {
					get {
						return VisualStyleElement.CreateElement (
							BUTTON,
							(int)BUTTONPARTS.BP_CHECKBOX,
							(int)CHECKBOXSTATES.CBS_CHECKEDHOT);
					}
				}
				public static VisualStyleElement CheckedNormal {
					get {
						return VisualStyleElement.CreateElement (
							BUTTON,
							(int)BUTTONPARTS.BP_CHECKBOX,
							(int)CHECKBOXSTATES.CBS_CHECKEDNORMAL);
					}
				}
				public static VisualStyleElement CheckedPressed {
					get {
						return VisualStyleElement.CreateElement (
							BUTTON,
							(int)BUTTONPARTS.BP_CHECKBOX,
							(int)CHECKBOXSTATES.CBS_CHECKEDPRESSED);
					}
				}
				public static VisualStyleElement MixedDisabled {
					get {
						return VisualStyleElement.CreateElement (
							BUTTON,
							(int)BUTTONPARTS.BP_CHECKBOX,
							(int)CHECKBOXSTATES.CBS_MIXEDDISABLED);
					}
				}
				public static VisualStyleElement MixedHot {
					get {
						return VisualStyleElement.CreateElement (
							BUTTON,
							(int)BUTTONPARTS.BP_CHECKBOX,
							(int)CHECKBOXSTATES.CBS_MIXEDHOT);
					}
				}
				public static VisualStyleElement MixedNormal {
					get {
						return VisualStyleElement.CreateElement (
							BUTTON,
							(int)BUTTONPARTS.BP_CHECKBOX,
							(int)CHECKBOXSTATES.CBS_MIXEDNORMAL);
					}
				}
				public static VisualStyleElement MixedPressed {
					get {
						return VisualStyleElement.CreateElement (
							BUTTON,
							(int)BUTTONPARTS.BP_CHECKBOX,
							(int)CHECKBOXSTATES.CBS_MIXEDPRESSED);
					}
				}
				public static VisualStyleElement UncheckedDisabled {
					get {
						return VisualStyleElement.CreateElement (
							BUTTON,
							(int)BUTTONPARTS.BP_CHECKBOX,
							(int)CHECKBOXSTATES.CBS_UNCHECKEDDISABLED);
					}
				}
				public static VisualStyleElement UncheckedHot {
					get {
						return VisualStyleElement.CreateElement (
							BUTTON,
							(int)BUTTONPARTS.BP_CHECKBOX,
							(int)CHECKBOXSTATES.CBS_UNCHECKEDHOT);
					}
				}
				public static VisualStyleElement UncheckedNormal {
					get {
						return VisualStyleElement.CreateElement (
							BUTTON,
							(int)BUTTONPARTS.BP_CHECKBOX,
							(int)CHECKBOXSTATES.CBS_UNCHECKEDNORMAL);
					}
				}
				public static VisualStyleElement UncheckedPressed {
					get {
						return VisualStyleElement.CreateElement (
							BUTTON,
							(int)BUTTONPARTS.BP_CHECKBOX,
							(int)CHECKBOXSTATES.CBS_UNCHECKEDPRESSED);
					}
				}
			}
			public static class GroupBox
			{
				public static VisualStyleElement Disabled {
					get {
						return VisualStyleElement.CreateElement (
							BUTTON,
							(int)BUTTONPARTS.BP_GROUPBOX,
							(int)GROUPBOXSTATES.GBS_DISABLED);
					}
				}
				public static VisualStyleElement Normal {
					get {
						return VisualStyleElement.CreateElement (
							BUTTON,
							(int)BUTTONPARTS.BP_GROUPBOX,
							(int)GROUPBOXSTATES.GBS_NORMAL);
					}
				}
			}
			public static class PushButton
			{
				public static VisualStyleElement Default {
					get {
						return VisualStyleElement.CreateElement (
							BUTTON,
							(int)BUTTONPARTS.BP_PUSHBUTTON,
							(int)PUSHBUTTONSTATES.PBS_DEFAULTED);
					}
				}
				public static VisualStyleElement Disabled {
					get {
						return VisualStyleElement.CreateElement (
							BUTTON,
							(int)BUTTONPARTS.BP_PUSHBUTTON,
							(int)PUSHBUTTONSTATES.PBS_DISABLED);
					}
				}
				public static VisualStyleElement Hot {
					get {
						return VisualStyleElement.CreateElement (
							BUTTON,
							(int)BUTTONPARTS.BP_PUSHBUTTON,
							(int)PUSHBUTTONSTATES.PBS_HOT);
					}
				}
				public static VisualStyleElement Normal {
					get {
						return VisualStyleElement.CreateElement (
							BUTTON,
							(int)BUTTONPARTS.BP_PUSHBUTTON,
							(int)PUSHBUTTONSTATES.PBS_NORMAL);
					}
				}
				public static VisualStyleElement Pressed {
					get {
						return VisualStyleElement.CreateElement (
							BUTTON,
							(int)BUTTONPARTS.BP_PUSHBUTTON,
							(int)PUSHBUTTONSTATES.PBS_PRESSED);
					}
				}
			}
			public static class RadioButton
			{
				public static VisualStyleElement CheckedDisabled {
					get {
						return VisualStyleElement.CreateElement (
							BUTTON,
							(int)BUTTONPARTS.BP_RADIOBUTTON,
							(int)RADIOBUTTONSTATES.RBS_CHECKEDDISABLED);
					}
				}
				public static VisualStyleElement CheckedHot {
					get {
						return VisualStyleElement.CreateElement (
							BUTTON,
							(int)BUTTONPARTS.BP_RADIOBUTTON,
							(int)RADIOBUTTONSTATES.RBS_CHECKEDHOT);
					}
				}
				public static VisualStyleElement CheckedNormal {
					get {
						return VisualStyleElement.CreateElement (
							BUTTON,
							(int)BUTTONPARTS.BP_RADIOBUTTON,
							(int)RADIOBUTTONSTATES.RBS_CHECKEDNORMAL);
					}
				}
				public static VisualStyleElement CheckedPressed {
					get {
						return VisualStyleElement.CreateElement (
							BUTTON,
							(int)BUTTONPARTS.BP_RADIOBUTTON,
							(int)RADIOBUTTONSTATES.RBS_CHECKEDPRESSED);
					}
				}
				public static VisualStyleElement UncheckedDisabled {
					get {
						return VisualStyleElement.CreateElement (
							BUTTON,
							(int)BUTTONPARTS.BP_RADIOBUTTON,
							(int)RADIOBUTTONSTATES.RBS_UNCHECKEDDISABLED);
					}
				}
				public static VisualStyleElement UncheckedHot {
					get {
						return VisualStyleElement.CreateElement (
							BUTTON,
							(int)BUTTONPARTS.BP_RADIOBUTTON,
							(int)RADIOBUTTONSTATES.RBS_UNCHECKEDHOT);
					}
				}
				public static VisualStyleElement UncheckedNormal {
					get {
						return VisualStyleElement.CreateElement (
							BUTTON,
							(int)BUTTONPARTS.BP_RADIOBUTTON,
							(int)RADIOBUTTONSTATES.RBS_UNCHECKEDNORMAL);
					}
				}
				public static VisualStyleElement UncheckedPressed {
					get {
						return VisualStyleElement.CreateElement (
							BUTTON,
							(int)BUTTONPARTS.BP_RADIOBUTTON,
							(int)RADIOBUTTONSTATES.RBS_UNCHECKEDPRESSED);
					}
				}
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
				public static VisualStyleElement Disabled {
					get {
						return VisualStyleElement.CreateElement (
							COMBOBOX,
							(int)COMBOBOXPARTS.CP_DROPDOWNBUTTON,
							(int)COMBOBOXSTYLESTATES.CBXS_DISABLED);
					}
				}
				public static VisualStyleElement Hot {
					get {
						return VisualStyleElement.CreateElement (
							COMBOBOX,
							(int)COMBOBOXPARTS.CP_DROPDOWNBUTTON,
							(int)COMBOBOXSTYLESTATES.CBXS_HOT);
					}
				}
				public static VisualStyleElement Normal {
					get {
						return VisualStyleElement.CreateElement (
							COMBOBOX,
							(int)COMBOBOXPARTS.CP_DROPDOWNBUTTON,
							(int)COMBOBOXSTYLESTATES.CBXS_NORMAL);
					}
				}
				public static VisualStyleElement Pressed {
					get {
						return VisualStyleElement.CreateElement (
							COMBOBOX,
							(int)COMBOBOXPARTS.CP_DROPDOWNBUTTON,
							(int)COMBOBOXSTYLESTATES.CBXS_PRESSED);
					}
				}
			}
			internal static class Border
			{
				public static VisualStyleElement Normal {
					get {
						return new VisualStyleElement (
							COMBOBOX,
							(int)COMBOBOXPARTS.CP_BORDER,
							(int)BORDERSTATES.CBB_NORMAL);
					}
				}
				public static VisualStyleElement Hot {
					get {
						return new VisualStyleElement (
							COMBOBOX,
							(int)COMBOBOXPARTS.CP_BORDER,
							(int)BORDERSTATES.CBB_HOT);
					}
				}
				public static VisualStyleElement Focused {
					get {
						return new VisualStyleElement (
							COMBOBOX,
							(int)COMBOBOXPARTS.CP_BORDER,
							(int)BORDERSTATES.CBB_FOCUSED);
					}
				}
				public static VisualStyleElement Disabled {
					get {
						return new VisualStyleElement (
							COMBOBOX,
							(int)COMBOBOXPARTS.CP_BORDER,
							(int)BORDERSTATES.CBB_DISABLED);
					}
				}
			}
		}
		#endregion
		#region DatePicker
		internal static class DatePicker
		{
			public static class DateBorder
			{
				public static VisualStyleElement Normal {
					get {
						return new VisualStyleElement (
							DATEPICKER,
							(int)DATEPICKERPARTS.DP_DATEBORDER,
							(int)DATEBORDERSTATES.DPDB_NORMAL);
					}
				}
				public static VisualStyleElement Hot {
					get {
						return new VisualStyleElement (
							DATEPICKER,
							(int)DATEPICKERPARTS.DP_DATEBORDER,
							(int)DATEBORDERSTATES.DPDB_HOT);
					}
				}
				public static VisualStyleElement Focused {
					get {
						return new VisualStyleElement (
							DATEPICKER,
							(int)DATEPICKERPARTS.DP_DATEBORDER,
							(int)DATEBORDERSTATES.DPDB_FOCUSED);
					}
				}
				public static VisualStyleElement Disabled {
					get {
						return new VisualStyleElement (
							DATEPICKER,
							(int)DATEPICKERPARTS.DP_DATEBORDER,
							(int)DATEBORDERSTATES.DPDB_DISABLED);
					}
				}
			}
			public static class ShowCalendarButtonRight
			{
				public static VisualStyleElement Normal {
					get {
						return new VisualStyleElement (
							DATEPICKER,
							(int)DATEPICKERPARTS.DP_SHOWCALENDARBUTTONRIGHT,
							(int)SHOWCALENDARBUTTONRIGHTSTATES.DPSCBR_NORMAL);
					}
				}
				public static VisualStyleElement Hot {
					get {
						return new VisualStyleElement (
							DATEPICKER,
							(int)DATEPICKERPARTS.DP_SHOWCALENDARBUTTONRIGHT,
							(int)SHOWCALENDARBUTTONRIGHTSTATES.DPSCBR_HOT);
					}
				}
				public static VisualStyleElement Pressed {
					get {
						return new VisualStyleElement (
							DATEPICKER,
							(int)DATEPICKERPARTS.DP_SHOWCALENDARBUTTONRIGHT,
							(int)SHOWCALENDARBUTTONRIGHTSTATES.DPSCBR_PRESSED);
					}
				}
				public static VisualStyleElement Disabled {
					get {
						return new VisualStyleElement (
							DATEPICKER,
							(int)DATEPICKERPARTS.DP_SHOWCALENDARBUTTONRIGHT,
							(int)SHOWCALENDARBUTTONRIGHTSTATES.DPSCBR_DISABLED);
					}
				}
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
				public static VisualStyleElement Hot {
					get {
						return VisualStyleElement.CreateElement (
							HEADER,
							(int)HEADERPARTS.HP_HEADERITEM,
							(int)HEADERITEMSTATES.HIS_HOT);
					}
				}
				public static VisualStyleElement Normal {
					get {
						return VisualStyleElement.CreateElement (
							HEADER,
							(int)HEADERPARTS.HP_HEADERITEM,
							(int)HEADERITEMSTATES.HIS_NORMAL);
					}
				}
				public static VisualStyleElement Pressed {
					get {
						return VisualStyleElement.CreateElement (
							HEADER,
							(int)HEADERPARTS.HP_HEADERITEM,
							(int)HEADERITEMSTATES.HIS_PRESSED);
					}
				}
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
				public static VisualStyleElement Normal {
					get {
						return VisualStyleElement.CreateElement (
							PROGRESS,
							(int)PROGRESSPARTS.PP_BAR,
							0);
					}
				}
			}
			public static class BarVertical
			{
				public static VisualStyleElement Normal {
					get {
						return VisualStyleElement.CreateElement (
							PROGRESS,
							(int)PROGRESSPARTS.PP_BARVERT,
							0);
					}
				}
			}
			public static class Chunk
			{
				public static VisualStyleElement Normal {
					get {
						return VisualStyleElement.CreateElement (
							PROGRESS,
							(int)PROGRESSPARTS.PP_CHUNK,
							0);
					}
				}
			}
			public static class ChunkVertical
			{
				public static VisualStyleElement Normal {
					get {
						return VisualStyleElement.CreateElement (
							PROGRESS,
							(int)PROGRESSPARTS.PP_CHUNKVERT,
							0);
					}
				}
			}
		}
		#endregion
		#region Rebar
		public static class Rebar
		{
			public static class Band
			{
				public static VisualStyleElement Normal {
					get {
						return VisualStyleElement.CreateElement (
							REBAR,
							(int)REBARPARTS.RP_BAND,
							0);
					}
				}
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
				public static VisualStyleElement DownDisabled {
					get {
						return VisualStyleElement.CreateElement (
							SCROLLBAR,
							(int)SCROLLBARPARTS.SBP_ARROWBTN,
							(int)ARROWBTNSTATES.ABS_DOWNDISABLED);
					}
				}
				public static VisualStyleElement DownHot {
					get {
						return VisualStyleElement.CreateElement (
							SCROLLBAR,
							(int)SCROLLBARPARTS.SBP_ARROWBTN,
							(int)ARROWBTNSTATES.ABS_DOWNHOT);
					}
				}
				public static VisualStyleElement DownNormal {
					get {
						return VisualStyleElement.CreateElement (
							SCROLLBAR,
							(int)SCROLLBARPARTS.SBP_ARROWBTN,
							(int)ARROWBTNSTATES.ABS_DOWNNORMAL);
					}
				}
				public static VisualStyleElement DownPressed {
					get {
						return VisualStyleElement.CreateElement (
							SCROLLBAR,
							(int)SCROLLBARPARTS.SBP_ARROWBTN,
							(int)ARROWBTNSTATES.ABS_DOWNPRESSED);
					}
				}
				public static VisualStyleElement LeftDisabled {
					get {
						return VisualStyleElement.CreateElement (
							SCROLLBAR,
							(int)SCROLLBARPARTS.SBP_ARROWBTN,
							(int)ARROWBTNSTATES.ABS_LEFTDISABLED);
					}
				}
				public static VisualStyleElement LeftHot {
					get {
						return VisualStyleElement.CreateElement (
							SCROLLBAR,
							(int)SCROLLBARPARTS.SBP_ARROWBTN,
							(int)ARROWBTNSTATES.ABS_LEFTHOT);
					}
				}
				public static VisualStyleElement LeftNormal {
					get {
						return VisualStyleElement.CreateElement (
							SCROLLBAR,
							(int)SCROLLBARPARTS.SBP_ARROWBTN,
							(int)ARROWBTNSTATES.ABS_LEFTNORMAL);
					}
				}
				public static VisualStyleElement LeftPressed {
					get {
						return VisualStyleElement.CreateElement (
							SCROLLBAR,
							(int)SCROLLBARPARTS.SBP_ARROWBTN,
							(int)ARROWBTNSTATES.ABS_LEFTPRESSED);
					}
				}
				public static VisualStyleElement RightDisabled {
					get {
						return VisualStyleElement.CreateElement (
							SCROLLBAR,
							(int)SCROLLBARPARTS.SBP_ARROWBTN,
							(int)ARROWBTNSTATES.ABS_RIGHTDISABLED);
					}
				}
				public static VisualStyleElement RightHot {
					get {
						return VisualStyleElement.CreateElement (
							SCROLLBAR,
							(int)SCROLLBARPARTS.SBP_ARROWBTN,
							(int)ARROWBTNSTATES.ABS_RIGHTHOT);
					}
				}
				public static VisualStyleElement RightNormal {
					get {
						return VisualStyleElement.CreateElement (
							SCROLLBAR,
							(int)SCROLLBARPARTS.SBP_ARROWBTN,
							(int)ARROWBTNSTATES.ABS_RIGHTNORMAL);
					}
				}
				public static VisualStyleElement RightPressed {
					get {
						return VisualStyleElement.CreateElement (
							SCROLLBAR,
							(int)SCROLLBARPARTS.SBP_ARROWBTN,
							(int)ARROWBTNSTATES.ABS_RIGHTPRESSED);
					}
				}
				public static VisualStyleElement UpDisabled {
					get {
						return VisualStyleElement.CreateElement (
							SCROLLBAR,
							(int)SCROLLBARPARTS.SBP_ARROWBTN,
							(int)ARROWBTNSTATES.ABS_UPDISABLED);
					}
				}
				public static VisualStyleElement UpHot {
					get {
						return VisualStyleElement.CreateElement (
							SCROLLBAR,
							(int)SCROLLBARPARTS.SBP_ARROWBTN,
							(int)ARROWBTNSTATES.ABS_UPHOT);
					}
				}
				public static VisualStyleElement UpNormal {
					get {
						return VisualStyleElement.CreateElement (
							SCROLLBAR,
							(int)SCROLLBARPARTS.SBP_ARROWBTN,
							(int)ARROWBTNSTATES.ABS_UPNORMAL);
					}
				}
				public static VisualStyleElement UpPressed {
					get {
						return VisualStyleElement.CreateElement (
							SCROLLBAR,
							(int)SCROLLBARPARTS.SBP_ARROWBTN,
							(int)ARROWBTNSTATES.ABS_UPPRESSED);
					}
				}
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
				public static VisualStyleElement Normal {
					get {
						return VisualStyleElement.CreateElement (
							SCROLLBAR,
							(int)SCROLLBARPARTS.SBP_GRIPPERHORZ,
							0);
					}
				}
			}
			public static class GripperVertical
			{
				public static VisualStyleElement Normal {
					get {
						return VisualStyleElement.CreateElement (
							VisualStyleElement.SCROLLBAR,
							(int)SCROLLBARPARTS.SBP_GRIPPERVERT,
							0);
					}
				}
			}
			public static class LeftTrackHorizontal
			{
				public static VisualStyleElement Disabled {
					get {
						return VisualStyleElement.CreateElement (
							SCROLLBAR,
							(int)SCROLLBARPARTS.SBP_UPPERTRACKHORZ,
							(int)SCROLLBARSTYLESTATES.SCRBS_DISABLED);
					}
				}
				public static VisualStyleElement Hot {
					get {
						return VisualStyleElement.CreateElement (
							SCROLLBAR,
							(int)SCROLLBARPARTS.SBP_UPPERTRACKHORZ,
							(int)SCROLLBARSTYLESTATES.SCRBS_HOT);
					}
				}
				public static VisualStyleElement Normal {
					get {
						return VisualStyleElement.CreateElement (
							SCROLLBAR,
							(int)SCROLLBARPARTS.SBP_UPPERTRACKHORZ,
							(int)SCROLLBARSTYLESTATES.SCRBS_NORMAL);
					}
				}
				public static VisualStyleElement Pressed {
					get {
						return VisualStyleElement.CreateElement (
							SCROLLBAR,
							(int)SCROLLBARPARTS.SBP_UPPERTRACKHORZ,
							(int)SCROLLBARSTYLESTATES.SCRBS_PRESSED);
					}
				}
			}
			public static class LowerTrackVertical
			{
				public static VisualStyleElement Disabled {
					get {
						return VisualStyleElement.CreateElement (
							SCROLLBAR,
							(int)SCROLLBARPARTS.SBP_LOWERTRACKVERT,
							(int)SCROLLBARSTYLESTATES.SCRBS_DISABLED);
					}
				}
				public static VisualStyleElement Hot {
					get {
						return VisualStyleElement.CreateElement (
							SCROLLBAR,
							(int)SCROLLBARPARTS.SBP_LOWERTRACKVERT,
							(int)SCROLLBARSTYLESTATES.SCRBS_HOT);
					}
				}
				public static VisualStyleElement Normal {
					get {
						return VisualStyleElement.CreateElement (
							SCROLLBAR,
							(int)SCROLLBARPARTS.SBP_LOWERTRACKVERT,
							(int)SCROLLBARSTYLESTATES.SCRBS_NORMAL);
					}
				}
				public static VisualStyleElement Pressed {
					get {
						return VisualStyleElement.CreateElement (
							SCROLLBAR,
							(int)SCROLLBARPARTS.SBP_LOWERTRACKVERT,
							(int)SCROLLBARSTYLESTATES.SCRBS_PRESSED);
					}
				}
			}
			public static class RightTrackHorizontal
			{
				public static VisualStyleElement Disabled {
					get {
						return VisualStyleElement.CreateElement (
							SCROLLBAR,
							(int)SCROLLBARPARTS.SBP_LOWERTRACKHORZ,
							(int)SCROLLBARSTYLESTATES.SCRBS_DISABLED);
					}
				}
				public static VisualStyleElement Hot {
					get {
						return VisualStyleElement.CreateElement (
							SCROLLBAR,
							(int)SCROLLBARPARTS.SBP_LOWERTRACKHORZ,
							(int)SCROLLBARSTYLESTATES.SCRBS_HOT);
					}
				}
				public static VisualStyleElement Normal {
					get {
						return VisualStyleElement.CreateElement (
							SCROLLBAR,
							(int)SCROLLBARPARTS.SBP_LOWERTRACKHORZ,
							(int)SCROLLBARSTYLESTATES.SCRBS_NORMAL);
					}
				}
				public static VisualStyleElement Pressed {
					get {
						return VisualStyleElement.CreateElement (
							SCROLLBAR,
							(int)SCROLLBARPARTS.SBP_LOWERTRACKHORZ,
							(int)SCROLLBARSTYLESTATES.SCRBS_PRESSED);
					}
				}
			}
			public static class SizeBox
			{
				public static VisualStyleElement LeftAlign {
					get {
						return VisualStyleElement.CreateElement (
							SCROLLBAR,
							(int)SCROLLBARPARTS.SBP_SIZEBOX,
							(int)SIZEBOXSTATES.SZB_LEFTALIGN);
					}
				}
				public static VisualStyleElement RightAlign {
					get {
						return VisualStyleElement.CreateElement (
							SCROLLBAR,
							(int)SCROLLBARPARTS.SBP_SIZEBOX,
							(int)SIZEBOXSTATES.SZB_RIGHTALIGN);
					}
				}
			}
			public static class ThumbButtonHorizontal
			{
				public static VisualStyleElement Disabled {
					get {
						return VisualStyleElement.CreateElement (
							SCROLLBAR,
							(int)SCROLLBARPARTS.SBP_THUMBBTNHORZ,
							(int)SCROLLBARSTYLESTATES.SCRBS_DISABLED);
					}
				}
				public static VisualStyleElement Hot {
					get {
						return VisualStyleElement.CreateElement (
							SCROLLBAR,
							(int)SCROLLBARPARTS.SBP_THUMBBTNHORZ,
							(int)SCROLLBARSTYLESTATES.SCRBS_HOT);
					}
				}
				public static VisualStyleElement Normal {
					get {
						return VisualStyleElement.CreateElement (
							SCROLLBAR,
							(int)SCROLLBARPARTS.SBP_THUMBBTNHORZ,
							1);
					}
				}
				public static VisualStyleElement Pressed {
					get {
						return VisualStyleElement.CreateElement (
							SCROLLBAR,
							(int)SCROLLBARPARTS.SBP_THUMBBTNHORZ,
							(int)SCROLLBARSTYLESTATES.SCRBS_PRESSED);
					}
				}
			}
			public static class ThumbButtonVertical
			{
				public static VisualStyleElement Disabled {
					get {
						return VisualStyleElement.CreateElement (
							SCROLLBAR,
							(int)SCROLLBARPARTS.SBP_THUMBBTNVERT,
							(int)SCROLLBARSTYLESTATES.SCRBS_DISABLED);
					}
				}
				public static VisualStyleElement Hot {
					get {
						return VisualStyleElement.CreateElement (
							SCROLLBAR,
							(int)SCROLLBARPARTS.SBP_THUMBBTNVERT,
							(int)SCROLLBARSTYLESTATES.SCRBS_HOT);
					}
				}
				public static VisualStyleElement Normal {
					get {
						return VisualStyleElement.CreateElement (
							SCROLLBAR,
							(int)SCROLLBARPARTS.SBP_THUMBBTNVERT,
							(int)SCROLLBARSTYLESTATES.SCRBS_NORMAL);
					}
				}
				public static VisualStyleElement Pressed {
					get {
						return VisualStyleElement.CreateElement (
							SCROLLBAR,
							(int)SCROLLBARPARTS.SBP_THUMBBTNVERT,
							(int)SCROLLBARSTYLESTATES.SCRBS_PRESSED);
					}
				}
			}
			public static class UpperTrackVertical
			{
				public static VisualStyleElement Disabled {
					get {
						return VisualStyleElement.CreateElement (
							SCROLLBAR,
							(int)SCROLLBARPARTS.SBP_UPPERTRACKVERT,
							(int)SCROLLBARSTYLESTATES.SCRBS_DISABLED);
					}
				}
				public static VisualStyleElement Hot {
					get {
						return VisualStyleElement.CreateElement (
							SCROLLBAR,
							(int)SCROLLBARPARTS.SBP_UPPERTRACKVERT,
							(int)SCROLLBARSTYLESTATES.SCRBS_HOT);
					}
				}
				public static VisualStyleElement Normal {
					get {
						return VisualStyleElement.CreateElement (
							SCROLLBAR,
							(int)SCROLLBARPARTS.SBP_UPPERTRACKVERT,
							(int)SCROLLBARSTYLESTATES.SCRBS_NORMAL);
					}
				}
				public static VisualStyleElement Pressed {
					get {
						return VisualStyleElement.CreateElement (
							SCROLLBAR,
							(int)SCROLLBARPARTS.SBP_UPPERTRACKVERT,
							(int)SCROLLBARSTYLESTATES.SCRBS_PRESSED);
					}
				}
			}
		}
		#endregion
		#region Spin
		public static class Spin
		{
			public static class Down
			{
				public static VisualStyleElement Disabled {
					get {
						return VisualStyleElement.CreateElement (
							SPIN,
							(int)SPINPARTS.SPNP_DOWN,
							(int)DOWNSTATES.DNS_DISABLED);
					}
				}
				public static VisualStyleElement Hot {
					get {
						return VisualStyleElement.CreateElement (
							SPIN,
							(int)SPINPARTS.SPNP_DOWN,
							(int)DOWNSTATES.DNS_HOT);
					}
				}
				public static VisualStyleElement Normal {
					get {
						return VisualStyleElement.CreateElement (
							SPIN,
							(int)SPINPARTS.SPNP_DOWN,
							(int)DOWNSTATES.DNS_NORMAL);
					}
				}
				public static VisualStyleElement Pressed {
					get {
						return VisualStyleElement.CreateElement (
							SPIN,
							(int)SPINPARTS.SPNP_DOWN,
							(int)DOWNSTATES.DNS_PRESSED);
					}
				}
			}
			public static class DownHorizontal
			{
				public static VisualStyleElement Disabled {
					get {
						return VisualStyleElement.CreateElement (
							SPIN,
							(int)SPINPARTS.SPNP_DOWNHORZ,
							(int)DOWNHORZSTATES.DNHZS_DISABLED);
					}
				}
				public static VisualStyleElement Hot {
					get {
						return VisualStyleElement.CreateElement (
							SPIN,
							(int)SPINPARTS.SPNP_DOWNHORZ,
							(int)DOWNHORZSTATES.DNHZS_HOT);
					}
				}
				public static VisualStyleElement Normal {
					get {
						return VisualStyleElement.CreateElement (
							SPIN,
							(int)SPINPARTS.SPNP_DOWNHORZ,
							(int)DOWNHORZSTATES.DNHZS_NORMAL);
					}
				}
				public static VisualStyleElement Pressed {
					get {
						return VisualStyleElement.CreateElement (
							SPIN,
							(int)SPINPARTS.SPNP_DOWNHORZ,
							(int)DOWNHORZSTATES.DNHZS_PRESSED);
					}
				}
			}
			public static class Up
			{
				public static VisualStyleElement Disabled {
					get {
						return VisualStyleElement.CreateElement (
							SPIN,
							(int)SPINPARTS.SPNP_UP,
							(int)UPSTATES.UPS_DISABLED);
					}
				}
				public static VisualStyleElement Hot {
					get {
						return VisualStyleElement.CreateElement (
							SPIN,
							(int)SPINPARTS.SPNP_UP,
							(int)UPSTATES.UPS_HOT);
					}
				}
				public static VisualStyleElement Normal {
					get {
						return VisualStyleElement.CreateElement (
							SPIN,
							(int)SPINPARTS.SPNP_UP,
							(int)UPSTATES.UPS_NORMAL);
					}
				}
				public static VisualStyleElement Pressed {
					get {
						return VisualStyleElement.CreateElement (
							SPIN,
							(int)SPINPARTS.SPNP_UP,
							(int)UPSTATES.UPS_PRESSED);
					}
				}
			}
			public static class UpHorizontal
			{
				public static VisualStyleElement Disabled {
					get {
						return VisualStyleElement.CreateElement (
							SPIN,
							(int)SPINPARTS.SPNP_UPHORZ,
							(int)UPHORZSTATES.UPHZS_DISABLED);
					}
				}
				public static VisualStyleElement Hot {
					get {
						return VisualStyleElement.CreateElement (
							SPIN,
							(int)SPINPARTS.SPNP_UPHORZ,
							(int)UPHORZSTATES.UPHZS_HOT);
					}
				}
				public static VisualStyleElement Normal {
					get {
						return VisualStyleElement.CreateElement (
							SPIN,
							(int)SPINPARTS.SPNP_UPHORZ,
							(int)UPHORZSTATES.UPHZS_NORMAL);
					}
				}
				public static VisualStyleElement Pressed {
					get {
						return VisualStyleElement.CreateElement (
							SPIN,
							(int)SPINPARTS.SPNP_UPHORZ,
							(int)UPHORZSTATES.UPHZS_PRESSED);
					}
				}
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
				public static VisualStyleElement Normal {
					get {
						return VisualStyleElement.CreateElement (
							STATUS,
							(int)STATUSPARTS.SP_GRIPPER,
							0);
					}
				}
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
				public static VisualStyleElement Normal {
					get {
						return VisualStyleElement.CreateElement (
							TAB,
							(int)TABPARTS.TABP_BODY,
							0);
					}
				}
			}
			public static class Pane
			{
				public static VisualStyleElement Normal {
					get {
						return VisualStyleElement.CreateElement (
							TAB,
							(int)TABPARTS.TABP_PANE,
							0);
					}
				}
			}
			public static class TabItem
			{
				public static VisualStyleElement Disabled {
					get {
						return VisualStyleElement.CreateElement (
							TAB,
							(int)TABPARTS.TABP_TABITEM,
							(int)TABITEMSTATES.TIS_DISABLED);
					}
				}
				public static VisualStyleElement Hot {
					get {
						return VisualStyleElement.CreateElement (
							TAB,
							(int)TABPARTS.TABP_TABITEM,
							(int)TABITEMSTATES.TIS_HOT);
					}
				}
				public static VisualStyleElement Normal {
					get {
						return VisualStyleElement.CreateElement (
							TAB,
							(int)TABPARTS.TABP_TABITEM,
							(int)TABITEMSTATES.TIS_NORMAL);
					}
				}
				public static VisualStyleElement Pressed {
					get {
						return VisualStyleElement.CreateElement (
							TAB,
							(int)TABPARTS.TABP_TABITEM,
							(int)TABITEMSTATES.TIS_SELECTED);
					}
				}
			}
			public static class TabItemBothEdges
			{
				public static VisualStyleElement Normal {
					get {
						return VisualStyleElement.CreateElement (
							TAB,
							(int)TABPARTS.TABP_TABITEMBOTHEDGE,
							0);
					}
				}
			}
			public static class TabItemLeftEdge
			{
				public static VisualStyleElement Disabled {
					get {
						return VisualStyleElement.CreateElement (
							TAB,
							(int)TABPARTS.TABP_TABITEMLEFTEDGE,
							(int)TABITEMLEFTEDGESTATES.TILES_DISABLED);
					}
				}
				public static VisualStyleElement Hot {
					get {
						return VisualStyleElement.CreateElement (
							TAB,
							(int)TABPARTS.TABP_TABITEMLEFTEDGE,
							(int)TABITEMLEFTEDGESTATES.TILES_HOT);
					}
				}
				public static VisualStyleElement Normal {
					get {
						return VisualStyleElement.CreateElement (
							TAB,
							(int)TABPARTS.TABP_TABITEMLEFTEDGE,
							(int)TABITEMLEFTEDGESTATES.TILES_NORMAL);
					}
				}
				public static VisualStyleElement Pressed {
					get {
						return VisualStyleElement.CreateElement (
							TAB,
							(int)TABPARTS.TABP_TABITEMLEFTEDGE,
							(int)TABITEMLEFTEDGESTATES.TILES_SELECTED);
					}
				}
			}
			public static class TabItemRightEdge
			{
				public static VisualStyleElement Disabled {
					get {
						return VisualStyleElement.CreateElement (
							TAB,
							(int)TABPARTS.TABP_TABITEMRIGHTEDGE,
							(int)TABITEMRIGHTEDGESTATES.TIRES_DISABLED);
					}
				}
				public static VisualStyleElement Hot {
					get {
						return VisualStyleElement.CreateElement (
							TAB,
							(int)TABPARTS.TABP_TABITEMRIGHTEDGE,
							(int)TABITEMRIGHTEDGESTATES.TIRES_HOT);
					}
				}
				public static VisualStyleElement Normal {
					get {
						return VisualStyleElement.CreateElement (
							TAB,
							(int)TABPARTS.TABP_TABITEMRIGHTEDGE,
							(int)TABITEMRIGHTEDGESTATES.TIRES_NORMAL);
					}
				}
				public static VisualStyleElement Pressed {
					get {
						return VisualStyleElement.CreateElement (
							TAB,
							(int)TABPARTS.TABP_TABITEMRIGHTEDGE,
							(int)TABITEMRIGHTEDGESTATES.TIRES_SELECTED);
					}
				}
			}
			public static class TopTabItem
			{
				public static VisualStyleElement Disabled {
					get {
						return VisualStyleElement.CreateElement (
							TAB,
							(int)TABPARTS.TABP_TOPTABITEM,
							(int)TOPTABITEMSTATES.TTIS_DISABLED);
					}
				}
				public static VisualStyleElement Hot {
					get {
						return VisualStyleElement.CreateElement (
							TAB,
							(int)TABPARTS.TABP_TOPTABITEM,
							(int)TOPTABITEMSTATES.TTIS_HOT);
					}
				}
				public static VisualStyleElement Normal {
					get {
						return VisualStyleElement.CreateElement (
							TAB,
							(int)TABPARTS.TABP_TOPTABITEM,
							(int)TOPTABITEMSTATES.TTIS_NORMAL);
					}
				}
				public static VisualStyleElement Pressed {
					get {
						return VisualStyleElement.CreateElement (
							TAB,
							(int)TABPARTS.TABP_TOPTABITEM,
							(int)TOPTABITEMSTATES.TTIS_SELECTED);
					}
				}
			}
			public static class TopTabItemBothEdges
			{
				public static VisualStyleElement Normal {
					get {
						return VisualStyleElement.CreateElement (
							TAB,
							(int)TABPARTS.TABP_TOPTABITEMBOTHEDGE,
							0);
					}
				}
			}
			public static class TopTabItemLeftEdge
			{
				public static VisualStyleElement Disabled {
					get {
						return VisualStyleElement.CreateElement (
							TAB,
							(int)TABPARTS.TABP_TOPTABITEMLEFTEDGE,
							(int)TOPTABITEMLEFTEDGESTATES.TTILES_DISABLED);
					}
				}
				public static VisualStyleElement Hot {
					get {
						return VisualStyleElement.CreateElement (
							TAB,
							(int)TABPARTS.TABP_TOPTABITEMLEFTEDGE,
							(int)TOPTABITEMLEFTEDGESTATES.TTILES_HOT);
					}
				}
				public static VisualStyleElement Normal {
					get {
						return VisualStyleElement.CreateElement (
							TAB,
							(int)TABPARTS.TABP_TOPTABITEMLEFTEDGE,
							(int)TOPTABITEMLEFTEDGESTATES.TTILES_NORMAL);
					}
				}
				public static VisualStyleElement Pressed {
					get {
						return VisualStyleElement.CreateElement (
							TAB,
							(int)TABPARTS.TABP_TOPTABITEMLEFTEDGE,
							(int)TOPTABITEMLEFTEDGESTATES.TTILES_SELECTED);
					}
				}
			}
			public static class TopTabItemRightEdge
			{
				public static VisualStyleElement Disabled {
					get {
						return VisualStyleElement.CreateElement (
							TAB,
							(int)TABPARTS.TABP_TOPTABITEMRIGHTEDGE,
							(int)TOPTABITEMRIGHTEDGESTATES.TTIRES_DISABLED);
					}
				}
				public static VisualStyleElement Hot {
					get {
						return VisualStyleElement.CreateElement (
							TAB,
							(int)TABPARTS.TABP_TOPTABITEMRIGHTEDGE,
							(int)TOPTABITEMRIGHTEDGESTATES.TTIRES_HOT);
					}
				}
				public static VisualStyleElement Normal {
					get {
						return VisualStyleElement.CreateElement (
							TAB,
							(int)TABPARTS.TABP_TOPTABITEMRIGHTEDGE,
							(int)TOPTABITEMRIGHTEDGESTATES.TTIRES_NORMAL);
					}
				}
				public static VisualStyleElement Pressed {
					get {
						return VisualStyleElement.CreateElement (
							TAB,
							(int)TABPARTS.TABP_TOPTABITEMRIGHTEDGE,
							(int)TOPTABITEMRIGHTEDGESTATES.TTIRES_SELECTED);
					}
				}
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
				public static VisualStyleElement Assist {
					get {
						return VisualStyleElement.CreateElement (
							EDIT,
							(int)EDITPARTS.EP_EDITTEXT,
							(int)EDITTEXTSTATES.ETS_ASSIST);
					}
				}
				public static VisualStyleElement Disabled {
					get {
						return VisualStyleElement.CreateElement (
							EDIT,
							(int)EDITPARTS.EP_EDITTEXT,
							(int)EDITTEXTSTATES.ETS_DISABLED);
					}
				}
				public static VisualStyleElement Focused {
					get {
						return VisualStyleElement.CreateElement (
							EDIT,
							(int)EDITPARTS.EP_EDITTEXT,
							(int)EDITTEXTSTATES.ETS_FOCUSED);
					}
				}
				public static VisualStyleElement Hot {
					get {
						return VisualStyleElement.CreateElement (
							EDIT,
							(int)EDITPARTS.EP_EDITTEXT,
							(int)EDITTEXTSTATES.ETS_HOT);
					}
				}
				public static VisualStyleElement Normal {
					get {
						return VisualStyleElement.CreateElement (
							EDIT,
							(int)EDITPARTS.EP_EDITTEXT,
							(int)EDITTEXTSTATES.ETS_NORMAL);
					}
				}
				public static VisualStyleElement ReadOnly {
					get {
						return VisualStyleElement.CreateElement (
							EDIT,
							(int)EDITPARTS.EP_EDITTEXT,
							(int)EDITTEXTSTATES.ETS_READONLY);
					}
				}
				public static VisualStyleElement Selected {
					get {
						return VisualStyleElement.CreateElement (
							EDIT,
							(int)EDITPARTS.EP_EDITTEXT,
							(int)EDITTEXTSTATES.ETS_SELECTED);
					}
				}
			}
		}
		#endregion
		#region ToolBar
		public static class ToolBar
		{
			public static class Button
			{
				public static VisualStyleElement Checked {
					get {
						return VisualStyleElement.CreateElement (
							TOOLBAR,
							(int)TOOLBARPARTS.TP_BUTTON,
							(int)TOOLBARSTYLESTATES.TS_CHECKED);
					}
				}
				public static VisualStyleElement Disabled {
					get {
						return VisualStyleElement.CreateElement (
							TOOLBAR,
							(int)TOOLBARPARTS.TP_BUTTON,
							(int)TOOLBARSTYLESTATES.TS_DISABLED);
					}
				}
				public static VisualStyleElement Hot {
					get {
						return VisualStyleElement.CreateElement (
							TOOLBAR,
							(int)TOOLBARPARTS.TP_BUTTON,
							(int)TOOLBARSTYLESTATES.TS_HOT);
					}
				}
				public static VisualStyleElement HotChecked {
					get {
						return VisualStyleElement.CreateElement (
							TOOLBAR,
							(int)TOOLBARPARTS.TP_BUTTON,
							(int)TOOLBARSTYLESTATES.TS_HOTCHECKED);
					}
				}
				public static VisualStyleElement Normal {
					get {
						return VisualStyleElement.CreateElement (
							TOOLBAR,
							(int)TOOLBARPARTS.TP_BUTTON,
							(int)TOOLBARSTYLESTATES.TS_NORMAL);
					}
				}
				public static VisualStyleElement Pressed {
					get {
						return VisualStyleElement.CreateElement (
							TOOLBAR,
							(int)TOOLBARPARTS.TP_BUTTON,
							(int)TOOLBARSTYLESTATES.TS_PRESSED);
					}
				}
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
				public static VisualStyleElement Disabled {
					get {
						return VisualStyleElement.CreateElement (
							TRACKBAR,
							(int)TRACKBARPARTS.TKP_THUMB,
							(int)THUMBSTATES.TUS_DISABLED);
					}
				}
				public static VisualStyleElement Focused {
					get {
						return VisualStyleElement.CreateElement (
							TRACKBAR,
							(int)TRACKBARPARTS.TKP_THUMB,
							(int)THUMBSTATES.TUS_FOCUSED);
					}
				}
				public static VisualStyleElement Hot {
					get {
						return VisualStyleElement.CreateElement (
							TRACKBAR,
							(int)TRACKBARPARTS.TKP_THUMB,
							(int)THUMBSTATES.TUS_HOT);
					}
				}
				public static VisualStyleElement Normal {
					get {
						return VisualStyleElement.CreateElement (
							TRACKBAR,
							(int)TRACKBARPARTS.TKP_THUMB,
							(int)THUMBSTATES.TUS_NORMAL);
					}
				}
				public static VisualStyleElement Pressed {
					get {
						return VisualStyleElement.CreateElement (
							TRACKBAR,
							(int)TRACKBARPARTS.TKP_THUMB,
							(int)THUMBSTATES.TUS_PRESSED);
					}
				}
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
				public static VisualStyleElement Disabled {
					get {
						return VisualStyleElement.CreateElement (
							TRACKBAR,
							(int)TRACKBARPARTS.TKP_THUMBVERT,
							(int)THUMBVERTSTATES.TUVS_DISABLED);
					}
				}
				public static VisualStyleElement Focused {
					get {
						return VisualStyleElement.CreateElement (
							TRACKBAR,
							(int)TRACKBARPARTS.TKP_THUMBVERT,
							(int)THUMBVERTSTATES.TUVS_FOCUSED);
					}
				}
				public static VisualStyleElement Hot {
					get {
						return VisualStyleElement.CreateElement (
							TRACKBAR,
							(int)TRACKBARPARTS.TKP_THUMBVERT,
							(int)THUMBVERTSTATES.TUVS_HOT);
					}
				}
				public static VisualStyleElement Normal {
					get {
						return VisualStyleElement.CreateElement (
							TRACKBAR,
							(int)TRACKBARPARTS.TKP_THUMBVERT,
							(int)THUMBVERTSTATES.TUVS_NORMAL);
					}
				}
				public static VisualStyleElement Pressed {
					get {
						return VisualStyleElement.CreateElement (
							TRACKBAR,
							(int)TRACKBARPARTS.TKP_THUMBVERT,
							(int)THUMBVERTSTATES.TUVS_PRESSED);
					}
				}
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
				public static VisualStyleElement Normal {
					get {
						return VisualStyleElement.CreateElement (
							TRACKBAR,
							(int)TRACKBARPARTS.TKP_TRACK,
							(int)TRACKSTATES.TRS_NORMAL);
					}
				}
			}
			public static class TrackVertical
			{
				public static VisualStyleElement Normal {
					get {
						return VisualStyleElement.CreateElement (
							TRACKBAR,
							(int)TRACKBARPARTS.TKP_TRACKVERT,
							(int)TRACKVERTSTATES.TRVS_NORMAL);
					}
				}
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
				public static VisualStyleElement Closed {
					get {
						return VisualStyleElement.CreateElement (
							TREEVIEW,
							(int)TREEVIEWPARTS.TVP_GLYPH,
							(int)GLYPHSTATES.GLPS_CLOSED);
					}
				}
				public static VisualStyleElement Opened {
					get {
						return VisualStyleElement.CreateElement (
							TREEVIEW,
							(int)TREEVIEWPARTS.TVP_GLYPH,
							(int)GLYPHSTATES.GLPS_OPENED);
					}
				}
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
	#region Part and state constants
	#region BUTTON
	enum BUTTONPARTS
	{
		BP_PUSHBUTTON = 1,
		BP_RADIOBUTTON,
		BP_CHECKBOX,
		BP_GROUPBOX
	}
	enum PUSHBUTTONSTATES
	{
		PBS_NORMAL = 1,
		PBS_HOT,
		PBS_PRESSED,
		PBS_DISABLED,
		PBS_DEFAULTED
	}
	enum RADIOBUTTONSTATES
	{
		RBS_UNCHECKEDNORMAL = 1,
		RBS_UNCHECKEDHOT,
		RBS_UNCHECKEDPRESSED,
		RBS_UNCHECKEDDISABLED,
		RBS_CHECKEDNORMAL,
		RBS_CHECKEDHOT,
		RBS_CHECKEDPRESSED,
		RBS_CHECKEDDISABLED
	}
	enum CHECKBOXSTATES
	{
		CBS_UNCHECKEDNORMAL = 1,
		CBS_UNCHECKEDHOT,
		CBS_UNCHECKEDPRESSED,
		CBS_UNCHECKEDDISABLED,
		CBS_CHECKEDNORMAL,
		CBS_CHECKEDHOT,
		CBS_CHECKEDPRESSED,
		CBS_CHECKEDDISABLED,
		CBS_MIXEDNORMAL,
		CBS_MIXEDHOT,
		CBS_MIXEDPRESSED,
		CBS_MIXEDDISABLED
	}
	enum GROUPBOXSTATES
	{
		GBS_NORMAL = 1,
		GBS_DISABLED
	}
	#endregion
	#region COMBOXBOX
	enum COMBOBOXPARTS
	{
		CP_DROPDOWNBUTTON = 1,
		CP_BORDER = 4
	}
	enum COMBOBOXSTYLESTATES
	{
		CBXS_NORMAL = 1,
		CBXS_HOT,
		CBXS_PRESSED,
		CBXS_DISABLED
	}
	enum BORDERSTATES
	{
		CBB_NORMAL = 1,
		CBB_HOT,
		CBB_FOCUSED,
		CBB_DISABLED
	}
	#endregion
	#region EDIT
	enum EDITPARTS
	{
		EP_EDITTEXT = 1
	}
	enum EDITTEXTSTATES {
		ETS_NORMAL = 1,
		ETS_HOT,
		ETS_SELECTED,
		ETS_DISABLED,
		ETS_FOCUSED,
		ETS_READONLY,
		ETS_ASSIST
	}
	#endregion
	#region HEADER
	enum HEADERPARTS
	{
		HP_HEADERITEM = 1
	}
	enum HEADERITEMSTATES
	{
		HIS_NORMAL = 1,
		HIS_HOT,
		HIS_PRESSED
	}
	#endregion
	#region PROGRESS
	enum PROGRESSPARTS
	{
		PP_BAR = 1,
		PP_BARVERT,
		PP_CHUNK,
		PP_CHUNKVERT
	}
	#endregion
	#region REBAR
	enum REBARPARTS
	{
		RP_BAND = 3
	}
	#endregion
	#region SCROLLBAR
	enum SCROLLBARPARTS
	{
		SBP_ARROWBTN = 1,
		SBP_THUMBBTNHORZ,
		SBP_THUMBBTNVERT,
		SBP_LOWERTRACKHORZ,
		SBP_UPPERTRACKHORZ,
		SBP_LOWERTRACKVERT,
		SBP_UPPERTRACKVERT,
		SBP_GRIPPERHORZ,
		SBP_GRIPPERVERT,
		SBP_SIZEBOX
	}
	enum ARROWBTNSTATES
	{
		ABS_UPNORMAL = 1,
		ABS_UPHOT,
		ABS_UPPRESSED,
		ABS_UPDISABLED,
		ABS_DOWNNORMAL,
		ABS_DOWNHOT,
		ABS_DOWNPRESSED,
		ABS_DOWNDISABLED,
		ABS_LEFTNORMAL,
		ABS_LEFTHOT,
		ABS_LEFTPRESSED,
		ABS_LEFTDISABLED,
		ABS_RIGHTNORMAL,
		ABS_RIGHTHOT,
		ABS_RIGHTPRESSED,
		ABS_RIGHTDISABLED,
		ABS_UPHOVER,
		ABS_DOWNHOVER,
		ABS_LEFTHOVER,
		ABS_RIGHTHOVER
	}
	enum SCROLLBARSTYLESTATES
	{
		SCRBS_NORMAL = 1,
		SCRBS_HOT,
		SCRBS_PRESSED,
		SCRBS_DISABLED
	}
	enum SIZEBOXSTATES
	{
		SZB_RIGHTALIGN = 1,
		SZB_LEFTALIGN
	}
	#endregion
	#region SPIN
	enum SPINPARTS
	{
		SPNP_UP = 1,
		SPNP_DOWN,
		SPNP_UPHORZ,
		SPNP_DOWNHORZ
	}
	enum UPSTATES
	{
		UPS_NORMAL = 1,
		UPS_HOT,
		UPS_PRESSED,
		UPS_DISABLED
	}
	enum DOWNSTATES
	{
		DNS_NORMAL = 1,
		DNS_HOT,
		DNS_PRESSED,
		DNS_DISABLED
	}
	enum UPHORZSTATES
	{
		UPHZS_NORMAL = 1,
		UPHZS_HOT,
		UPHZS_PRESSED,
		UPHZS_DISABLED
	}
	enum DOWNHORZSTATES
	{
		DNHZS_NORMAL = 1,
		DNHZS_HOT,
		DNHZS_PRESSED,
		DNHZS_DISABLED
	}
	#endregion
	#region STATUS
	enum STATUSPARTS
	{
		SP_GRIPPER = 3
	}
	#endregion
	#region TAB
	enum TABPARTS
	{
		TABP_TABITEM = 1,
		TABP_TABITEMLEFTEDGE,
		TABP_TABITEMRIGHTEDGE,
		TABP_TABITEMBOTHEDGE,
		TABP_TOPTABITEM,
		TABP_TOPTABITEMLEFTEDGE,
		TABP_TOPTABITEMRIGHTEDGE,
		TABP_TOPTABITEMBOTHEDGE,
		TABP_PANE,
		TABP_BODY
	}
	enum TABITEMSTATES
	{
		TIS_NORMAL = 1,
		TIS_HOT,
		TIS_SELECTED,
		TIS_DISABLED
	}
	enum TABITEMLEFTEDGESTATES
	{
		TILES_NORMAL = 1,
		TILES_HOT,
		TILES_SELECTED,
		TILES_DISABLED
	}
	enum TABITEMRIGHTEDGESTATES
	{
		TIRES_NORMAL = 1,
		TIRES_HOT,
		TIRES_SELECTED,
		TIRES_DISABLED
	}
	enum TOPTABITEMSTATES
	{
		TTIS_NORMAL = 1,
		TTIS_HOT,
		TTIS_SELECTED,
		TTIS_DISABLED
	}
	enum TOPTABITEMLEFTEDGESTATES
	{
		TTILES_NORMAL = 1,
		TTILES_HOT,
		TTILES_SELECTED,
		TTILES_DISABLED
	}
	enum TOPTABITEMRIGHTEDGESTATES
	{
		TTIRES_NORMAL = 1,
		TTIRES_HOT,
		TTIRES_SELECTED,
		TTIRES_DISABLED
	}
	#endregion
	#region TOOLBAR
	enum TOOLBARPARTS
	{
		TP_BUTTON = 1
	}
	enum TOOLBARSTYLESTATES
	{
		TS_NORMAL = 1,
		TS_HOT,
		TS_PRESSED,
		TS_DISABLED,
		TS_CHECKED,
		TS_HOTCHECKED
	}
	#endregion
	#region TRACKBAR
	enum TRACKBARPARTS
	{
		TKP_TRACK = 1,
		TKP_TRACKVERT,
		TKP_THUMB,
		TKP_THUMBVERT = 6
	}
	enum TRACKSTATES
	{
		TRS_NORMAL = 1
	}
	enum TRACKVERTSTATES
	{
		TRVS_NORMAL = 1
	}
	enum THUMBSTATES
	{
		TUS_NORMAL = 1,
		TUS_HOT,
		TUS_PRESSED,
		TUS_FOCUSED,
		TUS_DISABLED
	}
	enum THUMBVERTSTATES
	{
		TUVS_NORMAL = 1,
		TUVS_HOT,
		TUVS_PRESSED,
		TUVS_FOCUSED,
		TUVS_DISABLED
	}
	#endregion
	#region TREEVIEW
	enum TREEVIEWPARTS
	{
		TVP_GLYPH = 2
	}
	enum GLYPHSTATES
	{
		GLPS_CLOSED = 1,
		GLPS_OPENED
	}
	#endregion
	#endregion
}
