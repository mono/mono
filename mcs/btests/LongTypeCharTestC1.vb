REM LineNo: 16
REM ExpectedError: BC30277
REM ErrorMessage: Type character '&' does not match declared data type 'System.Object'.

REM LineNo: 19
REM ExpectedError: BC30277
REM ErrorMessage: Type character '$' does not match declared data type 'Long'.

REM LineNo: 21
REM ExpectedError: BC30205
REM ErrorMessage: End of statement expected.

Module M
	Sub Main()
		Dim a 'Default type assigned is Object
		a&=10 'Long type character does not conform with assigned type Object
		
		Dim b As Long
		b$=10 'String type character does not conform with assigned type Long

		Dim c &
	End Sub
End Module
