REM LineNo: 12
REM ExpectedError: BC30451
REM ErrorMessage: Name 'IsArray' is not declared.

Imports System

Module VariableC
    Dim a() As Integer ' = {1, 2, 3, 4, 5}

    Sub Main()
	dim arry as boolean
	arry = IsArray(a)
	If arry <> true then
		Throw New Exception ("#A1, Not an Array")
	End If
	

	ReDim Preserve a(10)
	Console.WriteLine(a(7))

    End Sub
End Module
