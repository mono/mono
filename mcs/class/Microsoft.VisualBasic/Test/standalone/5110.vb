Imports Microsoft.VisualBasic 
Imports System
Public Class TestClass 
	Public Function Test() As String
		'Begin Code
			Try
				Dim result As String = Right ("Hello", -2)
				Throw New Exception ("#Right01: Expected System.ArgumentException but got none")
			Catch e As Exception 
				If (e.GetType ().ToString()) <> "System.ArgumentException" Then
					Throw New Exception ("#LSet02: Expected System.ArgumentException but got " + e.GetType ().ToString ())
				End If
			End Try
		'End Code
	End Function
End Class
