Imports Microsoft.VisualBasic 
Imports System
Public Class TestClass
	Public Function Test() As Integer
		'Begin Code
			On Error Resume Next
			Err.Raise (-514)
			Return Err.Number
		'End Code
	End Function
End Class
