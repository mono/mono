Imports Microsoft.VisualBasic 
Imports System
Public Class TestClass 
	Public Function Test() As String
		'Begin Code
			SetAttr ("5218.txt", vbReadOnly Or vbHidden)
			Dim attr as FileAttribute = GetAttr ("5218.txt")
			Dim reqAttr = FileAttribute.Normal Or FileAttribute.Hidden
			If (attr and reqAttr) <> reqAttr Then
				Throw New Exception ("Attribute 'Normal or Hidden' not set properly")
			End If
			Return "success"
		'End Code
	End Function
End Class
