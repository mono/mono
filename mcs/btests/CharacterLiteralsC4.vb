REM LineNo: 9
REM ExpectedError: BC30648
REM ErrorMessage: String constants must end with a double quote.

Imports System
Module CharacterLiteral
	Sub Main()
		Try
			Dim f As Char="""c
					
		Catch e As Exception
		End Try
	End Sub
End Module
