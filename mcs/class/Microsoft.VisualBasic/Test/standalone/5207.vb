Imports Microsoft.VisualBasic 
Imports System
Public Class TestClass 
	Public Function Test() As String
		'Begin Code
			FileOpen (1, "5207.txt", OpenMode.Output)
			Dim a As String = "Hello"
			WriteLine (1, a)
			Dim b As Double = 100.25
			WriteLine (1, b)
			FileClose (1)

		'End Code
	End Function
End Class
