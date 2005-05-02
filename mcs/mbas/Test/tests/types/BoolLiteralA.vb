Option Strict Off
Imports System
Module M
	Sub Main()
		Dim a As Boolean=True
		Dim b As Boolean=False
		Dim c As Boolean
		c=a+b
		If a<>True
			Throw New Exception("BoolLiteralB:Failed")
		End If	
	End Sub
End Module
