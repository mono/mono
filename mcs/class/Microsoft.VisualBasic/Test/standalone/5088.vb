Imports Microsoft.VisualBasic 
Imports System
Public Class TestClass 
	Public Function Test() As Integer
		'Begin Code
			Dim a As Integer =  InStrRev ("World `Hello World", "Hello", 100)
			If a <> 0 Then
				Throw New Exception ("Expected 0 got " + a.ToString ())
			End If
			Return a
		'End Code
	End Function
End Class
