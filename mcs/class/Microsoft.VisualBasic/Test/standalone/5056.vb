Imports Microsoft.VisualBasic 
Imports System
Public Class TestClass 
	Public Function Test() As String
		'Begin Code
			Dim a As String =  ErrorToString (0)
			If a <> "" Then
				Throw New Exception ("#ErrorToString01")
			End If
			Return a
		'End Code
	End Function
End Class
