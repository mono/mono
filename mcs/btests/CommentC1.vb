REM LineNo: 19
REM ExpectedError: BC30451
REM ErrorMessage: Name 'Console' is not declared.

REM LineNo: 19
REM ExpectedError: BC30451
REM ErrorMessage: Name 'a' is not declared.

REM LineNo: 20
REM ExpectedError: BC30451
REM ErrorMessage: Name 'comment' is not declared.

REM LineNo: 21
REM ExpectedError: BC30205
REM ErrorMessage: End of statement expected.

Module M
	Sub Main()
		Console.WriteLine(a) 'Line Continuation within _
						 comment
		Dim a As IntegerREM Declaration
	End Sub
End Module
