Imports Microsoft.VisualBasic 
Imports System
Public Class TestClass 
	Public Function Test() As String
		'Begin Code
			Dim result As String = StrConv ("hello world", VbStrConv.ProperCase)
			If result <> "Hello World" Then
				Throw New Exception ("#StrComp01: Expected 'Hello World' but got " + result.ToString ())
			End If
			Return result
		'End Code
	End Function
End Class
