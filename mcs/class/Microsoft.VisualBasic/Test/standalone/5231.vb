Imports Microsoft.VisualBasic 
Imports System
Public Class TestClass 
	Public Function Test() As String
		'Begin Code
			Rmdir ("new_dir")	
			return "success"
		'End Code
	End Function
End Class
