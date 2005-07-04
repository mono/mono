Imports System
Imports Gtk

Module GtkTest

    public Win as Window
    public Btn as Button
        
    Sub Main()
        Application.Init ()
        Win = new Window ("VB Gtk+ Hello World")
        Btn = new Button ("Click Me! I'm awaiting for your click") 
		AddHandler Win.DeleteEvent, AddressOf OnQuit
		AddHandler Btn.Pressed, AddressOf OnPressed
        Win.Add (Btn) 
        Win.ShowAll()
        Application.Run ()
    End Sub
    
    sub OnPressed (sender as object, a as EventArgs)
    	Btn.Label = "Clicked by Someone!"
	end sub

	Sub OnQuit (sender as object, a as DeleteEventArgs)
		Application.Quit()
	end sub

End Module
