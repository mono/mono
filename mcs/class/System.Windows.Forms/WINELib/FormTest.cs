using System;
using System.Windows.Forms;

class FormTest : Form
{
 	static public void Main ()
	{
		Console.WriteLine("Creating Form");
		FormTest form = new FormTest();
		Application.Run (form);
	}
}

