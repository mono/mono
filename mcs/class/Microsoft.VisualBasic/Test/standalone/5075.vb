Imports Microsoft.VisualBasic 
Imports System
Public Class TestClass 
	Public Function Test() As Integer
		'Begin Code
			Dim a(2) As String
			a(0) = "Hello"
			a(1) = "Whole"
			a(2) = "World"
			Dim d() As String = Filter (a,"lo",False,CompareMethod.Binary)
			Console.WriteLine (d.Length)
			If d.Length() <> 2 Then
				Throw New Exception("#Filter01")
			End If
		'End Code
	End Function
End Class
