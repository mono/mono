REM LineNo: 11
REM ExpectedError: BC30311
REM ErrorMessage: Value of type 'Boolean' cannot be converted to 'Char'.

Imports System
Module CharacterLiteral
	Sub Main()
		Try
			Dim a As Char="R"c
			Dim b As Char="W"c
			Dim bl As Char=True
			Dim c As Char
			Dim d As Char
			c=a+b
		Catch e As Exception
		End Try
	End Sub
End Module
