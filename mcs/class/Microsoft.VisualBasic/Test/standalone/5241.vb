Imports Microsoft.VisualBasic 
Imports System
Public Class TestClass 
	Public Function Test() As String
		'Begin Code
			FileOpen (1, "5241.txt", OpenMode.Output)
			Print (1, "Hello")
			Print (1, "Hello after a tab", TAB(2), "Hello after 2 tabs")
		'End Code
	End Function
End Class
