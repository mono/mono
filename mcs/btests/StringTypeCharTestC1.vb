REM LineNo: 12
REM ExpectedError: BC30277
REM ErrorMessage: Type character '$' does not match declared data type 'System.Object'.

REM LineNo: 17
REM ExpectedError: BC30037
REM ErrorMessage: Character is not valid.

Module M
	Sub Main()
		Dim a 'Default type assigned is Object
		a$="Hello" 'String type character does not conform with assigned type Object
		
		Dim b As String
		b$=10L 'Long type character does not conform with assigned type String
		
		Dim c $

	End Sub
End Module
