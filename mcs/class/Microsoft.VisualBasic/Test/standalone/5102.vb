Imports Microsoft.VisualBasic 
Imports System
Public Class TestClass 
	Public Function Test() As String
		'Begin Code
			Dim result As String = LTrim ("          ")
			If Len (result) <> 0 Then
				Throw New Exception ("#LTrim01: Expected 0 but got " + Len (result).ToString ())
			End If
			Return result
		'End Code
	End Function
End Class
