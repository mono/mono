Imports Microsoft.VisualBasic 
Imports System
Public Class TestClass 
	Public Function Test() As String
		'Begin Code
			Dim input_value as DateTime = DateTime.Now
			FileOpen (1, "5244.txt", OpenMode.Output)
			Print (1, input_value)
		'End Code
	End Function
End Class
