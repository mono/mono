Imports Microsoft.VisualBasic 
Imports System
Public Class TestClass
	Public Function Test() As String
		'Begin Code
			'Ommiting optional value Due
			Dim d As double = FV (10,5,3)
			If (d <> -48315)
				Throw New Exception ("#FV1")
			End If
		'End Code
	End Function
End Class