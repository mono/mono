using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;


public class NativeWindowTest : NativeWindow {

	protected override void WndProc (ref Message m) {
		Console.WriteLine ("in NativeWindowTest WndProc");
		if (m.Msg == Win32.WM_DESTROY) {
			Console.WriteLine ("posting quit message");
			Win32.PostQuitMessage (0);
		}
		DefWndProc (ref m);
	}

	static public void Main () {
		Console.WriteLine ("NativeWindow sample application begin");
		Console.WriteLine ("Creating NativeWindow");
		NativeWindow nw = new NativeWindow ();
		Console.WriteLine ("Creating CreateParams");
		CreateParams cp = new CreateParams ();
		  
		Console.WriteLine ("setting up CreateParams");

		cp.Caption = "Mono Native Window Test";
		cp.ClassName = "mono_wine_class";
		cp.X = 10;
		cp.Y = 10;
		cp.Width = 640;
		cp.Height = 480;
		cp.ClassStyle = 0;
		cp.ExStyle = 0;
		cp.Param = 0;
		cp.Param = 0;
		cp.Style = (int) Win32.WS_OVERLAPPEDWINDOW;

		Console.WriteLine ("creating handle");
		nw.CreateHandle (cp);
		Console.WriteLine ("showing window");
		Win32.ShowWindow (nw.Handle, (int) Win32.SW_SHOW);

		int msg;

		while (Win32.GetMessageA (ref msg, 0, 0, 0) != 0) {
			Win32.TranslateMessage (ref msg);
			Win32.DispatchMessageA (ref msg);
		}
	}
}
