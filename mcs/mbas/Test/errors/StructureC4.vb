REM LineNo: 7
REM ExpectedError: BC30435
REM ErrorMessage: Members in a Structure cannot be declared 'Protected'.

Structure S
	Dim a as String
	Protected Const b as integer = 10
End Structure

Module M
	Sub Main()
		dim x as S
	End Sub
End Module
