//		
//			System.Windows.Forms.ComboBox
//
//			Author: 
//						Joel Basson		(jstrike@mweb.co.za)
//
//

using System;
using Gtk;
using GtkSharp;
using Gdk;
using GLib;
using Gnome;

namespace System.Windows.Forms
{
	[MonoTODO]
	internal class ShortcutHelper {
		public static void AddShortcutToWidget (Gtk.Widget widget, Gtk.AccelGroup group, Shortcut shortcut, string signal) {
			switch (shortcut) {
			case Shortcut.CtrlA : widget.AddAccelerator(signal, group, (uint)Gdk.Key.N, Gdk.ModifierType.ControlMask, Gtk.AccelFlags.Visible); break;
			case Shortcut.CtrlB : widget.AddAccelerator(signal, group, (uint)Gdk.Key.B, Gdk.ModifierType.ControlMask, Gtk.AccelFlags.Visible); break;
			case Shortcut.CtrlC : widget.AddAccelerator(signal, group, (uint)Gdk.Key.C, Gdk.ModifierType.ControlMask, Gtk.AccelFlags.Visible); break;
			case Shortcut.CtrlD : widget.AddAccelerator(signal, group, (uint)Gdk.Key.D, Gdk.ModifierType.ControlMask, Gtk.AccelFlags.Visible); break;
			case Shortcut.CtrlE : widget.AddAccelerator(signal, group, (uint)Gdk.Key.E, Gdk.ModifierType.ControlMask, Gtk.AccelFlags.Visible); break;
			case Shortcut.CtrlF : widget.AddAccelerator(signal, group, (uint)Gdk.Key.F, Gdk.ModifierType.ControlMask, Gtk.AccelFlags.Visible); break;
			case Shortcut.CtrlG : widget.AddAccelerator(signal, group, (uint)Gdk.Key.G, Gdk.ModifierType.ControlMask, Gtk.AccelFlags.Visible); break;
			case Shortcut.CtrlH : widget.AddAccelerator(signal, group, (uint)Gdk.Key.H, Gdk.ModifierType.ControlMask, Gtk.AccelFlags.Visible); break;
			case Shortcut.CtrlI : widget.AddAccelerator(signal, group, (uint)Gdk.Key.I, Gdk.ModifierType.ControlMask, Gtk.AccelFlags.Visible); break;
			case Shortcut.CtrlJ : widget.AddAccelerator(signal, group, (uint)Gdk.Key.J, Gdk.ModifierType.ControlMask, Gtk.AccelFlags.Visible); break;
			case Shortcut.CtrlK : widget.AddAccelerator(signal, group, (uint)Gdk.Key.K, Gdk.ModifierType.ControlMask, Gtk.AccelFlags.Visible); break;
			case Shortcut.CtrlL : widget.AddAccelerator(signal, group, (uint)Gdk.Key.L, Gdk.ModifierType.ControlMask, Gtk.AccelFlags.Visible); break;
			case Shortcut.CtrlM : widget.AddAccelerator(signal, group, (uint)Gdk.Key.M, Gdk.ModifierType.ControlMask, Gtk.AccelFlags.Visible); break;
			case Shortcut.CtrlN : widget.AddAccelerator(signal, group, (uint)Gdk.Key.N, Gdk.ModifierType.ControlMask, Gtk.AccelFlags.Visible); break;
			case Shortcut.CtrlO : widget.AddAccelerator(signal, group, (uint)Gdk.Key.O, Gdk.ModifierType.ControlMask, Gtk.AccelFlags.Visible); break;
			case Shortcut.CtrlP : widget.AddAccelerator(signal, group, (uint)Gdk.Key.P, Gdk.ModifierType.ControlMask, Gtk.AccelFlags.Visible); break;
			case Shortcut.CtrlQ : widget.AddAccelerator(signal, group, (uint)Gdk.Key.Q, Gdk.ModifierType.ControlMask, Gtk.AccelFlags.Visible); break;
			case Shortcut.CtrlR : widget.AddAccelerator(signal, group, (uint)Gdk.Key.R, Gdk.ModifierType.ControlMask, Gtk.AccelFlags.Visible); break;
			case Shortcut.CtrlS : widget.AddAccelerator(signal, group, (uint)Gdk.Key.S, Gdk.ModifierType.ControlMask, Gtk.AccelFlags.Visible); break;
			case Shortcut.CtrlT : widget.AddAccelerator(signal, group, (uint)Gdk.Key.T, Gdk.ModifierType.ControlMask, Gtk.AccelFlags.Visible); break;
			case Shortcut.CtrlU : widget.AddAccelerator(signal, group, (uint)Gdk.Key.U, Gdk.ModifierType.ControlMask, Gtk.AccelFlags.Visible); break;
			case Shortcut.CtrlV : widget.AddAccelerator(signal, group, (uint)Gdk.Key.V, Gdk.ModifierType.ControlMask, Gtk.AccelFlags.Visible); break;
			case Shortcut.CtrlW : widget.AddAccelerator(signal, group, (uint)Gdk.Key.W, Gdk.ModifierType.ControlMask, Gtk.AccelFlags.Visible); break;
			case Shortcut.CtrlX : widget.AddAccelerator(signal, group, (uint)Gdk.Key.X, Gdk.ModifierType.ControlMask, Gtk.AccelFlags.Visible); break;
			case Shortcut.CtrlY : widget.AddAccelerator(signal, group, (uint)Gdk.Key.Y, Gdk.ModifierType.ControlMask, Gtk.AccelFlags.Visible); break;
			case Shortcut.CtrlZ : widget.AddAccelerator(signal, group, (uint)Gdk.Key.Z, Gdk.ModifierType.ControlMask, Gtk.AccelFlags.Visible); break;

			case Shortcut.Ctrl0 : widget.AddAccelerator(signal, group, (uint)Gdk.Key.KP_0, Gdk.ModifierType.ControlMask, Gtk.AccelFlags.Visible); break;
			case Shortcut.Ctrl1 : widget.AddAccelerator(signal, group, (uint)Gdk.Key.KP_1, Gdk.ModifierType.ControlMask, Gtk.AccelFlags.Visible); break;
			case Shortcut.Ctrl2 : widget.AddAccelerator(signal, group, (uint)Gdk.Key.KP_2, Gdk.ModifierType.ControlMask, Gtk.AccelFlags.Visible); break;
			case Shortcut.Ctrl3 : widget.AddAccelerator(signal, group, (uint)Gdk.Key.KP_3, Gdk.ModifierType.ControlMask, Gtk.AccelFlags.Visible); break;
			case Shortcut.Ctrl4 : widget.AddAccelerator(signal, group, (uint)Gdk.Key.KP_4, Gdk.ModifierType.ControlMask, Gtk.AccelFlags.Visible); break;
			case Shortcut.Ctrl5 : widget.AddAccelerator(signal, group, (uint)Gdk.Key.KP_5, Gdk.ModifierType.ControlMask, Gtk.AccelFlags.Visible); break;
			case Shortcut.Ctrl6 : widget.AddAccelerator(signal, group, (uint)Gdk.Key.KP_6, Gdk.ModifierType.ControlMask, Gtk.AccelFlags.Visible); break;
			case Shortcut.Ctrl7 : widget.AddAccelerator(signal, group, (uint)Gdk.Key.KP_7, Gdk.ModifierType.ControlMask, Gtk.AccelFlags.Visible); break;
			case Shortcut.Ctrl8 : widget.AddAccelerator(signal, group, (uint)Gdk.Key.KP_8, Gdk.ModifierType.ControlMask, Gtk.AccelFlags.Visible); break;
			case Shortcut.Ctrl9 : widget.AddAccelerator(signal, group, (uint)Gdk.Key.KP_9, Gdk.ModifierType.ControlMask, Gtk.AccelFlags.Visible); break;

			case Shortcut.CtrlDel : widget.AddAccelerator(signal, group, (uint)Gdk.Key.Delete, Gdk.ModifierType.ControlMask, Gtk.AccelFlags.Visible); break;
			case Shortcut.CtrlIns : widget.AddAccelerator(signal, group, (uint)Gdk.Key.Insert, Gdk.ModifierType.ControlMask, Gtk.AccelFlags.Visible); break;

			case Shortcut.CtrlF1 : widget.AddAccelerator(signal, group, (uint)Gdk.Key.F1, Gdk.ModifierType.ControlMask, Gtk.AccelFlags.Visible); break; 
			case Shortcut.CtrlF2 : widget.AddAccelerator(signal, group, (uint)Gdk.Key.F2, Gdk.ModifierType.ControlMask, Gtk.AccelFlags.Visible); break;
			case Shortcut.CtrlF3 : widget.AddAccelerator(signal, group, (uint)Gdk.Key.F3, Gdk.ModifierType.ControlMask, Gtk.AccelFlags.Visible); break;
			case Shortcut.CtrlF4 : widget.AddAccelerator(signal, group, (uint)Gdk.Key.F4, Gdk.ModifierType.ControlMask, Gtk.AccelFlags.Visible); break;
			case Shortcut.CtrlF5 : widget.AddAccelerator(signal, group, (uint)Gdk.Key.F5, Gdk.ModifierType.ControlMask, Gtk.AccelFlags.Visible); break;
			case Shortcut.CtrlF6 : widget.AddAccelerator(signal, group, (uint)Gdk.Key.F6, Gdk.ModifierType.ControlMask, Gtk.AccelFlags.Visible); break;
			case Shortcut.CtrlF7 : widget.AddAccelerator(signal, group, (uint)Gdk.Key.F7, Gdk.ModifierType.ControlMask, Gtk.AccelFlags.Visible); break;
			case Shortcut.CtrlF8 : widget.AddAccelerator(signal, group, (uint)Gdk.Key.F8, Gdk.ModifierType.ControlMask, Gtk.AccelFlags.Visible); break;
			case Shortcut.CtrlF9 : widget.AddAccelerator(signal, group, (uint)Gdk.Key.F9, Gdk.ModifierType.ControlMask, Gtk.AccelFlags.Visible); break;
			case Shortcut.CtrlF10 : widget.AddAccelerator(signal, group, (uint)Gdk.Key.F10, Gdk.ModifierType.ControlMask, Gtk.AccelFlags.Visible); break; 
			case Shortcut.CtrlF11 : widget.AddAccelerator(signal, group, (uint)Gdk.Key.F11, Gdk.ModifierType.ControlMask, Gtk.AccelFlags.Visible); break;
			case Shortcut.CtrlF12 : widget.AddAccelerator(signal, group, (uint)Gdk.Key.F12, Gdk.ModifierType.ControlMask, Gtk.AccelFlags.Visible); break;

			case Shortcut.ShiftDel : widget.AddAccelerator(signal, group, (uint)Gdk.Key.Delete, Gdk.ModifierType.ShiftMask, Gtk.AccelFlags.Visible); break;
			case Shortcut.ShiftIns : widget.AddAccelerator(signal, group, (uint)Gdk.Key.Insert, Gdk.ModifierType.ShiftMask, Gtk.AccelFlags.Visible); break;

			case Shortcut.ShiftF1 : widget.AddAccelerator(signal, group, (uint)Gdk.Key.F1, Gdk.ModifierType.ShiftMask, Gtk.AccelFlags.Visible); break;
			case Shortcut.ShiftF2 : widget.AddAccelerator(signal, group, (uint)Gdk.Key.F2, Gdk.ModifierType.ShiftMask, Gtk.AccelFlags.Visible); break;
			case Shortcut.ShiftF3 : widget.AddAccelerator(signal, group, (uint)Gdk.Key.F4, Gdk.ModifierType.ShiftMask, Gtk.AccelFlags.Visible); break;
			case Shortcut.ShiftF4 : widget.AddAccelerator(signal, group, (uint)Gdk.Key.F4, Gdk.ModifierType.ShiftMask, Gtk.AccelFlags.Visible); break;
			case Shortcut.ShiftF5 : widget.AddAccelerator(signal, group, (uint)Gdk.Key.F5, Gdk.ModifierType.ShiftMask, Gtk.AccelFlags.Visible); break;
			case Shortcut.ShiftF6 : widget.AddAccelerator(signal, group, (uint)Gdk.Key.F6, Gdk.ModifierType.ShiftMask, Gtk.AccelFlags.Visible); break;
			case Shortcut.ShiftF7 : widget.AddAccelerator(signal, group, (uint)Gdk.Key.F7, Gdk.ModifierType.ShiftMask, Gtk.AccelFlags.Visible); break;
			case Shortcut.ShiftF8 : widget.AddAccelerator(signal, group, (uint)Gdk.Key.F8, Gdk.ModifierType.ShiftMask, Gtk.AccelFlags.Visible); break;
			case Shortcut.ShiftF9 : widget.AddAccelerator(signal, group, (uint)Gdk.Key.F9, Gdk.ModifierType.ShiftMask, Gtk.AccelFlags.Visible); break;
			case Shortcut.ShiftF10 : widget.AddAccelerator(signal, group, (uint)Gdk.Key.F10, Gdk.ModifierType.ShiftMask, Gtk.AccelFlags.Visible); break;
			case Shortcut.ShiftF11 : widget.AddAccelerator(signal, group, (uint)Gdk.Key.F11, Gdk.ModifierType.ShiftMask, Gtk.AccelFlags.Visible); break;
			case Shortcut.ShiftF12 : widget.AddAccelerator(signal, group, (uint)Gdk.Key.F12, Gdk.ModifierType.ShiftMask, Gtk.AccelFlags.Visible); break;

			case Shortcut.CtrlShift0 : widget.AddAccelerator(signal, group, (uint)Gdk.Key.KP_0, Gdk.ModifierType.ShiftMask|Gdk.ModifierType.ControlMask, Gtk.AccelFlags.Visible); break;
			case Shortcut.CtrlShift1 : widget.AddAccelerator(signal, group, (uint)Gdk.Key.KP_1, Gdk.ModifierType.ShiftMask|Gdk.ModifierType.ControlMask, Gtk.AccelFlags.Visible); break;
			case Shortcut.CtrlShift2 : widget.AddAccelerator(signal, group, (uint)Gdk.Key.KP_2, Gdk.ModifierType.ShiftMask|Gdk.ModifierType.ControlMask, Gtk.AccelFlags.Visible); break;
			case Shortcut.CtrlShift3 : widget.AddAccelerator(signal, group, (uint)Gdk.Key.KP_3, Gdk.ModifierType.ShiftMask|Gdk.ModifierType.ControlMask, Gtk.AccelFlags.Visible); break;
			case Shortcut.CtrlShift4 : widget.AddAccelerator(signal, group, (uint)Gdk.Key.KP_4, Gdk.ModifierType.ShiftMask|Gdk.ModifierType.ControlMask, Gtk.AccelFlags.Visible); break;
			case Shortcut.CtrlShift5 : widget.AddAccelerator(signal, group, (uint)Gdk.Key.KP_5, Gdk.ModifierType.ShiftMask|Gdk.ModifierType.ControlMask, Gtk.AccelFlags.Visible); break;
			case Shortcut.CtrlShift6 : widget.AddAccelerator(signal, group, (uint)Gdk.Key.KP_6, Gdk.ModifierType.ShiftMask|Gdk.ModifierType.ControlMask, Gtk.AccelFlags.Visible); break;
			case Shortcut.CtrlShift7 : widget.AddAccelerator(signal, group, (uint)Gdk.Key.KP_7, Gdk.ModifierType.ShiftMask|Gdk.ModifierType.ControlMask, Gtk.AccelFlags.Visible); break;
			case Shortcut.CtrlShift8 : widget.AddAccelerator(signal, group, (uint)Gdk.Key.KP_8, Gdk.ModifierType.ShiftMask|Gdk.ModifierType.ControlMask, Gtk.AccelFlags.Visible); break;
			case Shortcut.CtrlShift9 : widget.AddAccelerator(signal, group, (uint)Gdk.Key.KP_9, Gdk.ModifierType.ShiftMask|Gdk.ModifierType.ControlMask, Gtk.AccelFlags.Visible); break;
			
			case Shortcut.CtrlShiftA : widget.AddAccelerator(signal, group, (uint)Gdk.Key.N, Gdk.ModifierType.ShiftMask|Gdk.ModifierType.ControlMask, Gtk.AccelFlags.Visible); break;
			case Shortcut.CtrlShiftB : widget.AddAccelerator(signal, group, (uint)Gdk.Key.B, Gdk.ModifierType.ShiftMask|Gdk.ModifierType.ControlMask, Gtk.AccelFlags.Visible); break;
			case Shortcut.CtrlShiftC : widget.AddAccelerator(signal, group, (uint)Gdk.Key.C, Gdk.ModifierType.ShiftMask|Gdk.ModifierType.ControlMask, Gtk.AccelFlags.Visible); break;
			case Shortcut.CtrlShiftD : widget.AddAccelerator(signal, group, (uint)Gdk.Key.D, Gdk.ModifierType.ShiftMask|Gdk.ModifierType.ControlMask, Gtk.AccelFlags.Visible); break;
			case Shortcut.CtrlShiftE : widget.AddAccelerator(signal, group, (uint)Gdk.Key.E, Gdk.ModifierType.ShiftMask|Gdk.ModifierType.ControlMask, Gtk.AccelFlags.Visible); break;
			case Shortcut.CtrlShiftF : widget.AddAccelerator(signal, group, (uint)Gdk.Key.F, Gdk.ModifierType.ShiftMask|Gdk.ModifierType.ControlMask, Gtk.AccelFlags.Visible); break;
			case Shortcut.CtrlShiftG : widget.AddAccelerator(signal, group, (uint)Gdk.Key.G, Gdk.ModifierType.ShiftMask|Gdk.ModifierType.ControlMask, Gtk.AccelFlags.Visible); break;
			case Shortcut.CtrlShiftH : widget.AddAccelerator(signal, group, (uint)Gdk.Key.H, Gdk.ModifierType.ShiftMask|Gdk.ModifierType.ControlMask, Gtk.AccelFlags.Visible); break;
			case Shortcut.CtrlShiftI : widget.AddAccelerator(signal, group, (uint)Gdk.Key.I, Gdk.ModifierType.ShiftMask|Gdk.ModifierType.ControlMask, Gtk.AccelFlags.Visible); break;
			case Shortcut.CtrlShiftJ : widget.AddAccelerator(signal, group, (uint)Gdk.Key.J, Gdk.ModifierType.ShiftMask|Gdk.ModifierType.ControlMask, Gtk.AccelFlags.Visible); break;
			case Shortcut.CtrlShiftK : widget.AddAccelerator(signal, group, (uint)Gdk.Key.K, Gdk.ModifierType.ShiftMask|Gdk.ModifierType.ControlMask, Gtk.AccelFlags.Visible); break;
			case Shortcut.CtrlShiftL : widget.AddAccelerator(signal, group, (uint)Gdk.Key.L, Gdk.ModifierType.ShiftMask|Gdk.ModifierType.ControlMask, Gtk.AccelFlags.Visible); break;
			case Shortcut.CtrlShiftM : widget.AddAccelerator(signal, group, (uint)Gdk.Key.M, Gdk.ModifierType.ShiftMask|Gdk.ModifierType.ControlMask, Gtk.AccelFlags.Visible); break;
			case Shortcut.CtrlShiftN : widget.AddAccelerator(signal, group, (uint)Gdk.Key.N, Gdk.ModifierType.ShiftMask|Gdk.ModifierType.ControlMask, Gtk.AccelFlags.Visible); break;
			case Shortcut.CtrlShiftO : widget.AddAccelerator(signal, group, (uint)Gdk.Key.O, Gdk.ModifierType.ShiftMask|Gdk.ModifierType.ControlMask, Gtk.AccelFlags.Visible); break;
			case Shortcut.CtrlShiftP : widget.AddAccelerator(signal, group, (uint)Gdk.Key.P, Gdk.ModifierType.ShiftMask|Gdk.ModifierType.ControlMask, Gtk.AccelFlags.Visible); break;
			case Shortcut.CtrlShiftQ : widget.AddAccelerator(signal, group, (uint)Gdk.Key.Q, Gdk.ModifierType.ShiftMask|Gdk.ModifierType.ControlMask, Gtk.AccelFlags.Visible); break;
			case Shortcut.CtrlShiftR : widget.AddAccelerator(signal, group, (uint)Gdk.Key.R, Gdk.ModifierType.ShiftMask|Gdk.ModifierType.ControlMask, Gtk.AccelFlags.Visible); break;
			case Shortcut.CtrlShiftS : widget.AddAccelerator(signal, group, (uint)Gdk.Key.S, Gdk.ModifierType.ShiftMask|Gdk.ModifierType.ControlMask, Gtk.AccelFlags.Visible); break;
			case Shortcut.CtrlShiftT : widget.AddAccelerator(signal, group, (uint)Gdk.Key.T, Gdk.ModifierType.ShiftMask|Gdk.ModifierType.ControlMask, Gtk.AccelFlags.Visible); break;
			case Shortcut.CtrlShiftU : widget.AddAccelerator(signal, group, (uint)Gdk.Key.U, Gdk.ModifierType.ShiftMask|Gdk.ModifierType.ControlMask, Gtk.AccelFlags.Visible); break;
			case Shortcut.CtrlShiftV : widget.AddAccelerator(signal, group, (uint)Gdk.Key.V, Gdk.ModifierType.ShiftMask|Gdk.ModifierType.ControlMask, Gtk.AccelFlags.Visible); break;
			case Shortcut.CtrlShiftW : widget.AddAccelerator(signal, group, (uint)Gdk.Key.W, Gdk.ModifierType.ShiftMask|Gdk.ModifierType.ControlMask, Gtk.AccelFlags.Visible); break;
			case Shortcut.CtrlShiftX : widget.AddAccelerator(signal, group, (uint)Gdk.Key.X, Gdk.ModifierType.ShiftMask|Gdk.ModifierType.ControlMask, Gtk.AccelFlags.Visible); break;
			case Shortcut.CtrlShiftY : widget.AddAccelerator(signal, group, (uint)Gdk.Key.Y, Gdk.ModifierType.ShiftMask|Gdk.ModifierType.ControlMask, Gtk.AccelFlags.Visible); break;
			case Shortcut.CtrlShiftZ : widget.AddAccelerator(signal, group, (uint)Gdk.Key.Z, Gdk.ModifierType.ShiftMask|Gdk.ModifierType.ControlMask, Gtk.AccelFlags.Visible); break;

			case Shortcut.CtrlShiftF1 : widget.AddAccelerator(signal, group, (uint)Gdk.Key.F1, Gdk.ModifierType.ShiftMask|Gdk.ModifierType.ControlMask, Gtk.AccelFlags.Visible); break; 
			case Shortcut.CtrlShiftF2 : widget.AddAccelerator(signal, group, (uint)Gdk.Key.F2, Gdk.ModifierType.ShiftMask|Gdk.ModifierType.ControlMask, Gtk.AccelFlags.Visible); break;
			case Shortcut.CtrlShiftF3 : widget.AddAccelerator(signal, group, (uint)Gdk.Key.F3, Gdk.ModifierType.ShiftMask|Gdk.ModifierType.ControlMask, Gtk.AccelFlags.Visible); break;
			case Shortcut.CtrlShiftF4 : widget.AddAccelerator(signal, group, (uint)Gdk.Key.F4, Gdk.ModifierType.ShiftMask|Gdk.ModifierType.ControlMask, Gtk.AccelFlags.Visible); break;
			case Shortcut.CtrlShiftF5 : widget.AddAccelerator(signal, group, (uint)Gdk.Key.F5, Gdk.ModifierType.ShiftMask|Gdk.ModifierType.ControlMask, Gtk.AccelFlags.Visible); break;
			case Shortcut.CtrlShiftF6 : widget.AddAccelerator(signal, group, (uint)Gdk.Key.F6, Gdk.ModifierType.ShiftMask|Gdk.ModifierType.ControlMask, Gtk.AccelFlags.Visible); break;
			case Shortcut.CtrlShiftF7 : widget.AddAccelerator(signal, group, (uint)Gdk.Key.F7, Gdk.ModifierType.ShiftMask|Gdk.ModifierType.ControlMask, Gtk.AccelFlags.Visible); break;
			case Shortcut.CtrlShiftF8 : widget.AddAccelerator(signal, group, (uint)Gdk.Key.F8, Gdk.ModifierType.ShiftMask|Gdk.ModifierType.ControlMask, Gtk.AccelFlags.Visible); break;
			case Shortcut.CtrlShiftF9 : widget.AddAccelerator(signal, group, (uint)Gdk.Key.F9, Gdk.ModifierType.ShiftMask|Gdk.ModifierType.ControlMask, Gtk.AccelFlags.Visible); break;
			case Shortcut.CtrlShiftF10 : widget.AddAccelerator(signal, group, (uint)Gdk.Key.F10, Gdk.ModifierType.ShiftMask|Gdk.ModifierType.ControlMask, Gtk.AccelFlags.Visible); break; 
			case Shortcut.CtrlShiftF11 : widget.AddAccelerator(signal, group, (uint)Gdk.Key.F11, Gdk.ModifierType.ShiftMask|Gdk.ModifierType.ControlMask, Gtk.AccelFlags.Visible); break;
			case Shortcut.CtrlShiftF12 : widget.AddAccelerator(signal, group, (uint)Gdk.Key.F12, Gdk.ModifierType.ShiftMask|Gdk.ModifierType.ControlMask, Gtk.AccelFlags.Visible); break;

// TODO: 
//		Del = 46,
//		F1 = 112,
//		F2 = 113,
//		F3 = 114,
//		F4 = 115,
//		F5 = 116,
//		F6 = 117,
//		F7 = 118,
//		F8 = 119,
//		F9 = 120,
//		F10 = 121,
//		F11 = 122,
//		F12 = 123,
//		Ins = 45,
//		None = 0,

//		Alt0 = 262192,
//		Alt1 = 262193,
//		Alt2 = 262194,
//		Alt3 = 262195,
//		Alt4 = 262196,
//		Alt5 = 262197,
//		Alt6 = 262198,
//		Alt7 = 262199,
//		Alt8 = 262200,
//		Alt9 = 262201,
//		AltBksp = 262152,
//		AltF1 = 262256,
//		AltF2 = 262257,
//		AltF3 = 262258,
//		AltF4 = 262259,
//		AltF5 = 262260,
//		AltF6 = 262261,
//		AltF7 = 262262,
//		AltF8 = 262263,
//		AltF9 = 262264,
//		AltF10 = 262265,
//		AltF11 = 262266,
//		AltF12 = 262267,
			}
		}
	}

