Imports Microsoft.VisualBasic 
Imports System
Public Class TestClass
	Public Function Test() As Integer
		'Begin Code
			On Error Resume Next
			Dim a As Integer = 0
			a = 10 / a
			Return Err.Source
		'End Code
	End Function
End Class
