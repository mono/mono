REM LineNo: 13
REM ExpectedError: BC30801
REM ErrorMessage: Labels that are numbers must be followed by colons.

REM LineNo: 13
REM ExpectedError: BC30451
REM ErrorMessage: Name 'a' is not declared.

Imports System

Module BlockStatementsC4
	sub main
1a:
		Console.WriteLine("Invalid labels")
	end sub

End Module
