Imports System.Windows.Forms
Imports System.Drawing

Delegate Sub d_b1_onClick (s, evt)

Public Class TestClass
	Inherits Form
			
Public withevents button1 as Button
		
Public Sub PutButtonOnForm()

End Sub

Public Sub b1_onClick_handler (a,b) Handles button1.Click

End Sub

Public Sub New(ctest_a as string)   
	Dim b1_onClick as d_b1_onClick 
	
	b1_onClick = New d_b1_onClick(AddressOf Me.b1_onClick_handler)
	
	Me.button1 = New Button()
	Me.button1.Text = "Click Me"
	Me.button1.Location = New Point(100, 10)
	'Me.Click = b1_onClick
	Me.Controls.Add(Me.button1)

	System.Console.WriteLine (ctest_a)
End Sub

End Class
