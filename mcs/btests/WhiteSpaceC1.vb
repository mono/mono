REM LineNo: 24
REM ExpectedError: BC30205
REM ErrorMessage: End of statement expected.

REM LineNo: 25
REM ExpectedError: BC30451
REM ErrorMessage: Name 'Di' is not declared.

REM LineNo: 25
REM ExpectedError: BC30684
REM ErrorMessage: 'm' is a type and cannot be used as an expression.

REM LineNo: 25
REM ExpectedError: BC30800
REM ErrorMessage: Method arguments must be enclosed in parentheses.

REM LineNo: 25
REM ExpectedError: BC32017
REM ErrorMessage: Comma, ')', or a valid expression continuation expected.

Imports System
Module M
	Sub Main()
			Dim a As Integer=5	1
			Di	m b As String
	End Sub
End Module
