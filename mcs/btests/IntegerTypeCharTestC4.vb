REM LineNo: 8
REM ExpectedError: BC30277
REM ErrorMessage: Type character '&' does not match declared data type 'Integer'.

Module M
	Sub Main()
		Dim b As Integer
		b&=10 'Long type character does not conform with assigned type Integer
	End Sub
End Module
