Imports Microsoft.VisualBasic 
Imports System
Public Class TestClass 
	Public Function Test() As Integer
		'Begin Code
			Dim a As String = "Hello"
			Return InStr (1,a,"ell", CompareMethod.Binary)
		'End Code
	End Function
End Class
