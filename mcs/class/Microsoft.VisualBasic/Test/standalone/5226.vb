Imports Microsoft.VisualBasic 
Imports System
Public Class TestClass 
	Public Function Test() As String
		'Begin Code
			dim result as String
			FileOpen(1, "5226.txt", OpenMode.Output)
			WriteLine (1, "Hello World")
			Dim length as Long = Seek (1)
			If length <> 16 Then
				Throw New Exception ("Expected value is 16 but got " + length.ToString)
			End If
			return length
		'End Code
	End Function
End Class
