Imports Microsoft.VisualBasic 
Imports System
Public Class TestClass 
	Public Function Test() As Integer
		'Begin Code
			Dim a(1) As String
			Dim d() As String = Filter (a,"lo",False)
			If d.Length() <> 0 Then
				Throw New Exception("#Filter01")
			End If
			Return d.Length
		'End Code
	End Function
End Class
