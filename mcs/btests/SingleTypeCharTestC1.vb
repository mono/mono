REM LineNo: 12
REM ExpectedError: BC30277
REM ErrorMessage: Type character '!' does not match declared data type 'System.Object'.

REM LineNo: 17
REM ExpectedError: BC30205
REM ErrorMessage: End of statement expected.

Module M
	Sub Main()
		Dim a 'Default type assigned is Object
		a!=10 'Single type character does not conform with assigned type Object
		
		Dim b As Single
		b!=10 'Long type character does not conform with assigned type Integer
		
		Dim c !

	End Sub
End Module
