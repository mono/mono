Imports Microsoft.VisualBasic 
Imports System
Public Class TestClass 
	Public Function Test() As Integer
		'Begin Code
			Dim a() As String = {"Hello","Hello","Hello","Hello","Hello"}
			Dim d(1) As String
			d =  Filter (a,"Helo") ' Default is supposed to be 'True' and CompareMethod.Binary
			If d.Length() <> 0 Then
				Throw New Exception("#Filter01: Expected 5 but got "+d.Length.ToString())
			End If
			Return d.Length
		'End Code
	End Function
End Class
