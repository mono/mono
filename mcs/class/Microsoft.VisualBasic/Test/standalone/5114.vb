Imports Microsoft.VisualBasic 
Imports System
Public Class TestClass 
	Public Function Test() As String
		'Begin Code
			Dim result() As String = Split (Nothing)
			If result.Length <> 1 Then
				Throw New Exception ("#Split01: Expected 1 but got " + (result.Length).ToString ())
			End If
			Return result.Length
		'End Code
	End Function
End Class
