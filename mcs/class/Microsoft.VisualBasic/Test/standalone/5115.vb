Imports Microsoft.VisualBasic 
Imports System
Public Class TestClass 
	Public Function Test() As String
		'Begin Code
			Dim result() As String = Split ("Hello World","")
			If result.Length <> 1 Then
				Throw New Exception ("#Split01: Expected 1 but got " + (result.Length).ToString ())
			End If
			If Len (result (0)) <> 11 Then
				Throw New Exception ("#Split02: Expected 11 but got " + Len(result (0)).ToString ())
			End If
			Return result.Length
		'End Code
	End Function
End Class
