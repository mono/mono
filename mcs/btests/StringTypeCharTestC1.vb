REM LineNo: 8
REM ExpectedError: BC30277
REM ErrorMessage: Type character '$' does not match declared data type 'System.Object'.

Module M
	Sub Main()
		Dim a 'Default type assigned is Object
		a$="Hello" 'String type character does not conform with assigned type Object
		
		Dim b As String
		b$=10L 'Long type character does not conform with assigned type String
	End Sub
End Module
