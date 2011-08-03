//
// VisualStylesGtkPlus.cs: IVisualStyles that uses GtkPlus.
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
// Copyright (c) 2008 George Giolfan
//
// Authors:
//	George Giolfan (georgegiolfan@yahoo.com)
//

using System.Drawing;
using System.Collections.Generic;
namespace System.Windows.Forms.VisualStyles
{
	class VisualStylesGtkPlus : IVisualStyles
	{
		public static bool Initialize ()
		{
			return GtkPlus.Initialize ();
		}
		static GtkPlus GtkPlus {
			get {
				return GtkPlus.Instance;
			}
		}

		enum S {
			S_OK,
			S_FALSE
		}
		enum ThemeHandle {
			BUTTON = 1,
			COMBOBOX,
			EDIT,
			HEADER,
			PROGRESS,
			REBAR,
			SCROLLBAR,
			SPIN,
			STATUS,
			TAB,
			TOOLBAR,
			TRACKBAR,
			TREEVIEW
		}

		#region UxTheme
		public int UxThemeCloseThemeData (IntPtr hTheme)
		{
#if DEBUG
			return (int)((Enum.IsDefined (typeof (ThemeHandle), (int)hTheme)) ? S.S_OK : S.S_FALSE);
#else
			return (int)S.S_OK;
#endif
		}
		public int UxThemeDrawThemeParentBackground (IDeviceContext dc, Rectangle bounds, Control childControl)
		{
			return (int)S.S_FALSE;
		}
		public int UxThemeDrawThemeBackground (IntPtr hTheme, IDeviceContext dc, int iPartId, int iStateId, Rectangle bounds, Rectangle clipRectangle)
		{
			return (int)(DrawBackground ((ThemeHandle)(int)hTheme, dc, iPartId, iStateId, bounds, clipRectangle, Rectangle.Empty) ? S.S_OK : S.S_FALSE);
		}
		public int UxThemeDrawThemeBackground (IntPtr hTheme, IDeviceContext dc, int iPartId, int iStateId, Rectangle bounds)
		{
			return UxThemeDrawThemeBackground (hTheme, dc, iPartId, iStateId, bounds, bounds);
		}
		bool DrawBackground (ThemeHandle themeHandle, IDeviceContext dc, int part, int state, Rectangle bounds, Rectangle clipRectangle, Rectangle excludedArea) {
			GtkPlusState gtk_plus_state;
			GtkPlusToggleButtonValue gtk_plus_toggle_button_value;
			switch (themeHandle) {
			#region BUTTON
			case ThemeHandle.BUTTON:
				switch ((BUTTONPARTS)part) {
				#region BP_PUSHBUTTON
				case BUTTONPARTS.BP_PUSHBUTTON:
					switch ((PUSHBUTTONSTATES)state) {
					case PUSHBUTTONSTATES.PBS_NORMAL:
						gtk_plus_state = GtkPlusState.Normal;
						break;
					case PUSHBUTTONSTATES.PBS_HOT:
						gtk_plus_state = GtkPlusState.Hot;
						break;
					case PUSHBUTTONSTATES.PBS_PRESSED:
						gtk_plus_state = GtkPlusState.Pressed;
						break;
					case PUSHBUTTONSTATES.PBS_DISABLED:
						gtk_plus_state = GtkPlusState.Disabled;
						break;
					case PUSHBUTTONSTATES.PBS_DEFAULTED:
						gtk_plus_state = GtkPlusState.Normal;
						break;
					default:
						return false;
					}
					GtkPlus.ButtonPaint (dc, bounds, clipRectangle, (PUSHBUTTONSTATES)state == PUSHBUTTONSTATES.PBS_DEFAULTED, gtk_plus_state);
					return true;
				#endregion
				#region BP_RADIOBUTTON
				case BUTTONPARTS.BP_RADIOBUTTON:
					switch ((RADIOBUTTONSTATES)state) {
					case RADIOBUTTONSTATES.RBS_UNCHECKEDNORMAL:
						gtk_plus_state = GtkPlusState.Normal;
						gtk_plus_toggle_button_value = GtkPlusToggleButtonValue.Unchecked;
						break;
					case RADIOBUTTONSTATES.RBS_UNCHECKEDPRESSED:
						gtk_plus_state = GtkPlusState.Pressed;
						gtk_plus_toggle_button_value = GtkPlusToggleButtonValue.Unchecked;
						break;
					case RADIOBUTTONSTATES.RBS_UNCHECKEDHOT:
						gtk_plus_state = GtkPlusState.Hot;
						gtk_plus_toggle_button_value = GtkPlusToggleButtonValue.Unchecked;
						break;
					case RADIOBUTTONSTATES.RBS_UNCHECKEDDISABLED:
						gtk_plus_state = GtkPlusState.Disabled;
						gtk_plus_toggle_button_value = GtkPlusToggleButtonValue.Unchecked;
						break;
					case RADIOBUTTONSTATES.RBS_CHECKEDNORMAL:
						gtk_plus_state = GtkPlusState.Normal;
						gtk_plus_toggle_button_value = GtkPlusToggleButtonValue.Checked;
						break;
					case RADIOBUTTONSTATES.RBS_CHECKEDPRESSED:
						gtk_plus_state = GtkPlusState.Pressed;
						gtk_plus_toggle_button_value = GtkPlusToggleButtonValue.Checked;
						break;
					case RADIOBUTTONSTATES.RBS_CHECKEDHOT:
						gtk_plus_state = GtkPlusState.Hot;
						gtk_plus_toggle_button_value = GtkPlusToggleButtonValue.Checked;
						break;
					case RADIOBUTTONSTATES.RBS_CHECKEDDISABLED:
						gtk_plus_state = GtkPlusState.Disabled;
						gtk_plus_toggle_button_value = GtkPlusToggleButtonValue.Checked;
						break;
					default:
						return false;
					}
					GtkPlus.RadioButtonPaint (dc, bounds, clipRectangle, gtk_plus_state, gtk_plus_toggle_button_value);
					return true;
				#endregion
				#region BP_CHECKBOX
				case BUTTONPARTS.BP_CHECKBOX:
					switch ((CHECKBOXSTATES)state) {
					case CHECKBOXSTATES.CBS_UNCHECKEDNORMAL:
						gtk_plus_state = GtkPlusState.Normal;
						gtk_plus_toggle_button_value = GtkPlusToggleButtonValue.Unchecked;
						break;
					case CHECKBOXSTATES.CBS_UNCHECKEDPRESSED:
						gtk_plus_state = GtkPlusState.Pressed;
						gtk_plus_toggle_button_value = GtkPlusToggleButtonValue.Unchecked;
						break;
					case CHECKBOXSTATES.CBS_UNCHECKEDHOT:
						gtk_plus_state = GtkPlusState.Hot;
						gtk_plus_toggle_button_value = GtkPlusToggleButtonValue.Unchecked;
						break;
					case CHECKBOXSTATES.CBS_UNCHECKEDDISABLED:
						gtk_plus_state = GtkPlusState.Disabled;
						gtk_plus_toggle_button_value = GtkPlusToggleButtonValue.Unchecked;
						break;
					case CHECKBOXSTATES.CBS_CHECKEDNORMAL:
						gtk_plus_state = GtkPlusState.Normal;
						gtk_plus_toggle_button_value = GtkPlusToggleButtonValue.Checked;
						break;
					case CHECKBOXSTATES.CBS_CHECKEDPRESSED:
						gtk_plus_state = GtkPlusState.Pressed;
						gtk_plus_toggle_button_value = GtkPlusToggleButtonValue.Checked;
						break;
					case CHECKBOXSTATES.CBS_CHECKEDHOT:
						gtk_plus_state = GtkPlusState.Hot;
						gtk_plus_toggle_button_value = GtkPlusToggleButtonValue.Checked;
						break;
					case CHECKBOXSTATES.CBS_CHECKEDDISABLED:
						gtk_plus_state = GtkPlusState.Disabled;
						gtk_plus_toggle_button_value = GtkPlusToggleButtonValue.Checked;
						break;
					case CHECKBOXSTATES.CBS_MIXEDNORMAL:
						gtk_plus_state = GtkPlusState.Normal;
						gtk_plus_toggle_button_value = GtkPlusToggleButtonValue.Mixed;
						break;
					case CHECKBOXSTATES.CBS_MIXEDPRESSED:
						gtk_plus_state = GtkPlusState.Pressed;
						gtk_plus_toggle_button_value = GtkPlusToggleButtonValue.Mixed;
						break;
					case CHECKBOXSTATES.CBS_MIXEDHOT:
						gtk_plus_state = GtkPlusState.Hot;
						gtk_plus_toggle_button_value = GtkPlusToggleButtonValue.Mixed;
						break;
					case CHECKBOXSTATES.CBS_MIXEDDISABLED:
						gtk_plus_state = GtkPlusState.Disabled;
						gtk_plus_toggle_button_value = GtkPlusToggleButtonValue.Mixed;
						break;
					default:
						return false;
					}
					GtkPlus.CheckBoxPaint (dc, bounds, clipRectangle, gtk_plus_state, gtk_plus_toggle_button_value);
					return true;
				#endregion
				#region BP_GROUPBOX
				case BUTTONPARTS.BP_GROUPBOX:
					switch ((GROUPBOXSTATES)state) {
					case GROUPBOXSTATES.GBS_NORMAL:
						gtk_plus_state = GtkPlusState.Normal;
						break;
					case GROUPBOXSTATES.GBS_DISABLED:
						gtk_plus_state = GtkPlusState.Disabled;
						break;
					default:
						return false;
					}
					GtkPlus.GroupBoxPaint (dc, bounds, excludedArea, gtk_plus_state);
					return true;
				#endregion
				default: return false;
				}
			#endregion
			#region COMBOBOX
			case ThemeHandle.COMBOBOX:
				switch ((COMBOBOXPARTS)part) {
				#region CP_DROPDOWNBUTTON
				case COMBOBOXPARTS.CP_DROPDOWNBUTTON:
					switch ((COMBOBOXSTYLESTATES)state) {
					case COMBOBOXSTYLESTATES.CBXS_NORMAL: gtk_plus_state = GtkPlusState.Normal; break;
					case COMBOBOXSTYLESTATES.CBXS_HOT: gtk_plus_state = GtkPlusState.Hot; break;
					case COMBOBOXSTYLESTATES.CBXS_PRESSED: gtk_plus_state = GtkPlusState.Pressed; break;
					case COMBOBOXSTYLESTATES.CBXS_DISABLED: gtk_plus_state = GtkPlusState.Disabled; break;
					default: return false;
					}
					GtkPlus.ComboBoxPaintDropDownButton (dc, bounds, clipRectangle, gtk_plus_state);
					return true;
				#endregion
				#region CP_BORDER
				case COMBOBOXPARTS.CP_BORDER:
					switch ((BORDERSTATES)state) {
					case BORDERSTATES.CBB_NORMAL:
					case BORDERSTATES.CBB_HOT:
					case BORDERSTATES.CBB_FOCUSED:
					case BORDERSTATES.CBB_DISABLED:
						GtkPlus.ComboBoxPaintBorder (dc, bounds, clipRectangle);
						return true;
					default: return false;
					}
				#endregion
				default: return false;
				}
			#endregion
			#region EDIT
			case ThemeHandle.EDIT:
				switch ((EDITPARTS)part) {
				#region EP_EDITTEXT
				case EDITPARTS.EP_EDITTEXT:
					switch ((EDITTEXTSTATES)state) {
					case EDITTEXTSTATES.ETS_NORMAL:
					case EDITTEXTSTATES.ETS_ASSIST:
					case EDITTEXTSTATES.ETS_READONLY:
					case EDITTEXTSTATES.ETS_HOT:
					case EDITTEXTSTATES.ETS_SELECTED:
					case EDITTEXTSTATES.ETS_FOCUSED:
						gtk_plus_state = GtkPlusState.Normal;
						break;
					case EDITTEXTSTATES.ETS_DISABLED:
						gtk_plus_state = GtkPlusState.Disabled;
						break;
					default: return false;
					}
					GtkPlus.TextBoxPaint (dc, bounds, excludedArea, gtk_plus_state);
					return true;
				#endregion
				default: return false;
				}
			#endregion
			#region HEADER
			case ThemeHandle.HEADER:
				switch ((HEADERPARTS)part) {
				#region HP_HEADERITEM
				case HEADERPARTS.HP_HEADERITEM:
					switch ((HEADERITEMSTATES)state) {
					case HEADERITEMSTATES.HIS_NORMAL:
						gtk_plus_state = GtkPlusState.Normal;
						break;
					case HEADERITEMSTATES.HIS_HOT:
						gtk_plus_state = GtkPlusState.Hot;
						break;
					case HEADERITEMSTATES.HIS_PRESSED:
						gtk_plus_state = GtkPlusState.Pressed;
						break;
					default: return false;
					}
					GtkPlus.HeaderPaint (dc, bounds, clipRectangle, gtk_plus_state);
					return true;
				#endregion
				default: return false;
				}
			#endregion
			#region PROGRESS
			case ThemeHandle.PROGRESS:
				switch ((PROGRESSPARTS)part) {
				case PROGRESSPARTS.PP_BAR:
				case PROGRESSPARTS.PP_BARVERT:
					GtkPlus.ProgressBarPaintBar (dc, bounds, clipRectangle);
					return true;
				case PROGRESSPARTS.PP_CHUNK:
				case PROGRESSPARTS.PP_CHUNKVERT:
					GtkPlus.ProgressBarPaintChunk (dc, bounds, clipRectangle);
					return true;
				default: return false;
				}
			#endregion
			#region REBAR
			case ThemeHandle.REBAR:
				switch ((REBARPARTS)part) {
				case REBARPARTS.RP_BAND:
					GtkPlus.ToolBarPaint (dc, bounds, clipRectangle);
					return true;
				default: return false;
				}
			#endregion
			#region SCROLLBAR
			case ThemeHandle.SCROLLBAR:
				switch ((SCROLLBARPARTS)part) {
				#region SBP_ARROWBTN
				case SCROLLBARPARTS.SBP_ARROWBTN:
					bool horizontal;
					bool up_or_left;
					switch ((ARROWBTNSTATES)state) {
					case ARROWBTNSTATES.ABS_UPNORMAL:
						gtk_plus_state = GtkPlusState.Normal;
						horizontal = false;
						up_or_left = true;
						break;
					case ARROWBTNSTATES.ABS_UPHOT:
						gtk_plus_state = GtkPlusState.Hot;
						horizontal = false;
						up_or_left = true;
						break;
					case ARROWBTNSTATES.ABS_UPPRESSED:
						gtk_plus_state = GtkPlusState.Pressed;
						horizontal = false;
						up_or_left = true;
						break;
					case ARROWBTNSTATES.ABS_UPDISABLED:
						gtk_plus_state = GtkPlusState.Disabled;
						horizontal = false;
						up_or_left = true;
						break;
					case ARROWBTNSTATES.ABS_DOWNNORMAL:
						gtk_plus_state = GtkPlusState.Normal;
						horizontal = false;
						up_or_left = false;
						break;
					case ARROWBTNSTATES.ABS_DOWNHOT:
						gtk_plus_state = GtkPlusState.Hot;
						horizontal = false;
						up_or_left = false;
						break;
					case ARROWBTNSTATES.ABS_DOWNPRESSED:
						gtk_plus_state = GtkPlusState.Pressed;
						horizontal = false;
						up_or_left = false;
						break;
					case ARROWBTNSTATES.ABS_DOWNDISABLED:
						gtk_plus_state = GtkPlusState.Disabled;
						horizontal = false;
						up_or_left = false;
						break;
					case ARROWBTNSTATES.ABS_LEFTNORMAL:
						gtk_plus_state = GtkPlusState.Normal;
						horizontal = true;
						up_or_left = true;
						break;
					case ARROWBTNSTATES.ABS_LEFTHOT:
						gtk_plus_state = GtkPlusState.Hot;
						horizontal = true;
						up_or_left = true;
						break;
					case ARROWBTNSTATES.ABS_LEFTPRESSED:
						gtk_plus_state = GtkPlusState.Pressed;
						horizontal = true;
						up_or_left = true;
						break;
					case ARROWBTNSTATES.ABS_LEFTDISABLED:
						gtk_plus_state = GtkPlusState.Disabled;
						horizontal = true;
						up_or_left = true;
						break;
					case ARROWBTNSTATES.ABS_RIGHTNORMAL:
						gtk_plus_state = GtkPlusState.Normal;
						horizontal = true;
						up_or_left = false;
						break;
					case ARROWBTNSTATES.ABS_RIGHTHOT:
						gtk_plus_state = GtkPlusState.Hot;
						horizontal = true;
						up_or_left = false;
						break;
					case ARROWBTNSTATES.ABS_RIGHTPRESSED:
						gtk_plus_state = GtkPlusState.Pressed;
						horizontal = true;
						up_or_left = false;
						break;
					case ARROWBTNSTATES.ABS_RIGHTDISABLED:
						gtk_plus_state = GtkPlusState.Disabled;
						horizontal = true;
						up_or_left = false;
						break;
					default: return false;
					}
					GtkPlus.ScrollBarPaintArrowButton (dc, bounds, clipRectangle, gtk_plus_state, horizontal, up_or_left);
					return true;
				#endregion
				#region SBP_THUMBBTNHORZ, SBP_THUMBBTNVERT
				case SCROLLBARPARTS.SBP_THUMBBTNHORZ:
				case SCROLLBARPARTS.SBP_THUMBBTNVERT:
					if (!GetGtkPlusState ((SCROLLBARSTYLESTATES)state, out gtk_plus_state))
						return false;
					GtkPlus.ScrollBarPaintThumbButton (
						dc,
						bounds,
						clipRectangle,
						gtk_plus_state,
						(SCROLLBARPARTS)part == SCROLLBARPARTS.SBP_THUMBBTNHORZ);
					return true;
				#endregion
				#region SBP_LOWERTRACKHORZ, SBP_UPPERTRACKHORZ, SBP_LOWERTRACKVERT, SBP_UPPERTRACKVERT
				case SCROLLBARPARTS.SBP_LOWERTRACKHORZ:
				case SCROLLBARPARTS.SBP_UPPERTRACKHORZ:
				case SCROLLBARPARTS.SBP_LOWERTRACKVERT:
				case SCROLLBARPARTS.SBP_UPPERTRACKVERT:
					if (!GetGtkPlusState ((SCROLLBARSTYLESTATES)state, out gtk_plus_state))
						return false;
					GtkPlus.ScrollBarPaintTrack (
						dc,
						bounds,
						clipRectangle,
						gtk_plus_state,
						(SCROLLBARPARTS)part == SCROLLBARPARTS.SBP_LOWERTRACKHORZ ||
						(SCROLLBARPARTS)part == SCROLLBARPARTS.SBP_UPPERTRACKHORZ,
						(SCROLLBARPARTS)part == SCROLLBARPARTS.SBP_UPPERTRACKHORZ ||
						(SCROLLBARPARTS)part == SCROLLBARPARTS.SBP_UPPERTRACKVERT);
					return true;
				#endregion
				default: return false;
				}
			#endregion
			#region SPIN
			case ThemeHandle.SPIN:
				bool up;
				switch ((SPINPARTS)part) {
				#region SPNP_UP
				case SPINPARTS.SPNP_UP:
					up = true;
					switch ((UPSTATES)state) {
					case UPSTATES.UPS_NORMAL: gtk_plus_state = GtkPlusState.Normal; break;
					case UPSTATES.UPS_HOT: gtk_plus_state = GtkPlusState.Hot; break;
					case UPSTATES.UPS_PRESSED: gtk_plus_state = GtkPlusState.Pressed; break;
					case UPSTATES.UPS_DISABLED: gtk_plus_state = GtkPlusState.Disabled; break;
					default: return false;
					}
					break;
				#endregion
				#region SPNP_DOWN
				case SPINPARTS.SPNP_DOWN:
					up = false;
					switch ((DOWNSTATES)state) {
					case DOWNSTATES.DNS_NORMAL: gtk_plus_state = GtkPlusState.Normal; break;
					case DOWNSTATES.DNS_HOT: gtk_plus_state = GtkPlusState.Hot; break;
					case DOWNSTATES.DNS_PRESSED: gtk_plus_state = GtkPlusState.Pressed; break;
					case DOWNSTATES.DNS_DISABLED: gtk_plus_state = GtkPlusState.Disabled; break;
					default: return false;
					}
					break;
				#endregion
				default: return false;
				}
				GtkPlus.UpDownPaint (dc, bounds, clipRectangle, up, gtk_plus_state);
				return true;
			#endregion
			#region STATUS
			case ThemeHandle.STATUS:
				switch ((STATUSPARTS)part) {
				case STATUSPARTS.SP_GRIPPER:
					GtkPlus.StatusBarPaintGripper (dc, bounds, clipRectangle);
					return true;
				default: return false;
				}
			#endregion
			#region TABCONTROL
			case ThemeHandle.TAB:
				bool selected;
				switch ((TABPARTS)part) {
				#region TABP_TABITEM
				case TABPARTS.TABP_TABITEM:
					switch ((TABITEMSTATES)state) {
					case TABITEMSTATES.TIS_SELECTED:
						selected = true;
						break;
					case TABITEMSTATES.TIS_NORMAL:
					case TABITEMSTATES.TIS_HOT:
					case TABITEMSTATES.TIS_DISABLED:
						selected = false;
						break;
					default: return false;
					}
					break;
				#endregion
				#region TABP_TABITEMLEFTEDGE
				case TABPARTS.TABP_TABITEMLEFTEDGE:
					switch ((TABITEMLEFTEDGESTATES)state) {
					case TABITEMLEFTEDGESTATES.TILES_SELECTED:
						selected = true;
						break;
					case TABITEMLEFTEDGESTATES.TILES_NORMAL:
					case TABITEMLEFTEDGESTATES.TILES_HOT:
					case TABITEMLEFTEDGESTATES.TILES_DISABLED:
						selected = false;
						break;
					default: return false;
					}
					break;
				#endregion
				#region TABP_TABITEMRIGHTEDGE
				case TABPARTS.TABP_TABITEMRIGHTEDGE:
					switch ((TABITEMRIGHTEDGESTATES)state) {
					case TABITEMRIGHTEDGESTATES.TIRES_SELECTED:
						selected = true;
						break;
					case TABITEMRIGHTEDGESTATES.TIRES_NORMAL:
					case TABITEMRIGHTEDGESTATES.TIRES_HOT:
					case TABITEMRIGHTEDGESTATES.TIRES_DISABLED:
						selected = false;
						break;
					default: return false;
					}
					break;
				#endregion
				#region TABP_TABITEMBOTHEDGE
				case TABPARTS.TABP_TABITEMBOTHEDGE:
					selected = false;
					break;
				#endregion
				#region TABP_TOPTABITEM
				case TABPARTS.TABP_TOPTABITEM:
					switch ((TOPTABITEMSTATES)state) {
					case TOPTABITEMSTATES.TTIS_SELECTED:
						selected = true;
						break;
					case TOPTABITEMSTATES.TTIS_NORMAL:
					case TOPTABITEMSTATES.TTIS_HOT:
					case TOPTABITEMSTATES.TTIS_DISABLED:
						selected = false;
						break;
					default: return false;
					}
					break;
				#endregion
				#region TABP_TOPTABITEMLEFTEDGE
				case TABPARTS.TABP_TOPTABITEMLEFTEDGE:
					switch ((TOPTABITEMLEFTEDGESTATES)state) {
					case TOPTABITEMLEFTEDGESTATES.TTILES_SELECTED:
						selected = true;
						break;
					case TOPTABITEMLEFTEDGESTATES.TTILES_NORMAL:
					case TOPTABITEMLEFTEDGESTATES.TTILES_HOT:
					case TOPTABITEMLEFTEDGESTATES.TTILES_DISABLED:
						selected = false;
						break;
					default: return false;
					}
					break;
				#endregion
				#region TABP_TOPTABITEMRIGHTEDGE
				case TABPARTS.TABP_TOPTABITEMRIGHTEDGE:
					switch ((TOPTABITEMRIGHTEDGESTATES)state) {
					case TOPTABITEMRIGHTEDGESTATES.TTIRES_SELECTED:
						selected = true;
						break;
					case TOPTABITEMRIGHTEDGESTATES.TTIRES_NORMAL:
					case TOPTABITEMRIGHTEDGESTATES.TTIRES_HOT:
					case TOPTABITEMRIGHTEDGESTATES.TTIRES_DISABLED:
						selected = false;
						break;
					default: return false;
					}
					break;
				#endregion
				#region TABP_TOPTABITEMBOTHEDGE
				case TABPARTS.TABP_TOPTABITEMBOTHEDGE:
					selected = false;
					break;
				#endregion
				#region TABP_PANE
				case TABPARTS.TABP_PANE:
					GtkPlus.TabControlPaintPane (dc, bounds, clipRectangle);
					return true;
				#endregion
				default: return false;
				}
				GtkPlus.TabControlPaintTabItem (dc, bounds, clipRectangle, selected ? GtkPlusState.Pressed : GtkPlusState.Normal);
				return true;
			#endregion
			#region TOOLBAR
			case ThemeHandle.TOOLBAR:
				switch ((TOOLBARPARTS)part) {
				case TOOLBARPARTS.TP_BUTTON:
					switch ((TOOLBARSTYLESTATES)state) {
					case TOOLBARSTYLESTATES.TS_NORMAL:
						gtk_plus_state = GtkPlusState.Normal;
						break;
					case TOOLBARSTYLESTATES.TS_HOT:
						gtk_plus_state = GtkPlusState.Hot;
						break;
					case TOOLBARSTYLESTATES.TS_PRESSED:
						gtk_plus_state = GtkPlusState.Pressed;
						break;
					case TOOLBARSTYLESTATES.TS_DISABLED:
						gtk_plus_state = GtkPlusState.Disabled;
						break;
					case TOOLBARSTYLESTATES.TS_CHECKED:
					case TOOLBARSTYLESTATES.TS_HOTCHECKED:
						GtkPlus.ToolBarPaintCheckedButton (dc, bounds, clipRectangle);
						return true;
					default: return false;
					}
					GtkPlus.ToolBarPaintButton (dc, bounds, clipRectangle, gtk_plus_state);
					return true;
				default: return false;
				}
			#endregion
			#region TRACKBAR
			case ThemeHandle.TRACKBAR:
				switch ((TRACKBARPARTS)part) {
				#region TKP_TRACK
				case TRACKBARPARTS.TKP_TRACK:
					switch ((TRACKSTATES)state) {
					case TRACKSTATES.TRS_NORMAL:
						GtkPlus.TrackBarPaintTrack (dc, bounds, clipRectangle, true);
						return true;
					default: return false;
					}
				#endregion
				#region TKP_TRACKVERT
				case TRACKBARPARTS.TKP_TRACKVERT:
					switch ((TRACKVERTSTATES)state) {
					case TRACKVERTSTATES.TRVS_NORMAL:
						GtkPlus.TrackBarPaintTrack (dc, bounds, clipRectangle, false);
						return true;
					default: return false;
					}
				#endregion
				#region TKP_THUMB
				case TRACKBARPARTS.TKP_THUMB:
					switch ((THUMBSTATES)state) {
					case THUMBSTATES.TUS_NORMAL:
						gtk_plus_state = GtkPlusState.Normal;
						break;
					case THUMBSTATES.TUS_HOT:
						gtk_plus_state = GtkPlusState.Hot;
						break;
					case THUMBSTATES.TUS_PRESSED:
						gtk_plus_state = GtkPlusState.Pressed;
						break;
					case THUMBSTATES.TUS_FOCUSED:
						gtk_plus_state = GtkPlusState.Selected;
						break;
					case THUMBSTATES.TUS_DISABLED:
						gtk_plus_state = GtkPlusState.Disabled;
						break;
					default: return false;
					}
					GtkPlus.TrackBarPaintThumb (dc, bounds, clipRectangle, gtk_plus_state, true);
					return true;
				#endregion
				#region TKP_THUMBVERT
				case TRACKBARPARTS.TKP_THUMBVERT:
					switch ((THUMBVERTSTATES)state) {
					case THUMBVERTSTATES.TUVS_NORMAL:
						gtk_plus_state = GtkPlusState.Normal;
						break;
					case THUMBVERTSTATES.TUVS_HOT:
						gtk_plus_state = GtkPlusState.Hot;
						break;
					case THUMBVERTSTATES.TUVS_PRESSED:
						gtk_plus_state = GtkPlusState.Pressed;
						break;
					case THUMBVERTSTATES.TUVS_FOCUSED:
						gtk_plus_state = GtkPlusState.Selected;
						break;
					case THUMBVERTSTATES.TUVS_DISABLED:
						gtk_plus_state = GtkPlusState.Disabled;
						break;
					default: return false;
					}
					GtkPlus.TrackBarPaintThumb (dc, bounds, clipRectangle, gtk_plus_state, false);
					return true;
				#endregion
				default: return false;
				}
			#endregion
			#region TREEVIEW
			case ThemeHandle.TREEVIEW:
				switch ((TREEVIEWPARTS)part) {
				case TREEVIEWPARTS.TVP_GLYPH:
					bool closed;
					switch ((GLYPHSTATES)state) {
					case GLYPHSTATES.GLPS_CLOSED : closed = true; break;
					case GLYPHSTATES.GLPS_OPENED: closed = false; break;
					default: return false;
					}
					GtkPlus.TreeViewPaintGlyph (dc, bounds, clipRectangle, closed);
					return true;
				default: return false;
				}
			#endregion
			default: return false;
			}
		}
		static bool GetGtkPlusState (SCROLLBARSTYLESTATES state, out GtkPlusState result)
		{
			switch (state) {
			case SCROLLBARSTYLESTATES.SCRBS_NORMAL:
				result = GtkPlusState.Normal;
				break;
			case SCROLLBARSTYLESTATES.SCRBS_HOT:
				result = GtkPlusState.Hot;
				break;
			case SCROLLBARSTYLESTATES.SCRBS_PRESSED:
				result = GtkPlusState.Pressed;
				break;
			case SCROLLBARSTYLESTATES.SCRBS_DISABLED:
				result = GtkPlusState.Disabled;
				break;
			default:
				result = (GtkPlusState)0;
				return false;
			}
			return true;
		}
		public int UxThemeDrawThemeEdge (IntPtr hTheme, IDeviceContext dc, int iPartId, int iStateId, Rectangle bounds, Edges edges, EdgeStyle style, EdgeEffects effects, out Rectangle result)
		{
			result = Rectangle.Empty;
			return (int)S.S_FALSE;
		}
		public int UxThemeDrawThemeText (IntPtr hTheme, IDeviceContext dc, int iPartId, int iStateId, string text, TextFormatFlags textFlags, Rectangle bounds)
		{
			return (int)S.S_FALSE;
		}
		public int UxThemeGetThemeBackgroundContentRect (IntPtr hTheme, IDeviceContext dc, int iPartId, int iStateId, Rectangle bounds, out Rectangle result)
		{
			return (int)(GetBackgroundContentRectangle ((ThemeHandle)(int)hTheme, iPartId, iStateId, bounds, out result) ? S.S_OK : S.S_FALSE);
		}
		bool GetBackgroundContentRectangle (ThemeHandle handle, int part, int state, Rectangle bounds, out Rectangle result)
		{
			switch (handle) {
			case ThemeHandle.PROGRESS:
				switch ((PROGRESSPARTS)part) {
				case PROGRESSPARTS.PP_BAR:
				case PROGRESSPARTS.PP_BARVERT:
					result = GtkPlus.ProgressBarGetBackgroundContentRectagle (bounds);
					return true;
				}
				break;
			}
			result = Rectangle.Empty;
			return false;
		}
		public int UxThemeGetThemeBackgroundExtent (IntPtr hTheme, IDeviceContext dc, int iPartId, int iStateId, Rectangle contentBounds, out Rectangle result)
		{
			result = Rectangle.Empty;
			return (int)S.S_FALSE;
		}
		public int UxThemeGetThemeBackgroundRegion (IntPtr hTheme, IDeviceContext dc, int iPartId, int iStateId, Rectangle bounds, out Region result)
		{
			result = null;
			return (int)S.S_FALSE;
		}
		public int UxThemeGetThemeBool (IntPtr hTheme, int iPartId, int iStateId, BooleanProperty prop, out bool result)
		{
			result = false;
			return (int)S.S_FALSE;
		}
		public int UxThemeGetThemeColor (IntPtr hTheme, int iPartId, int iStateId, ColorProperty prop, out Color result)
		{
			result = Color.Black;
			return (int)S.S_FALSE;
		}
		public int UxThemeGetThemeEnumValue (IntPtr hTheme, int iPartId, int iStateId, EnumProperty prop, out int result)
		{
			result = 0;
			return (int)S.S_FALSE;
		}
		public int UxThemeGetThemeFilename (IntPtr hTheme, int iPartId, int iStateId, FilenameProperty prop, out string result)
		{
			result = null;
			return (int)S.S_FALSE;
		}
		public int UxThemeGetThemeInt (IntPtr hTheme, int iPartId, int iStateId, IntegerProperty prop, out int result)
		{
			return (int)(GetInteger ((ThemeHandle)(int)hTheme, iPartId, iStateId, prop, out result) ? S.S_OK : S.S_FALSE);
		}
		bool GetInteger (ThemeHandle handle, int part, int state, IntegerProperty property, out int result)
		{
			switch (handle) {
			case ThemeHandle.PROGRESS:
				switch ((PROGRESSPARTS)part) {
				case PROGRESSPARTS.PP_CHUNK:
				case PROGRESSPARTS.PP_CHUNKVERT:
					switch (property) {
					case IntegerProperty.ProgressChunkSize:
						result = ThemeWin32Classic.ProgressBarGetChunkSize ();
						return true;
					case IntegerProperty.ProgressSpaceSize:
						result = ThemeWin32Classic.ProgressBarChunkSpacing;
						return true;
					}
					break;
				}
				break;
			}
			result = 0;
			return false;
		}
		public int UxThemeGetThemeMargins (IntPtr hTheme, IDeviceContext dc, int iPartId, int iStateId, MarginProperty prop, out Padding result)
		{
			result = Padding.Empty;
			return (int)S.S_FALSE;
		}
		public int UxThemeGetThemePartSize (IntPtr hTheme, IDeviceContext dc, int iPartId, int iStateId, Rectangle bounds, ThemeSizeType type, out Size result)
		{
			return (int)(GetPartSize ((ThemeHandle)(int)hTheme, dc, iPartId, iStateId, bounds, true, type, out result) ? S.S_OK : S.S_FALSE);
		}
		public int UxThemeGetThemePartSize (IntPtr hTheme, IDeviceContext dc, int iPartId, int iStateId, ThemeSizeType type, out Size result)
		{
			return (int)(GetPartSize ((ThemeHandle)(int)hTheme, dc, iPartId, iStateId, Rectangle.Empty, false, type, out result) ? S.S_OK : S.S_FALSE);
		}
		bool GetPartSize (ThemeHandle themeHandle, IDeviceContext dc, int part, int state, Rectangle bounds, bool rectangleSpecified, ThemeSizeType type, out Size result)
		{
			switch (themeHandle) {
			#region BUTTON
			case ThemeHandle.BUTTON:
				switch ((BUTTONPARTS)part) {
				case BUTTONPARTS.BP_RADIOBUTTON:
					result = GtkPlus.RadioButtonGetSize ();
					return true;
				case BUTTONPARTS.BP_CHECKBOX:
					result = GtkPlus.CheckBoxGetSize ();
					return true;
				}
				break;
			#endregion
			#region HEADER
			case ThemeHandle.HEADER:
				switch ((HEADERPARTS)part) {
				case HEADERPARTS.HP_HEADERITEM:
					result = new Size (0, ThemeWin32Classic.ListViewGetHeaderHeight ());
					return true;
				}
				break;
			#endregion
			#region TRACKBAR
			case ThemeHandle.TRACKBAR:
				switch ((TRACKBARPARTS)part) {
				case TRACKBARPARTS.TKP_TRACK:
					result = new Size (0, ThemeWin32Classic.TrackBarHorizontalTrackHeight);
					return true;
				case TRACKBARPARTS.TKP_TRACKVERT:
					result = new Size (ThemeWin32Classic.TrackBarVerticalTrackWidth, 0);
					return true;
				case TRACKBARPARTS.TKP_THUMB:
				case TRACKBARPARTS.TKP_THUMBVERT:
					result = ThemeWin32Classic.TrackBarGetThumbSize ();
					if ((TRACKBARPARTS)part == TRACKBARPARTS.TKP_THUMBVERT) {
						int temporary = result.Width;
						result.Width = result.Height;
						result.Height = temporary;
					}
					return true;
				}
				break;
			#endregion
			}
			result = Size.Empty;
			return false;
		}
		public int UxThemeGetThemePosition (IntPtr hTheme, int iPartId, int iStateId, PointProperty prop, out Point result)
		{
			result = Point.Empty;
			return (int)S.S_FALSE;
		}
		public int UxThemeGetThemeString (IntPtr hTheme, int iPartId, int iStateId, StringProperty prop, out string result)
		{
			result = null;
			return (int)S.S_FALSE;
		}
		public int UxThemeGetThemeTextExtent (IntPtr hTheme, IDeviceContext dc, int iPartId, int iStateId, string textToDraw, TextFormatFlags flags, Rectangle bounds, out Rectangle result)
		{
			result = Rectangle.Empty;
			return (int)S.S_FALSE;
		}
		public int UxThemeGetThemeTextExtent (IntPtr hTheme, IDeviceContext dc, int iPartId, int iStateId, string textToDraw, TextFormatFlags flags, out Rectangle result)
		{
			result = Rectangle.Empty;
			return (int)S.S_FALSE;
		}
		public int UxThemeGetThemeTextMetrics (IntPtr hTheme, IDeviceContext dc, int iPartId, int iStateId, out TextMetrics result)
		{
			result = new TextMetrics ();
			return (int)S.S_FALSE;
		}
		public int UxThemeHitTestThemeBackground (IntPtr hTheme, IDeviceContext dc, int iPartId, int iStateId, HitTestOptions options, Rectangle backgroundRectangle, IntPtr hrgn, Point pt, out HitTestCode result)
		{
			result = HitTestCode.Bottom;
			return (int)S.S_FALSE;
		}
		public bool UxThemeIsAppThemed ()
		{
			return true;
		}
		public bool UxThemeIsThemeActive ()
		{
			return true;
		}
		public bool UxThemeIsThemeBackgroundPartiallyTransparent (IntPtr hTheme, int iPartId, int iStateId)
		{
			return true;
		}
		public bool UxThemeIsThemePartDefined (IntPtr hTheme, int iPartId)
		{
			switch ((ThemeHandle)(int)hTheme) {
			#region BUTTON
			case ThemeHandle.BUTTON:
				switch ((BUTTONPARTS)iPartId) {
				case BUTTONPARTS.BP_PUSHBUTTON:
				case BUTTONPARTS.BP_CHECKBOX:
				case BUTTONPARTS.BP_RADIOBUTTON:
				case BUTTONPARTS.BP_GROUPBOX:
					return true;
				default: return false;
				}
			#endregion
			#region COMBOBOX
			case ThemeHandle.COMBOBOX:
				switch ((COMBOBOXPARTS)iPartId) {
				case COMBOBOXPARTS.CP_DROPDOWNBUTTON:
				case COMBOBOXPARTS.CP_BORDER:
					return true;
				default: return false;
				}
			#endregion
			#region EDIT
			case ThemeHandle.EDIT:
				switch ((EDITPARTS)iPartId) {
				case EDITPARTS.EP_EDITTEXT:
					return true;
				default: return false;
				}
			#endregion
			#region HEADER
			case ThemeHandle.HEADER:
				switch ((HEADERPARTS)iPartId) {
				case HEADERPARTS.HP_HEADERITEM:
					return true;
				default: return false;
				}
			#endregion
			#region PROGRESS
			case ThemeHandle.PROGRESS:
				switch ((PROGRESSPARTS)iPartId) {
				case PROGRESSPARTS.PP_BAR:
				case PROGRESSPARTS.PP_BARVERT:
				case PROGRESSPARTS.PP_CHUNK:
				case PROGRESSPARTS.PP_CHUNKVERT:
					return true;
				default: return false;
				}
			#endregion
			#region REBAR
			case ThemeHandle.REBAR:
				switch ((REBARPARTS)iPartId) {
				case REBARPARTS.RP_BAND:
					return true;
				default: return false;
				}
			#endregion
			#region SCROLLBAR
			case ThemeHandle.SCROLLBAR:
				switch ((SCROLLBARPARTS)iPartId) {
				case SCROLLBARPARTS.SBP_ARROWBTN:
				case SCROLLBARPARTS.SBP_THUMBBTNHORZ:
				case SCROLLBARPARTS.SBP_THUMBBTNVERT:
				case SCROLLBARPARTS.SBP_LOWERTRACKHORZ:
				case SCROLLBARPARTS.SBP_UPPERTRACKHORZ:
				case SCROLLBARPARTS.SBP_LOWERTRACKVERT:
				case SCROLLBARPARTS.SBP_UPPERTRACKVERT:
					return true;
				default: return false;
				}
			#endregion
			#region SPIN
			case ThemeHandle.SPIN:
				switch ((SPINPARTS)iPartId) {
				case SPINPARTS.SPNP_UP:
				case SPINPARTS.SPNP_DOWN:
					return true;
				default: return false;
				}

			#endregion
			#region STATUS
			case ThemeHandle.STATUS:
				switch ((STATUSPARTS)iPartId) {
				case STATUSPARTS.SP_GRIPPER:
					return true;
				default: return false;
				}
			#endregion
			#region TABCONTROL
			case ThemeHandle.TAB:
				switch ((TABPARTS)iPartId) {
				case TABPARTS.TABP_TABITEM:
				case TABPARTS.TABP_TABITEMLEFTEDGE:
				case TABPARTS.TABP_TABITEMRIGHTEDGE:
				case TABPARTS.TABP_TABITEMBOTHEDGE:
				case TABPARTS.TABP_TOPTABITEM:
				case TABPARTS.TABP_TOPTABITEMLEFTEDGE:
				case TABPARTS.TABP_TOPTABITEMRIGHTEDGE:
				case TABPARTS.TABP_TOPTABITEMBOTHEDGE:
				case TABPARTS.TABP_PANE:
					return true;
				default: return false;
				}
			#endregion
			#region TOOLBAR
			case ThemeHandle.TOOLBAR:
				switch ((TOOLBARPARTS)iPartId) {
				case TOOLBARPARTS.TP_BUTTON:
					return true;
				default: return false;
				}
			#endregion
			#region TRACKBAR
			case ThemeHandle.TRACKBAR:
				switch ((TRACKBARPARTS)iPartId) {
				case TRACKBARPARTS.TKP_TRACK:
				case TRACKBARPARTS.TKP_TRACKVERT:
				case TRACKBARPARTS.TKP_THUMB:
				case TRACKBARPARTS.TKP_THUMBVERT:
					return true;
				default: return false;
				}
			#endregion
			#region TREEVIEW
			case ThemeHandle.TREEVIEW:
				switch ((TREEVIEWPARTS)iPartId) {
				case TREEVIEWPARTS.TVP_GLYPH:
					return true;
				default: return false;
				}
			#endregion
			default: return false;
			}
		}
		public IntPtr UxThemeOpenThemeData (IntPtr hWnd, string classList)
		{
			ThemeHandle theme_handle;
			try {
				theme_handle = (ThemeHandle)Enum.Parse (typeof (ThemeHandle), classList);
			} catch (ArgumentException) {
				return IntPtr.Zero;
			}
			return (IntPtr)(int)theme_handle;
		}
		#endregion
		#region VisualStyleInformation
		public string VisualStyleInformationAuthor {
			get {
				return null;
			}
		}
		public string VisualStyleInformationColorScheme {
			get {
				return null;
			}
		}
		public string VisualStyleInformationCompany {
			get {
				return null;
			}
		}
		public Color VisualStyleInformationControlHighlightHot {
			get {
				return Color.Black;
			}
		}
		public string VisualStyleInformationCopyright {
			get {
				return null;
			}
		}
		public string VisualStyleInformationDescription {
			get {
				return null;
			}
		}
		public string VisualStyleInformationDisplayName {
			get {
				return null;
			}
		}
		public string VisualStyleInformationFileName {
			get {
				return null;
			}
		}
		public bool VisualStyleInformationIsSupportedByOS
		{
			get {
				return true;
			}
		}
		public int VisualStyleInformationMinimumColorDepth {
			get {
				return 0;
			}
		}
		public string VisualStyleInformationSize {
			get {
				return null;
			}
		}
		public bool VisualStyleInformationSupportsFlatMenus {
			get {
				return false;
			}
		}
		public Color VisualStyleInformationTextControlBorder {
			get {
				return Color.Black;
			}
		}
		public string VisualStyleInformationUrl {
			get {
				return null;	
			}
		}
		public string VisualStyleInformationVersion {
			get {
				return null;	
			}
		}
		#endregion
		#region VisualStyleRenderer
		public void VisualStyleRendererDrawBackgroundExcludingArea (IntPtr theme, IDeviceContext dc, int part, int state, Rectangle bounds, Rectangle excludedArea)
		{
			DrawBackground ((ThemeHandle)(int)theme, dc, part, state, bounds, bounds, excludedArea);
		}
		#endregion
	}
}
