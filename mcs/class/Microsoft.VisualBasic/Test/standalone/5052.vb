Imports Microsoft.VisualBasic 
Imports System
Public Class TestClass 
	Public Function Test() As String
		'Begin Code
			Try
				Dim a As String =  ErrorToString (65536)
				Throw New Exception ("#ErrorToString01")
			Catch e As Exception
				If (e.GetType ().ToString ()) <> "System.ArgumentException" Then
					Throw New Exception ("#ErrorToString02")
				End If
			End Try
		'End Code
	End Function
End Class
