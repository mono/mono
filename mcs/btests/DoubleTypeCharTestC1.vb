REM LineNo: 16
REM ExpectedError: BC30277
REM ErrorMessage: Type character '#' does not match declared data type 'System.Object'.

REM LineNo: 19
REM ExpectedError: BC30277
REM ErrorMessage: Type character '&' does not match declared data type 'Double'.

REM LineNo: 21
REM ExpectedError: BC30205
REM ErrorMessage: End of statement expected.

Module M
	Sub Main()
		Dim a 'Default type assigned is Object
		a#=10 'Double type character does not conform with assigned type Object
		
		Dim b As Double
		b&=10 'Long type character does not conform with assigned type Double

		Dim c #

		
	End Sub
End Module
