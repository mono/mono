Imports Microsoft.VisualBasic 
Imports System
Public Class TestClass 
	Public Function Test() As String
		'Begin Code
			FileOpen (1, "5234.txt", OpenMode.Output)
			PrintLine (1, "Hello","Hello after a tab", TAB(2), "Hello after 2 tabs")
		'End Code
	End Function
End Class
