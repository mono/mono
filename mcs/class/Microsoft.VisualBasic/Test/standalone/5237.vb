Imports Microsoft.VisualBasic 
Imports System
Public Class TestClass 
	Public Function Test() As String
		'Begin Code
			Dim input_value as DateTime = DateTime.Now
			FileOpen (1, "5237.txt", OpenMode.Output)
			PrintLine (1, input_value) 
			FileClose (1)
		'End Code
	End Function
End Class
