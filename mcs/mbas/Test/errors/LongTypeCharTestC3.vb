REM LineNo: 8
REM ExpectedError: BC30277
REM ErrorMessage: Type character '$' does not match declared data type 'Long'.

Module M
	Sub Main()
		Dim b As Long
		b$=10 'String type character does not conform with assigned type Long
	End Sub
End Module
