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

namespace System.Windows.Forms{

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
	
	
	
	
	
	[MonoTODO]
	internal class ShortcutHelper {
		public static void AddShortcutToWidget (Gtk.Widget widget, Gtk.AccelGroup group, Shortcut shortcut, string signal) {
			Gtk.AccelKey ak = new Gtk.AccelKey ();
			switch (shortcut) {
			case Shortcut.CtrlA : ak.accel_key = (uint)Gdk.Key.A;
				widget.AddAccelerator(signal, group, ak, Gdk.ModifierType.ControlMask, Gtk.AccelFlags.Visible); break;
			case Shortcut.CtrlB : ak.accel_key = (uint)Gdk.Key.B;
				widget.AddAccelerator(signal, group, ak, Gdk.ModifierType.ControlMask, Gtk.AccelFlags.Visible); break;
			case Shortcut.CtrlC : ak.accel_key = (uint)Gdk.Key.C;
				widget.AddAccelerator(signal, group, ak, Gdk.ModifierType.ControlMask, Gtk.AccelFlags.Visible); break;
			case Shortcut.CtrlD : ak.accel_key = (uint)Gdk.Key.D;
				widget.AddAccelerator(signal, group, ak, Gdk.ModifierType.ControlMask, Gtk.AccelFlags.Visible); break;
			case Shortcut.CtrlE : ak.accel_key = (uint)Gdk.Key.E;
				widget.AddAccelerator(signal, group, ak, Gdk.ModifierType.ControlMask, Gtk.AccelFlags.Visible); break;
			case Shortcut.CtrlF : ak.accel_key = (uint)Gdk.Key.F;
				widget.AddAccelerator(signal, group, ak, Gdk.ModifierType.ControlMask, Gtk.AccelFlags.Visible); break;
			case Shortcut.CtrlG : ak.accel_key = (uint)Gdk.Key.G;
				widget.AddAccelerator(signal, group, ak, Gdk.ModifierType.ControlMask, Gtk.AccelFlags.Visible); break;
			case Shortcut.CtrlH : ak.accel_key = (uint)Gdk.Key.H;
				widget.AddAccelerator(signal, group, ak, Gdk.ModifierType.ControlMask, Gtk.AccelFlags.Visible); break;
			case Shortcut.CtrlI : ak.accel_key = (uint)Gdk.Key.I;
				widget.AddAccelerator(signal, group, ak, Gdk.ModifierType.ControlMask, Gtk.AccelFlags.Visible); break;
			case Shortcut.CtrlJ : ak.accel_key = (uint)Gdk.Key.J;
				widget.AddAccelerator(signal, group, ak, Gdk.ModifierType.ControlMask, Gtk.AccelFlags.Visible); break;
			case Shortcut.CtrlK : ak.accel_key = (uint)Gdk.Key.K;
				widget.AddAccelerator(signal, group, ak, Gdk.ModifierType.ControlMask, Gtk.AccelFlags.Visible); break;
			case Shortcut.CtrlL : ak.accel_key = (uint)Gdk.Key.L;
				widget.AddAccelerator(signal, group, ak, Gdk.ModifierType.ControlMask, Gtk.AccelFlags.Visible); break;
			case Shortcut.CtrlM : ak.accel_key = (uint)Gdk.Key.M;
				widget.AddAccelerator(signal, group, ak, Gdk.ModifierType.ControlMask, Gtk.AccelFlags.Visible); break;
			case Shortcut.CtrlN : ak.accel_key = (uint)Gdk.Key.N;
				widget.AddAccelerator(signal, group, ak, Gdk.ModifierType.ControlMask, Gtk.AccelFlags.Visible); break;
			case Shortcut.CtrlO : ak.accel_key = (uint)Gdk.Key.O;
				widget.AddAccelerator(signal, group, ak, Gdk.ModifierType.ControlMask, Gtk.AccelFlags.Visible); break;
			case Shortcut.CtrlP : ak.accel_key = (uint)Gdk.Key.P;
				widget.AddAccelerator(signal, group, ak, Gdk.ModifierType.ControlMask, Gtk.AccelFlags.Visible); break;
			case Shortcut.CtrlQ : ak.accel_key = (uint)Gdk.Key.Q;
				widget.AddAccelerator(signal, group, ak, Gdk.ModifierType.ControlMask, Gtk.AccelFlags.Visible); break;
			case Shortcut.CtrlR : ak.accel_key = (uint)Gdk.Key.R;
				widget.AddAccelerator(signal, group, ak, Gdk.ModifierType.ControlMask, Gtk.AccelFlags.Visible); break;
			case Shortcut.CtrlS : ak.accel_key = (uint)Gdk.Key.S;
				widget.AddAccelerator(signal, group, ak, Gdk.ModifierType.ControlMask, Gtk.AccelFlags.Visible); break;
			case Shortcut.CtrlT : ak.accel_key = (uint)Gdk.Key.T;
				widget.AddAccelerator(signal, group, ak, Gdk.ModifierType.ControlMask, Gtk.AccelFlags.Visible); break;
			case Shortcut.CtrlU : ak.accel_key = (uint)Gdk.Key.U;
				widget.AddAccelerator(signal, group, ak, Gdk.ModifierType.ControlMask, Gtk.AccelFlags.Visible); break;
			case Shortcut.CtrlV : ak.accel_key = (uint)Gdk.Key.V;
				widget.AddAccelerator(signal, group, ak, Gdk.ModifierType.ControlMask, Gtk.AccelFlags.Visible); break;
			case Shortcut.CtrlW : ak.accel_key = (uint)Gdk.Key.W;
				widget.AddAccelerator(signal, group, ak, Gdk.ModifierType.ControlMask, Gtk.AccelFlags.Visible); break;
			case Shortcut.CtrlX : ak.accel_key = (uint)Gdk.Key.X;
				widget.AddAccelerator(signal, group, ak, Gdk.ModifierType.ControlMask, Gtk.AccelFlags.Visible); break;
			case Shortcut.CtrlY : ak.accel_key = (uint)Gdk.Key.Y;
				widget.AddAccelerator(signal, group, ak, Gdk.ModifierType.ControlMask, Gtk.AccelFlags.Visible); break;
			case Shortcut.CtrlZ : ak.accel_key = (uint)Gdk.Key.Z;
				widget.AddAccelerator(signal, group, ak, Gdk.ModifierType.ControlMask, Gtk.AccelFlags.Visible); break;


			case Shortcut.Ctrl0 : ak.accel_key = (uint)Gdk.Key.KP_0;
				widget.AddAccelerator(signal, group, ak, Gdk.ModifierType.ControlMask, Gtk.AccelFlags.Visible); break;
			case Shortcut.Ctrl1 : ak.accel_key = (uint)Gdk.Key.KP_1;
				widget.AddAccelerator(signal, group, ak, Gdk.ModifierType.ControlMask, Gtk.AccelFlags.Visible); break;
			case Shortcut.Ctrl2 : ak.accel_key = (uint)Gdk.Key.KP_2;
				widget.AddAccelerator(signal, group, ak, Gdk.ModifierType.ControlMask, Gtk.AccelFlags.Visible); break;
			case Shortcut.Ctrl3 : ak.accel_key = (uint)Gdk.Key.KP_3;
				widget.AddAccelerator(signal, group, ak, Gdk.ModifierType.ControlMask, Gtk.AccelFlags.Visible); break;
			case Shortcut.Ctrl4 : ak.accel_key = (uint)Gdk.Key.KP_4;
				widget.AddAccelerator(signal, group, ak, Gdk.ModifierType.ControlMask, Gtk.AccelFlags.Visible); break;
			case Shortcut.Ctrl5 : ak.accel_key = (uint)Gdk.Key.KP_5;
				widget.AddAccelerator(signal, group, ak, Gdk.ModifierType.ControlMask, Gtk.AccelFlags.Visible); break;
			case Shortcut.Ctrl6 : ak.accel_key = (uint)Gdk.Key.KP_6;
				widget.AddAccelerator(signal, group, ak, Gdk.ModifierType.ControlMask, Gtk.AccelFlags.Visible); break;
			case Shortcut.Ctrl7 : ak.accel_key = (uint)Gdk.Key.KP_7;
				widget.AddAccelerator(signal, group, ak, Gdk.ModifierType.ControlMask, Gtk.AccelFlags.Visible); break;
			case Shortcut.Ctrl8 : ak.accel_key = (uint)Gdk.Key.KP_8;
				widget.AddAccelerator(signal, group, ak, Gdk.ModifierType.ControlMask, Gtk.AccelFlags.Visible); break;
			case Shortcut.Ctrl9 : ak.accel_key = (uint)Gdk.Key.KP_9;
				widget.AddAccelerator(signal, group, ak, Gdk.ModifierType.ControlMask, Gtk.AccelFlags.Visible); break;

			case Shortcut.CtrlDel : ak.accel_key = (uint)Gdk.Key.Delete;
				widget.AddAccelerator(signal, group, ak, Gdk.ModifierType.ControlMask, Gtk.AccelFlags.Visible); break;
			case Shortcut.CtrlIns : ak.accel_key = (uint)Gdk.Key.Insert;
				widget.AddAccelerator(signal, group, ak, Gdk.ModifierType.ControlMask, Gtk.AccelFlags.Visible); break;

			case Shortcut.CtrlF1 : ak.accel_key = (uint)Gdk.Key.F1;
				widget.AddAccelerator(signal, group, ak, Gdk.ModifierType.ControlMask, Gtk.AccelFlags.Visible); break; 
			case Shortcut.CtrlF2 : ak.accel_key = (uint)Gdk.Key.F2;
				widget.AddAccelerator(signal, group, ak, Gdk.ModifierType.ControlMask, Gtk.AccelFlags.Visible); break;
			case Shortcut.CtrlF3 : ak.accel_key = (uint)Gdk.Key.F3;
				widget.AddAccelerator(signal, group, ak, Gdk.ModifierType.ControlMask, Gtk.AccelFlags.Visible); break;
			case Shortcut.CtrlF4 : ak.accel_key = (uint)Gdk.Key.F4;
				widget.AddAccelerator(signal, group, ak, Gdk.ModifierType.ControlMask, Gtk.AccelFlags.Visible); break;
			case Shortcut.CtrlF5 : ak.accel_key = (uint)Gdk.Key.F5;
				widget.AddAccelerator(signal, group, ak, Gdk.ModifierType.ControlMask, Gtk.AccelFlags.Visible); break;
			case Shortcut.CtrlF6 : ak.accel_key = (uint)Gdk.Key.F6;
				widget.AddAccelerator(signal, group, ak, Gdk.ModifierType.ControlMask, Gtk.AccelFlags.Visible); break;
			case Shortcut.CtrlF7 : ak.accel_key = (uint)Gdk.Key.F7;
				widget.AddAccelerator(signal, group, ak, Gdk.ModifierType.ControlMask, Gtk.AccelFlags.Visible); break;
			case Shortcut.CtrlF8 : ak.accel_key = (uint)Gdk.Key.F8;
				widget.AddAccelerator(signal, group, ak, Gdk.ModifierType.ControlMask, Gtk.AccelFlags.Visible); break;
			case Shortcut.CtrlF9 : ak.accel_key = (uint)Gdk.Key.F9;
				widget.AddAccelerator(signal, group, ak, Gdk.ModifierType.ControlMask, Gtk.AccelFlags.Visible); break;
			case Shortcut.CtrlF10 : ak.accel_key = (uint)Gdk.Key.F10;
				widget.AddAccelerator(signal, group, ak, Gdk.ModifierType.ControlMask, Gtk.AccelFlags.Visible); break; 
			case Shortcut.CtrlF11 : ak.accel_key = (uint)Gdk.Key.F11;
				widget.AddAccelerator(signal, group, ak, Gdk.ModifierType.ControlMask, Gtk.AccelFlags.Visible); break;
			case Shortcut.CtrlF12 : ak.accel_key = (uint)Gdk.Key.F12;
				widget.AddAccelerator(signal, group, ak, Gdk.ModifierType.ControlMask, Gtk.AccelFlags.Visible); break;

			case Shortcut.ShiftDel : ak.accel_key = (uint)Gdk.Key.Delete;
				widget.AddAccelerator(signal, group, ak, Gdk.ModifierType.ShiftMask, Gtk.AccelFlags.Visible); break;
			case Shortcut.ShiftIns : ak.accel_key = (uint)Gdk.Key.Insert;
				widget.AddAccelerator(signal, group, ak, Gdk.ModifierType.ShiftMask, Gtk.AccelFlags.Visible); break;

			case Shortcut.ShiftF1 : ak.accel_key = (uint)Gdk.Key.F1;
				widget.AddAccelerator(signal, group, ak, Gdk.ModifierType.ShiftMask, Gtk.AccelFlags.Visible); break;
			case Shortcut.ShiftF2 : ak.accel_key = (uint)Gdk.Key.F2;
				widget.AddAccelerator(signal, group, ak, Gdk.ModifierType.ShiftMask, Gtk.AccelFlags.Visible); break;
			case Shortcut.ShiftF3 : ak.accel_key = (uint)Gdk.Key.F3;
				widget.AddAccelerator(signal, group, ak, Gdk.ModifierType.ShiftMask, Gtk.AccelFlags.Visible); break;
			case Shortcut.ShiftF4 : ak.accel_key = (uint)Gdk.Key.F4;
				widget.AddAccelerator(signal, group, ak, Gdk.ModifierType.ShiftMask, Gtk.AccelFlags.Visible); break;
			case Shortcut.ShiftF5 : ak.accel_key = (uint)Gdk.Key.F5;
				widget.AddAccelerator(signal, group, ak, Gdk.ModifierType.ShiftMask, Gtk.AccelFlags.Visible); break;
			case Shortcut.ShiftF6 : ak.accel_key = (uint)Gdk.Key.F6;
				widget.AddAccelerator(signal, group, ak, Gdk.ModifierType.ShiftMask, Gtk.AccelFlags.Visible); break;
			case Shortcut.ShiftF7 : ak.accel_key = (uint)Gdk.Key.F7;
				widget.AddAccelerator(signal, group, ak, Gdk.ModifierType.ShiftMask, Gtk.AccelFlags.Visible); break;
			case Shortcut.ShiftF8 : ak.accel_key = (uint)Gdk.Key.F8;
				widget.AddAccelerator(signal, group, ak, Gdk.ModifierType.ShiftMask, Gtk.AccelFlags.Visible); break;
			case Shortcut.ShiftF9 : ak.accel_key = (uint)Gdk.Key.F9;
				widget.AddAccelerator(signal, group, ak, Gdk.ModifierType.ShiftMask, Gtk.AccelFlags.Visible); break;
			case Shortcut.ShiftF10 : ak.accel_key = (uint)Gdk.Key.F10;
				widget.AddAccelerator(signal, group, ak, Gdk.ModifierType.ShiftMask, Gtk.AccelFlags.Visible); break;
			case Shortcut.ShiftF11 : ak.accel_key = (uint)Gdk.Key.F11;
				widget.AddAccelerator(signal, group, ak, Gdk.ModifierType.ShiftMask, Gtk.AccelFlags.Visible); break;
			case Shortcut.ShiftF12 : ak.accel_key = (uint)Gdk.Key.F12;
				widget.AddAccelerator(signal, group, ak, Gdk.ModifierType.ShiftMask, Gtk.AccelFlags.Visible); break;

			case Shortcut.CtrlShift0 : ak.accel_key = (uint)Gdk.Key.KP_0;
				widget.AddAccelerator(signal, group, ak, Gdk.ModifierType.ShiftMask|Gdk.ModifierType.ControlMask, Gtk.AccelFlags.Visible); break;
			case Shortcut.CtrlShift1 : ak.accel_key = (uint)Gdk.Key.KP_1;
				widget.AddAccelerator(signal, group, ak, Gdk.ModifierType.ShiftMask|Gdk.ModifierType.ControlMask, Gtk.AccelFlags.Visible); break;
			case Shortcut.CtrlShift2 : ak.accel_key = (uint)Gdk.Key.KP_2;
				widget.AddAccelerator(signal, group, ak, Gdk.ModifierType.ShiftMask|Gdk.ModifierType.ControlMask, Gtk.AccelFlags.Visible); break;
			case Shortcut.CtrlShift3 : ak.accel_key = (uint)Gdk.Key.KP_3;
				widget.AddAccelerator(signal, group, ak, Gdk.ModifierType.ShiftMask|Gdk.ModifierType.ControlMask, Gtk.AccelFlags.Visible); break;
			case Shortcut.CtrlShift4 : ak.accel_key = (uint)Gdk.Key.KP_4;
				widget.AddAccelerator(signal, group, ak, Gdk.ModifierType.ShiftMask|Gdk.ModifierType.ControlMask, Gtk.AccelFlags.Visible); break;
			case Shortcut.CtrlShift5 : ak.accel_key = (uint)Gdk.Key.KP_5;
				widget.AddAccelerator(signal, group, ak, Gdk.ModifierType.ShiftMask|Gdk.ModifierType.ControlMask, Gtk.AccelFlags.Visible); break;
			case Shortcut.CtrlShift6 : ak.accel_key = (uint)Gdk.Key.KP_6;
				widget.AddAccelerator(signal, group, ak, Gdk.ModifierType.ShiftMask|Gdk.ModifierType.ControlMask, Gtk.AccelFlags.Visible); break;
			case Shortcut.CtrlShift7 : ak.accel_key = (uint)Gdk.Key.KP_7;
				widget.AddAccelerator(signal, group, ak, Gdk.ModifierType.ShiftMask|Gdk.ModifierType.ControlMask, Gtk.AccelFlags.Visible); break;
			case Shortcut.CtrlShift8 : ak.accel_key = (uint)Gdk.Key.KP_8;
				widget.AddAccelerator(signal, group, ak, Gdk.ModifierType.ShiftMask|Gdk.ModifierType.ControlMask, Gtk.AccelFlags.Visible); break;
			case Shortcut.CtrlShift9 : ak.accel_key = (uint)Gdk.Key.KP_9;
				widget.AddAccelerator(signal, group, ak, Gdk.ModifierType.ShiftMask|Gdk.ModifierType.ControlMask, Gtk.AccelFlags.Visible); break;
			
			case Shortcut.CtrlShiftA : ak.accel_key = (uint)Gdk.Key.A;
				widget.AddAccelerator(signal, group, ak, Gdk.ModifierType.ShiftMask|Gdk.ModifierType.ControlMask, Gtk.AccelFlags.Visible); break;
			case Shortcut.CtrlShiftB : ak.accel_key = (uint)Gdk.Key.B;
				widget.AddAccelerator(signal, group, ak, Gdk.ModifierType.ShiftMask|Gdk.ModifierType.ControlMask, Gtk.AccelFlags.Visible); break;
			case Shortcut.CtrlShiftC : ak.accel_key = (uint)Gdk.Key.C;
				widget.AddAccelerator(signal, group, ak, Gdk.ModifierType.ShiftMask|Gdk.ModifierType.ControlMask, Gtk.AccelFlags.Visible); break;
			case Shortcut.CtrlShiftD : ak.accel_key = (uint)Gdk.Key.D;
				widget.AddAccelerator(signal, group, ak, Gdk.ModifierType.ShiftMask|Gdk.ModifierType.ControlMask, Gtk.AccelFlags.Visible); break;
			case Shortcut.CtrlShiftE : ak.accel_key = (uint)Gdk.Key.E;
				widget.AddAccelerator(signal, group, ak, Gdk.ModifierType.ShiftMask|Gdk.ModifierType.ControlMask, Gtk.AccelFlags.Visible); break;
			case Shortcut.CtrlShiftF : ak.accel_key = (uint)Gdk.Key.F;
				widget.AddAccelerator(signal, group, ak, Gdk.ModifierType.ShiftMask|Gdk.ModifierType.ControlMask, Gtk.AccelFlags.Visible); break;
			case Shortcut.CtrlShiftG : ak.accel_key = (uint)Gdk.Key.G;
				widget.AddAccelerator(signal, group, ak, Gdk.ModifierType.ShiftMask|Gdk.ModifierType.ControlMask, Gtk.AccelFlags.Visible); break;
			case Shortcut.CtrlShiftH : ak.accel_key = (uint)Gdk.Key.H;
				widget.AddAccelerator(signal, group, ak, Gdk.ModifierType.ShiftMask|Gdk.ModifierType.ControlMask, Gtk.AccelFlags.Visible); break;
			case Shortcut.CtrlShiftI : ak.accel_key = (uint)Gdk.Key.I;
				widget.AddAccelerator(signal, group, ak, Gdk.ModifierType.ShiftMask|Gdk.ModifierType.ControlMask, Gtk.AccelFlags.Visible); break;
			case Shortcut.CtrlShiftJ : ak.accel_key = (uint)Gdk.Key.J;
				widget.AddAccelerator(signal, group, ak, Gdk.ModifierType.ShiftMask|Gdk.ModifierType.ControlMask, Gtk.AccelFlags.Visible); break;
			case Shortcut.CtrlShiftK : ak.accel_key = (uint)Gdk.Key.K;
				widget.AddAccelerator(signal, group, ak, Gdk.ModifierType.ShiftMask|Gdk.ModifierType.ControlMask, Gtk.AccelFlags.Visible); break;
			case Shortcut.CtrlShiftL : ak.accel_key = (uint)Gdk.Key.L;
				widget.AddAccelerator(signal, group, ak, Gdk.ModifierType.ShiftMask|Gdk.ModifierType.ControlMask, Gtk.AccelFlags.Visible); break;
			case Shortcut.CtrlShiftM : ak.accel_key = (uint)Gdk.Key.M;
				widget.AddAccelerator(signal, group, ak, Gdk.ModifierType.ShiftMask|Gdk.ModifierType.ControlMask, Gtk.AccelFlags.Visible); break;
			case Shortcut.CtrlShiftN : ak.accel_key = (uint)Gdk.Key.N;
				widget.AddAccelerator(signal, group, ak, Gdk.ModifierType.ShiftMask|Gdk.ModifierType.ControlMask, Gtk.AccelFlags.Visible); break;
			case Shortcut.CtrlShiftO : ak.accel_key = (uint)Gdk.Key.O;
				widget.AddAccelerator(signal, group, ak, Gdk.ModifierType.ShiftMask|Gdk.ModifierType.ControlMask, Gtk.AccelFlags.Visible); break;
			case Shortcut.CtrlShiftP : ak.accel_key = (uint)Gdk.Key.P;
				widget.AddAccelerator(signal, group, ak, Gdk.ModifierType.ShiftMask|Gdk.ModifierType.ControlMask, Gtk.AccelFlags.Visible); break;
			case Shortcut.CtrlShiftQ : ak.accel_key = (uint)Gdk.Key.Q;
				widget.AddAccelerator(signal, group, ak, Gdk.ModifierType.ShiftMask|Gdk.ModifierType.ControlMask, Gtk.AccelFlags.Visible); break;
			case Shortcut.CtrlShiftR : ak.accel_key = (uint)Gdk.Key.R;
				widget.AddAccelerator(signal, group, ak, Gdk.ModifierType.ShiftMask|Gdk.ModifierType.ControlMask, Gtk.AccelFlags.Visible); break;
			case Shortcut.CtrlShiftS : ak.accel_key = (uint)Gdk.Key.S;
				widget.AddAccelerator(signal, group, ak, Gdk.ModifierType.ShiftMask|Gdk.ModifierType.ControlMask, Gtk.AccelFlags.Visible); break;
			case Shortcut.CtrlShiftT : ak.accel_key = (uint)Gdk.Key.T;
				widget.AddAccelerator(signal, group, ak, Gdk.ModifierType.ShiftMask|Gdk.ModifierType.ControlMask, Gtk.AccelFlags.Visible); break;
			case Shortcut.CtrlShiftU : ak.accel_key = (uint)Gdk.Key.U;
				widget.AddAccelerator(signal, group, ak, Gdk.ModifierType.ShiftMask|Gdk.ModifierType.ControlMask, Gtk.AccelFlags.Visible); break;
			case Shortcut.CtrlShiftV : ak.accel_key = (uint)Gdk.Key.V;
				widget.AddAccelerator(signal, group, ak, Gdk.ModifierType.ShiftMask|Gdk.ModifierType.ControlMask, Gtk.AccelFlags.Visible); break;
			case Shortcut.CtrlShiftW : ak.accel_key = (uint)Gdk.Key.W;
				widget.AddAccelerator(signal, group, ak, Gdk.ModifierType.ShiftMask|Gdk.ModifierType.ControlMask, Gtk.AccelFlags.Visible); break;
			case Shortcut.CtrlShiftX : ak.accel_key = (uint)Gdk.Key.X;
				widget.AddAccelerator(signal, group, ak, Gdk.ModifierType.ShiftMask|Gdk.ModifierType.ControlMask, Gtk.AccelFlags.Visible); break;
			case Shortcut.CtrlShiftY : ak.accel_key = (uint)Gdk.Key.Y;
				widget.AddAccelerator(signal, group, ak, Gdk.ModifierType.ShiftMask|Gdk.ModifierType.ControlMask, Gtk.AccelFlags.Visible); break;
			case Shortcut.CtrlShiftZ : ak.accel_key = (uint)Gdk.Key.Z;
				widget.AddAccelerator(signal, group, ak, Gdk.ModifierType.ShiftMask|Gdk.ModifierType.ControlMask, Gtk.AccelFlags.Visible); break;

			case Shortcut.CtrlShiftF1 : ak.accel_key = (uint)Gdk.Key.F1;
				widget.AddAccelerator(signal, group, ak, Gdk.ModifierType.ShiftMask|Gdk.ModifierType.ControlMask, Gtk.AccelFlags.Visible); break; 
			case Shortcut.CtrlShiftF2 : ak.accel_key = (uint)Gdk.Key.F2;
				widget.AddAccelerator(signal, group, ak, Gdk.ModifierType.ShiftMask|Gdk.ModifierType.ControlMask, Gtk.AccelFlags.Visible); break;
			case Shortcut.CtrlShiftF3 : ak.accel_key = (uint)Gdk.Key.F3;
				widget.AddAccelerator(signal, group, ak, Gdk.ModifierType.ShiftMask|Gdk.ModifierType.ControlMask, Gtk.AccelFlags.Visible); break;
			case Shortcut.CtrlShiftF4 : ak.accel_key = (uint)Gdk.Key.F4;
				widget.AddAccelerator(signal, group, ak, Gdk.ModifierType.ShiftMask|Gdk.ModifierType.ControlMask, Gtk.AccelFlags.Visible); break;
			case Shortcut.CtrlShiftF5 : ak.accel_key = (uint)Gdk.Key.F5;
				widget.AddAccelerator(signal, group, ak, Gdk.ModifierType.ShiftMask|Gdk.ModifierType.ControlMask, Gtk.AccelFlags.Visible); break;
			case Shortcut.CtrlShiftF6 : ak.accel_key = (uint)Gdk.Key.F6;
				widget.AddAccelerator(signal, group, ak, Gdk.ModifierType.ShiftMask|Gdk.ModifierType.ControlMask, Gtk.AccelFlags.Visible); break;
			case Shortcut.CtrlShiftF7 : ak.accel_key = (uint)Gdk.Key.F7;
				widget.AddAccelerator(signal, group, ak, Gdk.ModifierType.ShiftMask|Gdk.ModifierType.ControlMask, Gtk.AccelFlags.Visible); break;
			case Shortcut.CtrlShiftF8 : ak.accel_key = (uint)Gdk.Key.F8;
				widget.AddAccelerator(signal, group, ak, Gdk.ModifierType.ShiftMask|Gdk.ModifierType.ControlMask, Gtk.AccelFlags.Visible); break;
			case Shortcut.CtrlShiftF9 : ak.accel_key = (uint)Gdk.Key.F9;
				widget.AddAccelerator(signal, group, ak, Gdk.ModifierType.ShiftMask|Gdk.ModifierType.ControlMask, Gtk.AccelFlags.Visible); break;
			case Shortcut.CtrlShiftF10 : ak.accel_key = (uint)Gdk.Key.F10;
				widget.AddAccelerator(signal, group, ak, Gdk.ModifierType.ShiftMask|Gdk.ModifierType.ControlMask, Gtk.AccelFlags.Visible); break; 
			case Shortcut.CtrlShiftF11 : ak.accel_key = (uint)Gdk.Key.F11;
				widget.AddAccelerator(signal, group, ak, Gdk.ModifierType.ShiftMask|Gdk.ModifierType.ControlMask, Gtk.AccelFlags.Visible); break;
			case Shortcut.CtrlShiftF12 : ak.accel_key = (uint)Gdk.Key.F12;
				widget.AddAccelerator(signal, group, ak, Gdk.ModifierType.ShiftMask|Gdk.ModifierType.ControlMask, Gtk.AccelFlags.Visible); break;

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
}
