Imports System.Windows.Forms
Imports System.Drawing
Imports System

Public Class Form1
	Inherits Form

Public WithEvents b1 As Button
Public WithEvents b2 As Button

Public Sub b1_onClick (sender As Object, e as System.EventArgs) 
	Console.WriteLine ("b1 clicked")
End Sub

Public Sub b2_onClick (sender As Object, e as System.EventArgs) 
	Console.WriteLine ("b2 clicked")
End Sub

Public Sub New()
	b1 = New Button()
	b1.Location = New Point(100, 10)
	Controls.Add (b1)
	AddHandler b1.Click, AddressOf b1_onClick
	
	b2 = New Button()
	b2.Location = New Point(100, 60)
	Controls.Add (b2)	
	AddHandler b2.Click, AddressOf b2_onClick
End Sub

End Class

