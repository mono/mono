Imports Microsoft.VisualBasic 
Imports System
Public Class TestClass 
	Public Function Test() As Double
		'Begin Code
			Dim a As String = Nothing
			Try
				Dim d As double = Asc (a)
				Throw New Exception ("#Asc01")
			Catch e As Exception
				If (e.GetType ().ToString ()) <> "System.ArgumentException" Then
					Throw New Exception ("#Asc01: Expected System.ArgumentException but got " + e.GetType ().ToString ())
				End If
			End Try
		'End Code
	End Function
End Class
