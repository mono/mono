Imports Microsoft.VisualBasic 
Imports System
Public Class TestClass 
	Public Function Test() As String
		'Begin Code
			Dim result() As String = Split ("HelloHelloHelloHello","ELLO", ,CompareMethod.Text)
			If result.Length <> 5 Then
				Throw New Exception ("#Split01: Expected 5 but got " + (result.Length).ToString ())
			End If
			If Len (result (0)) <> 1 Then
				Throw New Exception ("#Split02: Expected 1 but got " + Len(result (0)).ToString ())
			End If
			Return result.Length
		'End Code
	End Function
End Class
