Imports Microsoft.VisualBasic 
Imports System
Public Class TestClass 
	Public Function Test() As String
		'Begin Code
			Dim result As String = Len (New String("Hello"))
			If result <> 5 Then
				Throw New Exception ("#Len01:Expected 5 but got " + result)
			End If
			Return result
		'End Code
	End Function
End Class
