Imports Microsoft.VisualBasic 
Imports System
Public Class TestClass
	Public Function Test() As String
		'Begin Code
			'Ommiting optional values FV, Due and guess
			Dim d As double = Financial.Rate (1,3,100)	
			Return d
		'End Code
	End Function
End Class