Imports Microsoft.VisualBasic 
Imports System
Public Class TestClass 
	Public Function Test() As String
		'Begin Code
			FileOpen (1, "5212.txt", OpenMode.Output)
			Dim a As Double = 24.5
			Write (1, a)
			FileClose (1)

		'End Code
	End Function
End Class
