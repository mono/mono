using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections;

class MyMenuItem : MenuItem
{
	public MyMenuItem( string s) : base(s)
	{
	}
	
	public int GetID()
	{
		return MenuID;
	}
	
	public new IntPtr Handle 
	{
		get 
		{
			return IntPtr.Zero;
		}
	}
	
}

// Test basic functionality of the Application and Form class
class MenuTest : Form 
{

	Button 		button;
	MainMenu	testMenu_ = null;

	public MenuTest () : base ()
	{
		ClientSize = new Size(300, 250);
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
	
	MenuItem[] RecentFilesMenu()
	{
		MenuItem menuItem1 = new MenuItem("MenuTest.cs");
		MenuItem menuItem2 = new MenuItem("/etc/passwd");
		MenuItem menuItem3 = new MenuItem("~/.wine/config");
		return new MenuItem[3] {menuItem1, menuItem2, menuItem3};
	}

	// Doesn't gets called, waiting for Button implementation
	void OnMenuButtonClick( object c, EventArgs e)
	{
		if( Menu != null) 
		{
			Menu = null;
		}
		else 
		{
			Menu = testMenu_;
		}
	}
	
	MenuItem menuItem4 = null;

	public void CreateMyMainMenu()
	{
		testMenu_ = new MainMenu();

		MyMenuItem myMI = new MyMenuItem("2");
		MenuItem refMi = myMI;
		IntPtr ip = refMi.Handle;
		ip = myMI.Handle;
		System.Console.WriteLine("My menu ID {0}", myMI.GetID());
		
		MenuItem menuItem1 = new MenuItem("&New", new System.EventHandler(this.OnFileNew));
		MenuItem menuItem2 = new MenuItem("&Open...", new System.EventHandler(this.OnFileOpen));
		MenuItem menuItem3 = new MenuItem("&Quit", new System.EventHandler(this.OnFileQuit));
		MenuItem recentFiles = new MenuItem("Recent files", RecentFilesMenu());
		MenuItem FileMenu = new MenuItem("File", new MenuItem[]{menuItem1, menuItem2,recentFiles, menuItem3});

		myMI = new MyMenuItem("2");
		System.Console.WriteLine("My menu ID {0}", myMI.GetID());
		
		menuItem1.Text = "&File";
		menuItem2.Text = "&Edit";
		menuItem3.Text = "E&xit";

		MenuItem mi10 = new MenuItem("Dos");
		MenuItem mi11 = new MenuItem("Unix");
		menuItem4 = new MenuItem("&Save As...", new MenuItem[]{mi10, mi11});
		FileMenu.MenuItems.Add(2, menuItem4);
		int pos = testMenu_.MenuItems.Add(FileMenu);
		System.Console.WriteLine("Menu File added at position {0}", pos);

		MenuItem menuTest1 = new MenuItem("&Test properties", new System.EventHandler(this.OnTestProperties));
		MenuItem TestMenu = new MenuItem("Test", new MenuItem[]{menuTest1});
		testMenu_.MenuItems.Add(TestMenu);

		Menu = testMenu_;
		
		myMI = new MyMenuItem("2");
		System.Console.WriteLine("My menu ID {0}", myMI.GetID());
	}
	
	protected void OnFileNew( object sender, System.EventArgs e)
	{
		MessageBox.Show(this, "The File->New command selected", "MenuTest");
		menuItem4.Click += new System.EventHandler( this.OnFileSaveAs);
		menuItem4.MenuItems.Clear();
	}

	protected void OnFileOpen( object sender, System.EventArgs e)
	{
		MessageBox.Show(this, "A file-open dialog will appear soon", "MenuTest");
	}

	protected void OnFileQuit( object sender, System.EventArgs e)
	{
		System.Console.WriteLine("The Exit command selected");
		Application.Exit();
	}

	protected void OnFileSaveAs( object sender, System.EventArgs e)
	{
		MessageBox.Show("OnFileSaveAs");
		menuItem4.Index = 0;
	}

	protected void OnTestProperties( object sender, System.EventArgs e)
	{
		MenuItem send = sender as MenuItem;
		if( send != null)
		{
			Menu parent = send.Parent;
			if( parent != null){
				MenuItem mi1 = new MenuItem("BarBreak");
				mi1.BarBreak = true;
				MenuItem mi2 = new MenuItem("Break");
				mi2.Break = true;
				MenuItem mi3 = new MenuItem("Checked");
				mi3.Checked = true;
				MenuItem mi4 = new MenuItem("Disabled");
				mi4.Enabled = false;
				MenuItem mi5 = new MenuItem("DefaultItem");
				mi5.DefaultItem = true;
				MenuItem mi6 = new MenuItem("RadioCheck");
				mi6.RadioCheck = true;
				mi6.Checked = true;
				MenuItem mi7 = new MenuItem("-");
				mi7.RadioCheck = true;

				MenuItem SubMenu = new MenuItem("SubMenu", new MenuItem[]{mi1, mi2, mi3, mi4, mi5, mi6, mi7});
				parent.MenuItems.Add(SubMenu);
			}
		}
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

	static public void Test2()
	{
		MenuItem mi = new MyMenuItem("123");
		MenuItem mp = new MenuItem("PPP", new MenuItem[] { mi });
		MenuItem mc = mi.CloneMenu();
		
		System.Console.WriteLine("Clone equals to original {0}", mc.Equals(mi));
		System.Console.WriteLine("Original Parent {0}", mi.Parent.ToString());
		System.Console.WriteLine("Clone Parent {0}", mc.Parent != null ? mc.Parent.ToString() : "<null>");
		System.Console.WriteLine("Clone Parent is the same {0}", mc.Parent == mi.Parent);
	}

	static public void Test3()
	{
		MenuItem mi1 = new MenuItem("123");
		MenuItem mi2 = new MenuItem("234");

		MenuItem parent = new MenuItem( "parent", new MenuItem[] { mi1, mi2});
		IList il = (IList)parent.MenuItems;
		System.Console.WriteLine("List of menu items IsReadOnly {0}, IsFixedSize {1}", il.IsReadOnly, il.IsFixedSize);
		il.Add(new MenuItem("This must be inside"));
		//il.Add( new ArrayList());
		//il[1] = new MenuItem("345");
		//parent.MenuItems[1] = new MenuItem("asd");
	}

	static public int Main (String[] args)
	{
		Test3();
		Test2();
		Test1();
		return 0;
	}
}
