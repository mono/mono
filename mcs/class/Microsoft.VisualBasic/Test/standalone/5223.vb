Imports Microsoft.VisualBasic 
Imports System
Public Class TestClass 
	Public Function Test() As Long
		'Begin Code
			dim result as String
			FileOpen(1, "5223.txt", OpenMode.Random)
			FileGet (1, result, 10)
			Return seek (1)
		'End Code
	End Function
End Class
