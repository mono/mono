REM LineNo: 9
REM ExpectedError: BC30203
REM ErrorMessage: Identifier expected.

Imports System

Module BlockStatementsC4
	sub main
_#$%dd:
		Console.WriteLine("Invalid labels")
	end sub

End Module
