REM LineNo: 14
REM ExpectedError: BC30452
REM ErrorMessage: Operator '*' is not defined for types 'Char' and 'Char'.

Imports System
Module CharacterLiteral
	Sub Main()
		Try
			Dim a As Char="R"c
			Dim b As Char="W"c
			Dim c As Char
			Dim d As Char
			c=a+b
			d=a*b
			
		Catch e As Exception
		End Try
	End Sub
End Module
