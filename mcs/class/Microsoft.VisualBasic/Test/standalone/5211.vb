Imports Microsoft.VisualBasic 
Imports System
Public Class TestClass 
	Public Function Test() As String
		'Begin Code
			FileOpen (1, "5211.txt", OpenMode.Output)
			Write (1,"Hello", " ", "World"," ","!")
			FileClose (1)

		'End Code
	End Function
End Class
