Imports Microsoft.VisualBasic 
Imports System
Public Class TestClass 
	<VBFixedArray(3)>Dim a() As Integer = {1,2,3,4,5,6,7,8,9}
	Public Function Test() As Integer
		'Begin Code
			Return a (7)
		'End Code
	End Function
End Class
