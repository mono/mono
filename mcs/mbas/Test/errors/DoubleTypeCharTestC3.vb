REM LineNo: 8
REM ExpectedError: BC30277
REM ErrorMessage: Type character '&' does not match declared data type 'Double'.

Module M
	Sub Main()
		Dim b As Double
		b&=10 'Long type character does not conform with assigned type Double
	End Sub
End Module
