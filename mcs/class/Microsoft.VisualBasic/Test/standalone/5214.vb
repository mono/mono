Imports Microsoft.VisualBasic 
Imports System
Public Class TestClass 
	Public Function Test() As String
		'Begin Code
			FileOpen (1, "5214.txt", OpenMode.Output)
			Dim a As String = "Hello"
			Write (1, a)
			Dim b As Double = 100.25
			Write (1, b)
			FileClose (1)

		'End Code
	End Function
End Class
