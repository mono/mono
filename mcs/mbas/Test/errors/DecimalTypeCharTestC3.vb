REM LineNo: 8
REM ExpectedError: BC30277
REM ErrorMessage: Type character '&' does not match declared data type 'Decimal'.

Module M
	Sub Main()
		Dim b As Decimal
		b&=10 'Long type character does not conform with assigned type Decimal
	End Sub
End Module
