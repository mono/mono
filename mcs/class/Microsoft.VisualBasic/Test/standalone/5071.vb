Imports Microsoft.VisualBasic 
Imports System
Public Class TestClass 
	Public Function Test() As Char
		'Begin Code
			Dim a As String = "78"
			Dim d As Char = ChrW (a)
			Return d 	'Should return 'N'
		'End Code
	End Function
End Class
