Imports Microsoft.VisualBasic 
Imports System
Public Class TestClass 
	Public Function Test() As String
		'Begin Code
			Dim result As String = FormatNumber ("-22.345578", 10,,TriState.True,TriState.True)	
			Console.WriteLine (result)
			If Len (result) <> 15 Then
				Throw New Exception ("Expected (22.3455780000) but got " + result.ToString())
			End If
		'End Code
	End Function
End Class
