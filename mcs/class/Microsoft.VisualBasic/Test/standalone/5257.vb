Imports Microsoft.VisualBasic 
Imports System
Public Class TestClass 
	Public Function Test() As String
		'Begin Code
			dim result as String
			FileOpen (1, "5257.txt", OpenMode.Input)
			result = LineInput (1)
			Return result
		'End Code
	End Function
End Class
