Imports Microsoft.VisualBasic 
Imports System
Public Class TestClass 
	Public Function Test() As String
		'Begin Code
			Try
				Dim result As String = FormatCurrency (234.34234, 100)
				Throw New Exception ("#FormatCurrency01")
			Catch e As Exception 
				If (e.GetType ().ToString ()) <> "System.ArgumentException" Then
					Throw New Exception ("#FormatCurrency01: Expected System.ArgumentException but got" + e.GetType ().ToString ())
				End If
			End Try
		'End Code
	End Function
End Class
