Imports Microsoft.VisualBasic 
Imports System
Public Class TestClass
	Public Function Test() As String
		'Begin Code
			'Ommiting optional values FV and Due
			Dim d As double = NPer (0.5,2000,1000)
			Return d
		'End Code
	End Function
End Class