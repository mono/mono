using System;
using System.Windows.Forms;

class FormTest
{
 	static public void Main ()
	{
		Console.WriteLine ("Creating Form");
		Form form = new Form ();
		Application.Run (form);
	}
}

