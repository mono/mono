REM LineNo: 5
REM ExpectedError: BC30281
REM ErrorMessage: Structure 'S' must contain at least one instance member variable or Event declaration.

Structure S
End Structure

Module M
	Sub Main()
		dim x as S
	End Sub
End Module
