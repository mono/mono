Imports Microsoft.VisualBasic 
Imports System
Public Class TestClass 
	Public Function Test() As String
		'Begin Code
			Try
				Dim result As String = StrConv ("hello world", -1)
				Throw New Exception ("#StrConv01")
			Catch e As Exception
				If (e.GetType ().ToString ()) <> "System.ArgumentException" Then	
					Throw New Exception ("#StrConv02: Expected ArgumentException but got " + e.GetType ().ToString ())
				End If
			End Try
		'End Code
	End Function
End Class
