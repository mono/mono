Imports Microsoft.VisualBasic 
Imports System
Public Class TestClass 
	Public Function Test() As Integer
		'Begin Code
			Dim a As Integer =  InStr ("", "Hello")
			If a <> 0 Then
				Throw New Exception ("Expected 0 got " + a)
			End If
			Return a
		'End Code
	End Function
End Class
