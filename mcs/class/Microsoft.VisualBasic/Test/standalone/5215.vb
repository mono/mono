Imports Microsoft.VisualBasic 
Imports System
Public Class TestClass 
	Public Function Test() As String
		'Begin Code
			FileOpen (1, "5215.txt", OpenMode.Output)
			WriteLine (1, "Hello", TAB(50), "World")
			WriteLine (1, "Hello", TAB(), "World")
		'End Code
	End Function
End Class
