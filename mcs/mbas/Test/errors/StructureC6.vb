REM LineNo: 7
REM ExpectedError: BC30435
REM ErrorMessage: Members in a Structure cannot be declared 'Protected'.

Structure S
	Dim a As String
	Protected Structure S1
		dim g as string
	End Structure
End Structure

Module M
	Sub Main()
		dim x as S
	End Sub
End Module
