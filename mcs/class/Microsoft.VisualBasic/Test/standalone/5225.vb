Imports Microsoft.VisualBasic 
Imports System
Public Class TestClass 
	Public Function Test() As String
		'Begin Code
			dim result as String
			FileOpen(1, "5225.txt", OpenMode.Input)
			Input (1, result)
			Input (1, result)
			Dim length as Long = Seek (1)
			If length <> 13 Then
				Throw New Exception ("Expected value is 13 but got " + length.ToString)
			End If
			FileClose(1)
			return length
		'End Code
	End Function
End Class
