REM LineNo: 11
REM ExpectedError: BC32007
REM ErrorMessage: 'Integer' values cannot be converted to 'Char'. Use 'Microsoft.VisualBasic.ChrW' to interpret a numeric value as a Unicode character or first convert it to 'String' to produce a digit.

Imports System
Module CharacterLiteral
	Sub Main()
		Try
			Dim a As Char="R"c
			Dim b As Char="W"c
			Dim i As Char=23
			Dim c As Char
			Dim d As Char
			c=a+b
		Catch e As Exception
		End Try
	End Sub
End Module
