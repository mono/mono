using System;
using System.Windows.Forms;

// Test basic functionality of the Application and Form class
class MenuTest : Form {

	Button 		button;
	MainMenu	testMenu_ = null;

	public MenuTest () : base ()
	{
		CreateMyMainMenu();

		button = new Button ();
		button.Top = 20;
		button.Left = 20;
		button.Width = 50;
		button.Height = 50;
		button.Parent = this;
		button.Text = "Menu";

		button.Click += new EventHandler(OnMenuButtonClick);

	}

	// Doesn't gets called, waiting for Button implementation
	void OnMenuButtonClick( object c, EventArgs e)
	{
		if( Menu != null) {
			Menu = null;
		}
		else {
			Menu = testMenu_;
		}
	}

	public void CreateMyMainMenu()
	{
		testMenu_ = new MainMenu();

		MenuItem menuItem1 = new MenuItem();
		MenuItem menuItem2 = new MenuItem();
		MenuItem menuItem3 = new MenuItem();

		menuItem1.Text = "&File";
		menuItem2.Text = "&Edit";
		menuItem3.Text = "E&xit";

		testMenu_.MenuItems.Add(menuItem1);
		testMenu_.MenuItems.Add(menuItem2);
		testMenu_.MenuItems.Add(menuItem3);

		Menu = testMenu_;
	}


	// - verifies the WndProc can be overridden propery
	// - verifies the Application.MessageLoop is working properly
	protected override void WndProc (ref Message m)
	{
		base.WndProc (ref m);

		// should be true after the Run command is reached
		//Console.WriteLine ("Application.MessageLoop: " +
				   //Application.MessageLoop);
	}

	static public void Test1 ()
	{
		MenuTest form = new MenuTest ();

		//should be false
		Console.WriteLine ("Application.MessageLoop: " +
		Application.MessageLoop);

		Application.Run (form);
	}


 	static public int Main (String[] args)
	{
		Test1();
		return 0;
	}
}
