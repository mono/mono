Imports Microsoft.VisualBasic 
Imports System
Public Class TestClass 
	Public Function Test() As String
		'Begin Code
			Dim result As String = Replace ("Hello World","World","", , ,CompareMethod.Binary)
			If Len (result) <> 6 Then
				Throw New Exception ("#Replace01: Expected 6 but got " + Len (result).ToString ())
			End If
			Return result
		'End Code
	End Function
End Class
