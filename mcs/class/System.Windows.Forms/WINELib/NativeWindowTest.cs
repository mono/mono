using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;


public class NativeWindowTest : NativeWindow {

	protected override void WndProc (ref Message m) 
	{
		Console.WriteLine ("in NativeWindowTest WndProc");

		m.Result = (IntPtr) 0;

		if (m.Msg == Win32.WM_DESTROY) {
			Console.WriteLine ("posting quit message");
			Win32.PostQuitMessage (0);
		} else
			DefWndProc (ref m);
	}
	
	static public void Main () {
		Console.WriteLine ("NativeWindow sample application begin");

		Console.WriteLine ("Creating NativeWindow");
		NativeWindowTest nw = new NativeWindowTest ();

		Console.WriteLine ("Creating CreateParams");
		CreateParams cp = new CreateParams ();
		  
		Console.WriteLine ("setting up CreateParams");

		cp.Caption = "Mono Native Window Test";
		cp.ClassName = "mono_native_window";
		cp.X = 10;
		cp.Y = 10;
		cp.Width = 640;
		cp.Height = 480;
		cp.ClassStyle = 0;
		cp.ExStyle = 0;
		cp.Param = 0;
		cp.Param = 0;
		cp.Style = (int) (
			Win32.WS_OVERLAPPEDWINDOW | Win32.WS_HSCROLL| 
			Win32.WS_VSCROLL);

		Console.WriteLine ("creating handle");
		nw.CreateHandle (cp);

		Console.WriteLine ("showing window");
		Win32.ShowWindow (nw.Handle, (int) Win32.SW_SHOW);

		Win32.MSG msg = new Win32.MSG();

		while (Win32.GetMessageA (ref msg, 0, 0, 0) != 0) {
			Win32.TranslateMessage (ref msg);
			Win32.DispatchMessageA (ref msg);
		}
	}
}