	public enum Shortcut {
		Alt0 = 262192,
		Alt1 = 262193,
		Alt2 = 262194,
		Alt3 = 262195,
		Alt4 = 262196,
		Alt5 = 262197,
		Alt6 = 262198,
		Alt7 = 262199,
		Alt8 = 262200,
		Alt9 = 262201,
		AltBksp = 262152,
		AltF1 = 262256,
		AltF2 = 262257,
		AltF3 = 262258,
		AltF4 = 262259,
		AltF5 = 262260,
		AltF6 = 262261,
		AltF7 = 262262,
		AltF8 = 262263,
		AltF9 = 262264,
		AltF10 = 262265,
		AltF11 = 262266,
		AltF12 = 262267,
		Ctrl0 = 131120,
		Ctrl1 = 131121,
		Ctrl2 = 131122,
		Ctrl3 = 131123,
		Ctrl4 = 131124,
		Ctrl5 = 131125,
		Ctrl6 = 131126,
		Ctrl7 = 131127,
		Ctrl8 = 131128,
		Ctrl9 = 131129,
		CtrlA = 131137,
		CtrlB = 131138,
		CtrlC = 131139,
		CtrlD = 131140,
		CtrlDel = 131118,
		CtrlE = 131141,
		CtrlF = 131142,
		CtrlF1 = 131184,
		CtrlF2 = 131185,
		CtrlF3 = 131186,
		CtrlF4 = 131187,
		CtrlF5 = 131188,
		CtrlF6 = 131189,
		CtrlF7 = 131190,
		CtrlF8 = 131191,
		CtrlF9 = 131192,
		CtrlF10 = 131193,
		CtrlF11 = 131194,
		CtrlF12 = 131195,
		CtrlG = 131143,
		CtrlH = 131144,
		CtrlI = 131145,
		CtrlIns = 131117,
		CtrlJ = 131146,
		CtrlK = 131147,
		CtrlL = 131148,
		CtrlM = 131149,
		CtrlN = 131150,
		CtrlO = 131151,
		CtrlP = 131152,
		CtrlQ = 131153,
		CtrlR = 131154,
		CtrlS = 131155,
		CtrlT = 131156,
		CtrlU = 131157,
		CtrlV = 131158,
		CtrlW = 131159,
		CtrlX = 131160,
		CtrlY = 131161,
		CtrlZ = 131162,
		CtrlShift0 = 196656,
		CtrlShift1 = 196657,
		CtrlShift2 = 196658,
		CtrlShift3 = 196659,
		CtrlShift4 = 196660,
		CtrlShift5 = 196661,
		CtrlShift6 = 196662,
		CtrlShift7 = 196663,
		CtrlShift8 = 196664,
		CtrlShift9 = 196665,
		CtrlShiftA = 196673,
		CtrlShiftB = 196674,
		CtrlShiftC = 196675,
		CtrlShiftD = 196676,
		CtrlShiftE = 196677,
		CtrlShiftF = 196678,
		CtrlShiftF1 = 196720,
		CtrlShiftF2 = 196721,
		CtrlShiftF3 = 196722,
		CtrlShiftF4 = 196723,
		CtrlShiftF5 = 196724,
		CtrlShiftF6 = 196725,
		CtrlShiftF7 = 196726,
		CtrlShiftF8 = 196727,
		CtrlShiftF9 = 196728,
		CtrlShiftF10 = 196729,
		CtrlShiftF11 = 196730,
		CtrlShiftF12 = 196731,
		CtrlShiftG = 196679,
		CtrlShiftH = 196680,
		CtrlShiftI = 196681,
		CtrlShiftJ = 196682,
		CtrlShiftK = 196683,
		CtrlShiftL = 196684,
		CtrlShiftM = 196685,
		CtrlShiftN = 196686,
		CtrlShiftO = 196687,
		CtrlShiftP = 196688,
		CtrlShiftQ = 196689,
		CtrlShiftR = 196690,
		CtrlShiftS = 196691,
		CtrlShiftT = 196692,
		CtrlShiftU = 196693,
		CtrlShiftV = 196694,
		CtrlShiftW = 196695,
		CtrlShiftX = 196696,
		CtrlShiftY = 196697,
		CtrlShiftZ = 196698,
		Del = 46,
		F1 = 112,
		F2 = 113,
		F3 = 114,
		F4 = 115,
		F5 = 116,
		F6 = 117,
		F7 = 118,
		F8 = 119,
		F9 = 120,
		F10 = 121,
		F11 = 122,
		F12 = 123,
		Ins = 45,
		None = 0,
		ShiftDel = 65582,
		ShiftF1 = 65648,
		ShiftF2 = 65649,
		ShiftF3 = 65650,
		ShiftF4 = 65651,
		ShiftF5 = 65652,
		ShiftF6 = 65653,
		ShiftF7 = 65654,
		ShiftF8 = 65655,
		ShiftF9 = 65656,
		ShiftF10 = 65657,
		ShiftF11 = 65658,
		ShiftF12 = 65659,
		ShiftIns = 65581
	}
}
