REM LineNo: 8
REM ExpectedError: BC30277
REM ErrorMessage: Type character '!' does not match declared data type 'System.Object'.

Module M
	Sub Main()
		Dim a 'Default type assigned is Object
		a!=10 'Single type character does not conform with assigned type Object
		
		Dim b As Single
		b!=10 'Long type character does not conform with assigned type Integer
	End Sub
End Module
