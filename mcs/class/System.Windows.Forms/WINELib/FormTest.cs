using System;
using System.Windows.Forms;

// Test basic functionality of the Application and Form class
class FormTest : Form
{
	public class DoubleClickMessageFilter : IMessageFilter {

		public bool PreFilterMessage(ref Message m)
		{
			Console.WriteLine ("PreFilter(ing) message");
			
			if (m.Msg == Win32.WM_MOUSEMOVE) {
				Console.WriteLine ("captured mousemove");
				return true;
			}
			return false;
		}
	}

	// - verifies the WndProc can be overridden propery 
	// - verifies the Application.MessageLoop is working properly
	protected override void WndProc (ref Message m)
	{
		base.WndProc (ref m);

		// should be true after the Run command is reached
		Console.WriteLine ("Application.MessageLoop: " + 
				   Application.MessageLoop);
	}

	static public void Test1 ()
	{
		MessageBox.Show ("test derived form");
		FormTest form = new FormTest ();
		DoubleClickMessageFilter f = new DoubleClickMessageFilter();
		Application.AddMessageFilter (f);

                // should be false
                Console.WriteLine ("Application.MessageLoop: " +
				   Application.MessageLoop);

		Application.Run (form);
		Application.RemoveMessageFilter (f);
	}

	static public void Test2 ()
	{
		MessageBox.Show ("test non-derived form, ctrl-c from console to quit");
		Form form = new Form ();
		form.Show ();
		Application.DoEvents ();
		Application.Run ();
	}
	
 	static public int Main (String[] args)
	{
		Test1();
		Test2();
		return 0;
	}
}
