Imports Microsoft.VisualBasic 
Imports System
Public Class TestClass
	Public Function Test() As Integer
		'Begin Code
			On Error Resume Next
			Dim a As Integer = 0	
			Dim b As Integer = 20/a
			Return Erl	'Must return 0
		'End Code
	End Function
End Class
