Imports Microsoft.VisualBasic 
Imports System
Public Class TestClass
	Public Function Test() As String
		'Begin Code
			'Ommiting optional values FV and Due
			Dim d As double = PPmt (0.5,3,4,-100)	
			Return d
		'End Code
	End Function
End Class