Imports Microsoft.VisualBasic 
Imports System
Public Class TestClass 
	Public Function Test() As String
		'Begin Code
			dim result as String
			FileOpen(1, "5255.txt", OpenMode.Input)
			Input (1, result)
			Input (1, result)
			Dim length as Long = loc (1)
			FileClose(1)
			return length
		'End Code
	End Function
End Class
