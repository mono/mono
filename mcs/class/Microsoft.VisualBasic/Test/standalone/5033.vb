Imports Microsoft.VisualBasic 
Imports System
Public Class TestClass 
	<VBFixedArray(3,4)>Dim a(,) As Integer = New Integer (,) {{5,6},{7,8}}
	Public Function Test() As Integer
		'Begin Code
			Return a (1,1)
		'End Code
	End Function
End Class
