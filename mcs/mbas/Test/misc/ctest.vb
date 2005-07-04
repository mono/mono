Imports System.Windows.Forms

' Delegate Sub ClickHandler (s, evt)

Public Class TestClass
	Inherits Form
			
Public withevents button1 as Button


' TODO: omitting 'as System.EventArgs' in the second parameter gives: 
' 			error BC30408: Method 'System.Void OneClick (object, object)' does not match 
'			delegate 'System.Void System.EventHandler (object, System.EventArgs)'
' This probably isn't following the lax binding rules when Option Strict is Off
Public Sub OneClick (a, b as System.EventArgs) Handles button1.Click
	Me.button1.Text = "Clicked"
End Sub

'Public Sub AnotherClick (a, b)
'	Me.button1.Text = "Clicked Another Way"
'End Sub

Public sub PutButtonOnForm
	'Dim clicker as ClickHandler
	 
	' TODO: this line is giving:
	'			error BC-0100: Internal error: Could not find delegate constructor!
	'clicker = New ClickHandler(AddressOf Me.AnotherClick)
	
	Me.button1.Text = "Click Me"
	Me.button1.Location = New System.Drawing.Point(100, 10)
	
	'Me.button1.Click = clicker

	Me.Controls.Add(Me.button1)
	System.Console.WriteLine ("PutButtonOnForm")
End sub

Public Sub New(ctest_a as string)   
	Me.button1 = New Button()
	System.Console.WriteLine(ctest_a)
End Sub

End Class
