Imports System.Windows.Forms

Module Test

Sub MySub(b)
	System.Console.WriteLine (b)
End Sub

Sub Main()
	Dim a as Integer
	Dim fgh as Integer
	Dim btn as Button
	Dim frm as TestClass
	
	System.Console.WriteLine ("This var ")
	System.Console.WriteLine ("contains ")
	a = (1 + 2) * 144
	
	System.Console.WriteLine (a)
	a = 1
	If (a > 2) Then
		System.Console.WriteLine ("Greater than 2")
	Else	
		System.Console.WriteLine ("Less than 2")
	End If
	
	a = 3
	If (a > 2) Then
		System.Console.WriteLine ("Greater than 2")
	Else	
		System.Console.WriteLine ("Less than 2")
	End If	
	
	frm = new TestClass("a")
	frm.Width = 300
	frm.Height = 80
	frm.PutButtonOnForm()
	frm.button1.Text = "AAA"
	frm.ShowDialog()
	MySub("parameter!!!!!!")
End Sub

End Module
