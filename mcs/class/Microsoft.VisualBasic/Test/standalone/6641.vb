Imports Microsoft.VisualBasic 
Imports System
Public Class TestClass 
	Public Function Test() As String
		'Begin Code
			Dim a(2) As String
			a (0) = "Dado"
			a (1) = "Ben"
			a (2) = "David"
			Dim result () As String = Filter(a,"Da",True,CompareMethod.Binary)
			Return result (1)
		'End Code
	End Function
End Class
