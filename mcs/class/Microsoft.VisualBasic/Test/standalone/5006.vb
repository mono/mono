Imports Microsoft.VisualBasic 
Imports System
Public Class TestClass
	Public Function Test() As String
		'Begin Code
			'Ommiting optional values FV and Due
			Dim d As double = IPmt (10,2,3,7)
			If (d <> -69.4736842105263) 
				Throw New Exception ("#IPmt1")
			End If
			Return d
		'End Code
	End Function
End Class