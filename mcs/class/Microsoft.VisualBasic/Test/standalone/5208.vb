Imports Microsoft.VisualBasic 
Imports System
Public Class TestClass 
	Public Function Test() As String
		'Begin Code
			FileOpen (1, "5208.txt", OpenMode.Output)
			Write (1, "Just a test")
			FileClose (1)

		'End Code
	End Function
End Class
