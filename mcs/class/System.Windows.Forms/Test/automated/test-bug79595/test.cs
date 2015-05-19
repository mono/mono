using System;
using System.Windows.Forms;

public class principal:Form {

	Button boton1;
	Button boton2;

	public principal() {

		boton1 = new Button();
		boton1.Text = "Click here";
		boton1.Top = 10;
		boton1.Click += new EventHandler(click1);
		this.Controls.Add(boton1);
	}
	
	public void click1(object sender, EventArgs e) {
	
		// here, the form should be cleared, erasing all the controls,
		// but it didn't work. This is the bug
		
		this.Controls.Clear();
		boton2 = new Button();
		boton2.Text = "Exit test";
		boton2.Top = 60; // put it lower to be sure that it won't overlap the old button
		boton2.Click += new EventHandler(click2);
		this.Controls.Add(boton2);
	}
	
	public void click2(object sender, EventArgs e) {
	
		// exit the demo
		
		this.Close();
	
	}
	
	static public void Main() {
	
		principal classdemo = new principal();
		Application.Run(classdemo);
	
	}
	
}
