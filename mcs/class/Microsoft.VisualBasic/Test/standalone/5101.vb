Imports Microsoft.VisualBasic 
Imports System
Public Class TestClass 
	Public Function Test() As String
		'Begin Code
			Dim result As String = LTrim ("     Hello     ")
			If Len (result) <> 10 Then
				Throw New Exception ("#LTrim01: Expected 10 but got " + Len (result).ToString ())
			End If
			Return result
		'End Code
	End Function
End Class
