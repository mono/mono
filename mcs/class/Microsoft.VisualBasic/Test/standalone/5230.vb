Imports Microsoft.VisualBasic 
Imports System
Public Class TestClass 
	Public Function Test() As String
		'Begin Code
			dim result as String
			FileOpen(1, "5230.txt", OpenMode.Append)
			WriteLine (1, "Hello World")
			Seek (1, 1)
			WriteLine (1, "H")
			Dim length as Long = seek (1)
			If length <> 6 Then
				Throw New Exception ("Expected value is 6 but got " + length.ToString)
			End If
			return length
		'End Code
	End Function
End Class
