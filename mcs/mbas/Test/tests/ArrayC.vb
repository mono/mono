Imports System

Module VariableC
    Dim a() As Integer = {1, 2, 3, 4, 5}

    Sub Main()
	ReDim Preserve a(10)
	
	a(7) = 8
	If a(7) <> 8 then
		Throw New Exception ("#A1, Unexpected result")
	End If
	
	If a(2) <> 3 then
		Throw New Exception ("#A2, Unexpected result - Preserve keyword not working")
	End If
    End Sub
End Module
