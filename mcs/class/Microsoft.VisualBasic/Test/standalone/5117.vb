Imports Microsoft.VisualBasic 
Imports System
Public Class TestClass 
	Public Function Test() As String
		'Begin Code
			Try
				Dim result() As String = Split ("HelloHelloHelloHello","ello",0)
				Throw New Exception ("#Split01")
			Catch e As Exception
				If (e.GetType ().ToString ()) <> "System.IndexOutOfRangeException" Then
					Throw New Exception ("#Split02 : Expected System.IndexOutOfRangeException but got " + e.GetType ().ToString ())
				End If
			End Try
		'End Code
	End Function
End Class
