Imports Microsoft.VisualBasic 
Imports System
Public Class TestClass 
	Public Function Test() As String
		'Begin Code
			On Error Resume Next
			Dim i As Integer = 0
			i = 10/i
			Return ErrorToString ()
		'End Code
	End Function
End Class
