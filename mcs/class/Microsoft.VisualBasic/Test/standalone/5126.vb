Imports Microsoft.VisualBasic 
Imports System
Public Class TestClass 
	Public Function Test() As Integer
		'Begin Code
			Dim result As Integer = StrComp ("Hello","HELLO")
			If result <> 1 Then
				Throw New Exception ("#StrComp01: Expected 1 but got " + result.ToString ())
			End If
			Return result
		'End Code
	End Function
End Class
