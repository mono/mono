REM LineNo: 23
REM ExpectedError: BC32007
REM ErrorMessage: 'Integer' values cannot be converted to 'Char'. Use 'Microsoft.VisualBasic.ChrW' to interpret a numeric value as a Unicode character or first convert it to 'String' to produce a digit.

REM LineNo: 24
REM ExpectedError: BC30311
REM ErrorMessage: Value of type 'Boolean' cannot be converted to 'Char'.

REM LineNo: 25
REM ExpectedError: BC30311
REM ErrorMessage: Value of type 'Double' cannot be converted to 'Char'.

REM LineNo: 29
REM ExpectedError: BC30452
REM ErrorMessage: Operator '*' is not defined for types 'Char' and 'Char'.

Imports System
Module CharacterLiteral
	Sub Main()
		Try
			Dim a As Char="R"c
			Dim b As Char="W"c
			Dim i As Char=23
			Dim bl As Char=True
			Dim f As Char=1.23
			Dim c As Char
			Dim d As Char
			c=a+b
			d=a*b
			
		Catch e As Exception
		End Try
	End Sub
End Module
