Imports System
Imports Gtk

Module GtkTest

    Sub Main()
        DIM Win as Window
        DIM Btn as Button
        
        Application.Init ()
        Win = new Window ("VB Gtk+ Hello World")
        Btn = new Button ("Click Me!") 
        Win.Add (Btn) 
        Win.ShowAll()
        Application.Run ()
    End Sub

End Module

