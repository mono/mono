Imports Microsoft.VisualBasic 
Imports System
Public Class TestClass 
	Public Function Test() As String
		'Begin Code
			Try
				Dim result As String = LSet ("Hello", -2)
				Throw New Exception ("#LSet01: Expected System.ArgumentOutOfRangeException but got none")
			Catch e As Exception 
				If (e.GetType ().ToString()) <> "System.ArgumentOutOfRangeException" Then
					Throw New Exception ("#LSet02: Expected System.ArgumentOutOfRangeException but got " + e.GetType ().ToString ())
				End If
			End Try
		'End Code
	End Function
End Class
