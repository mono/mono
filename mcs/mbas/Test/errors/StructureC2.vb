REM LineNo: 8
REM ExpectedError: BC30629
REM ErrorMessage: Structures cannot declare a non-shared 'Sub New' with no parameters.

Structure S
	Dim a as String

	Sub NEW()
	end Sub
End Structure

Module M
	Sub Main()
		dim x as S
	End Sub
End Module
