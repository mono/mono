REM LineNo: 16
REM ExpectedError: BC30277
REM ErrorMessage: Type character '@' does not match declared data type 'System.Object'.

REM LineNo: 19
REM ExpectedError: BC30277
REM ErrorMessage: Type character '&' does not match declared data type 'Decimal'.

REM LineNo: 21
REM ExpectedError: BC30037
REM ErrorMessage: Character is not valid.

Module M
	Sub Main()
		Dim a 'Default type assigned is Object
		a@=10 'Decimal type character does not conform with assigned type Object
		
		Dim b As Decimal
		b&=10 'Long type character does not conform with assigned type Decimal

		Dim c @
		
	End Sub
End Module
