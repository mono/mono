Imports Microsoft.VisualBasic 
Imports System
Public Class TestClass 
	Public Function Test() As Double
		'Begin Code
			'MS doc says AscW throws an exception when the string is nothing, however it returns 0 for the following
			Dim a As Object = Nothing
			Dim d As double = AscW (a)
			Return d
		'End Code
	End Function
End Class
