Imports Microsoft.VisualBasic 
Imports System
Public Class TestClass 
	Public Function Test() As String
		'Begin Code
			Dim result As String = LSet ("   Hello     ",10)
			Console.WriteLine (result)
			If Len (result) <> 10 Then
				Throw New Exception ("#LSet01: Expected 10 but got " + Len (result).ToString ())
			End If
			Return result
		'End Code
	End Function
End Class
