REM LineNo: 21
REM ExpectedError: BC30801
REM ErrorMessage: Labels that are numbers must be followed by colons.

REM LineNo: 21
REM ExpectedError: BC30451
REM ErrorMessage: Name 'a' is not declared.

REM LineNo: 22
REM ExpectedError: BC30203
REM ErrorMessage: Identifier expected.

' BC30801: Labels that are numbers must be followed by colons
' BC30451: Name 'a' is not declared
' BC30203: Identifier expected

Imports System

Module BlockStatementsC4
	sub main
1a:
_#$%dd:
		Console.WriteLine("Invalid labels")
	end sub

End Module
