REM LineNo: 7
REM ExpectedError: BC31047
REM ErrorMessage: Protected types can only be declared inside of a class.

Structure S
	Dim a As String
	Protected Class c
	end class
End Structure

Module M
	Sub Main()
		dim x as S
	End Sub
End Module
