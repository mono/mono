REM LineNo: 10
REM ExpectedError: BC30468
REM ErrorMessage: Type declaration characters are not valid in this context.

REM LineNo: 12
REM ExpectedError: BC30037
REM ErrorMessage: Character is not valid.

Imports System
Namespace IntegerTypeCharTest%
	Module M
		Sub [sub]%()
		End Sub
		Sub Main()
		End Sub
	End Module
End Namespace
