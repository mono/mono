Imports Microsoft.VisualBasic 
Imports System
Public Class TestClass 
	Public Function Test() As String
		'Begin Code
			FileOpen (1, "5216.txt", OpenMode.Output)
			PrintLine (1, "Hello", SPC(50), "World")
			PrintLine (1, "Hello", SPC(20), "World")
		'End Code
	End Function
End Class
