REM LineNo: 7
REM ExpectedError: BC31067
REM ErrorMessage: Method in a structure cannot be declared 'Protected' or 'Protected Friend'.

Structure S
	Dim a As String
	protected sub f(l as long)
	end sub
End Structure

Module M
	Sub Main()
		dim x as S
	End Sub
End Module
