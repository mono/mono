Imports Microsoft.VisualBasic 
Imports System
Public Class TestClass 
	Public Function Test() As String
		'Begin Code
			dim result as String
			Dim fn As Integer = FreeFile
			FileOpen (1, "5253.txt", OpenMode.Random)
			FilePut (1, "Hello", 1)
			FilePut (1, "World", 2)
			FileClose (1)

			FileOpen(1, "5253.txt", OpenMode.Random)
			FileGet (1,result)
			Dim location = Loc (1)	
			FileClose (1)
			Return location
		'End Code
	End Function
End Class
