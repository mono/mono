Imports Microsoft.VisualBasic 
Imports System
Public Class TestClass 
	Public Function Test() As String
		'Begin Code
			dim result as String
			FileOpen (1, "5229.txt", OpenMode.Binary)
			FilePut (1, "Hello", 1)
			FilePut (1, "World", 6)
			FileClose (1)

			FileOpen(1, "5229.txt", OpenMode.Binary)
			seek (1, 2)	
			FileGet (1, result)
			return result
		'End Code
	End Function
End Class
