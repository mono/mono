Imports Microsoft.VisualBasic 
Imports System
Public Class TestClass 
	Public Function Test() As String
		'Begin Code
			FileOpen (1, "5217.txt", OpenMode.Output)
			WriteLine (1, "Hello", SPC(-20), "World")
		'End Code
	End Function
End Class
