Imports Microsoft.VisualBasic 
Imports System
Public Class TestClass 
	Public Function Test() As String
		'Begin Code
			Dim result As String = UCase ("ABCDEFG")
			If result <> "ABCDEFG" then 
				Throw New Exception ("#UCase01: Expected 'ASDFGF' but got " + result)
			End if 
			Return result
		'End Code
	End Function
End Class
