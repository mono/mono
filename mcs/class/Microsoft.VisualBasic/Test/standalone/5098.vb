Imports Microsoft.VisualBasic 
Imports System
Public Class TestClass 
	Public Function Test() As String
		'Begin Code
			Dim result As String = LSet ("Hellooooooooooo", 5)
			If result <> "Hello" Then
				Throw New Exception ("#Len01:Expected 'Hello' but got " + result)
			End If
			Return result
		'End Code
	End Function
End Class
