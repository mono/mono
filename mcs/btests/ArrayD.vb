Imports System
Imports Microsoft.VisualBasic

Module VariableC
    Dim a() As Integer = {1, 2, 3, 4, 5}

    Sub Main()
	dim c as Integer

	c = UBound(a, 1)
	c = LBound(a, 1)
	'c = UBound(a)
	'c = LBound(a)

    End Sub
End Module
