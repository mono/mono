REM LineNo: 12
REM ExpectedError: BC30451
REM ErrorMessage: Name 'b' is not declared.



Imports System

Module BlockStatementsC1
	sub main
		Dim a As Integer = 10
a: b:
		Console.WriteLine(a)
	end sub

End Module
