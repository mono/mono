Imports Microsoft.VisualBasic 
Imports System
Public Class TestClass 
	Public Function Test() As String
		'Begin Code
			FileOpen (1, "5210.txt", OpenMode.Output)
			Write (1)
			FileClose (1)
			Return "success"
		'End Code
	End Function
End Class
