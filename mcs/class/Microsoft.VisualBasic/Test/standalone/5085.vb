Imports Microsoft.VisualBasic 
Imports System
Public Class TestClass 
	Public Function Test() As Integer
		'Begin Code
			Dim a As Integer =  InStr (3, "Hello", "")
			If a <> 3 Then
				Throw New Exception ("Expected 3 got " + a.ToString ())
			End If
			Return a
		'End Code
	End Function
End Class
