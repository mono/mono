Imports Microsoft.VisualBasic 
Imports System
Public Class TestClass 
	Public Function Test() As String
		'Begin Code
			Dim input as Boolean
			FileOpen (1, "5236.txt", OpenMode.Output)
			PrintLine (1, input) 'Should write 'False' in the file
		'End Code
	End Function
End Class
