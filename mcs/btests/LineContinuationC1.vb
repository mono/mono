REM LineNo: 16
REM ExpectedError: BC30205
REM ErrorMessage: End of statement expected.

REM LineNo: 17
REM ExpectedError: BC30108
REM ErrorMessage: 'Integer' is a type, and so is not a valid expression.

REM LineNo: 17
REM ExpectedError: BC30287
REM ErrorMessage: '.' expected.

Imports System
Module M
	Sub Main()
		Dim a As_
			Integer
	End Sub
End Module
