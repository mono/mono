Imports Microsoft.VisualBasic 
Imports System
Public Class TestClass 
	Public Function Test() As String
		'Begin Code
			FileOpen (1, "5204.txt", OpenMode.Output)
			WriteLine (1,"Hello", " ", "World"," ","!")
			FileClose (1)

		'End Code
	End Function
End Class
