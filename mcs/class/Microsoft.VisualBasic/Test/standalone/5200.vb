Imports Microsoft.VisualBasic 
Imports System
Public Class TestClass 
	Public Function Test() As String
		'Begin Code
			FileOpen (1, "5200.txt", OpenMode.Output)
			WriteLine (1, "Just a test")
			FileClose (1)

		'End Code
	End Function
End Class
