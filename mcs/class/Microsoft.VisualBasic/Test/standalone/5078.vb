Imports Microsoft.VisualBasic 
Imports System
Public Class TestClass 
	Public Function Test() As Integer
		'Begin Code
			Dim a() As String = {"Hello","Hello","Hello","Hello","Hello"}
			Dim d(1) As String
			d =  Filter (a,"lo",True)
			If d.Length() <> 5 Then
				Throw New Exception("#Filter01")
			End If
			Return d.Length
		'End Code
	End Function
End Class
