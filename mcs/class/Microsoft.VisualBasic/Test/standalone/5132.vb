Imports Microsoft.VisualBasic 
Imports System
Public Class TestClass 
	Public Function Test() As String
		'Begin Code
			Dim result As String = StrConv ("Hello World", VbStrConv.LowerCase)
			If result <> "hello world" Then
				Throw New Exception ("#StrComp01: Expected 'hello world' but got " + result.ToString ())
			End If
			Return result
		'End Code
	End Function
End Class
