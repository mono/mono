Imports Microsoft.VisualBasic 
Imports System
Public Class TestClass
	<VBFixedArray(3)>Dim a() As Integer = {1,2}
	Public Function Test() As Integer
		'Begin Code
			Return a (1)
		'End Code
	End Function
End Class
