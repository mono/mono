Imports Microsoft.VisualBasic 
Imports System
Public Class TestClass 
	Public Function Test() As String
		'Begin Code
			Dim a(2) As Object
			Dim b As New C ()
			a (0) = b
			a (1) = b
			a (2) = b
			Try
				Dim c As String = Join (a)
				Throw New Exception ("#Join01")
			Catch e As Exception
				If (e.GetType ().ToString ()) <> "System.ArgumentException" Then
					Throw New Exception ("#Join02: Expected ArgumentException but got " + e.GetType ().ToString ())
				End If
			End Try 
			
		'End Code
	End Function
End Class

Public Class C
End Class
