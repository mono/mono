Imports Microsoft.VisualBasic 
Imports System
Public Class TestClass 
	Public Function Test() As String
		'Begin Code
			Dim result As String = FormatDateTime ("2/2/02 12:34:56", DateFormat.ShortTime)	
			If Len (result) > 5 Then
				Throw New Exception ("Expected 5 but got " + (Len (result)).ToString())
			End If
			return result
		'End Code
	End Function
End Class
