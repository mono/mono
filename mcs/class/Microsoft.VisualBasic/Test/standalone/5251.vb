Imports Microsoft.VisualBasic 
Imports System
Public Class TestClass 
	Public Function Test() As Long
		'Begin Code	
			FileOpen (1, "5251.txt", OpenMode.Output)
			Return LOF (1)
		'End Code
	End Function
End Class
Public Class Sample
End Class
