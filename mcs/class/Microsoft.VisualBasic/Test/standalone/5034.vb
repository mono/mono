Imports Microsoft.VisualBasic 
Imports System
Public Class TestClass 
	<VBFixedArray(1,4)>Dim a(,) As Integer = New Integer (,) {{5,6,7,8,9,10},{7,8,9,10,11,12},{70,80,90,100,11,12}}
	Public Function Test() As Integer
		'Begin Code
			Return a (2,5)
		'End Code
	End Function
End Class
