Imports Microsoft.VisualBasic 
Imports System
Public Class TestClass 
	Public Function Test() As Char
		'Begin Code
			Dim a As String = "Hello"
			Try
				Dim d As Char = ChrW (a)
				Throw new Exception ("#Chr01")
			Catch e As Exception
				If (e.GetType ().ToString ())	<> "System.InvalidCastException" Then
					Throw new Exception ("#Chr01: expeced InvalidCastException but got " + e.GetType ().ToString ())
				End If
			End Try
			
		'End Code
	End Function
End Class
