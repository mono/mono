REM LineNo: 13
REM ExpectedError: BC30288
REM ErrorMessage: Local variable 'b' is already declared in the current block.

Module M
	Sub Main()
		Dim a As Long
		a=10

		Dim b&
		b&=20

		Dim b As Long
		b&=20
	End Sub
End Module
