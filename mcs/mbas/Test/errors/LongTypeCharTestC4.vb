REM LineNo: 7
REM ExpectedError: BC30302
REM ErrorMessage: Type character '&' cannot be used in a declaration with an explicit type.

Module M
	Sub Main()
		Dim b& As Long
		b=20
	End Sub
End Module
