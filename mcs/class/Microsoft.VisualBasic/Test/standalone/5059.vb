Imports Microsoft.VisualBasic 
Imports System
Public Class TestClass 
	Public Function Test() As Double
		'Begin Code
			Dim a As String = "Not a number"
			Try
				Dim b As Integer = Int (a)
				Throw New Exception ("#Int01")
			Catch e As Exception
				If (e.GetType ().ToString ()) <> "System.InvalidCastException" Then
					Throw New Exception ("#Int02:"+e.GetType ().ToString())
				End If
			End Try
		'End Code
	End Function
End Class
