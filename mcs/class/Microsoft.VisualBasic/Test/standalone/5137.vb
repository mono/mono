Imports Microsoft.VisualBasic 
Imports System
Public Class TestClass 
	Public Function Test() As String
		'Begin Code
			Dim result As String = StrDup (5, "Wow!")
			If Len(result) <> 5 Then
				Throw New Exception ("#StrComp01: Expected 5 but got " + Len (result).ToString ())
			End If
			Return result
		'End Code
	End Function
End Class
