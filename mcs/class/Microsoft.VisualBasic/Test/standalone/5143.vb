Imports Microsoft.VisualBasic 
Imports System
Public Class TestClass 
	Public Function Test() As String
		'Begin Code
			Dim result As String = FormatPercent ("-22.345578")	
			Console.WriteLine (result)
			If Len (result) <> 10 Then
				Throw New Exception ("Expected -2,234.56% but got " + result.ToString ())
			End If
		'End Code
	End Function
End Class
