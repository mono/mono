Imports Microsoft.VisualBasic 
Imports System
Public Class TestClass
	Public Function Test() As String
		'Begin Code
			'Ommiting optional values FV and Due
			Dim d As double = PV (0.5,3,0)	
			If (d <> 0)
				Throw New Exception ("#PV1")
			End If
		'End Code
	End Function
End Class