REM LineNo: 13
REM ExpectedError: BC30311
REM ErrorMessage: Value of type 'Char' cannot be converted to 'Boolean'.

Imports System

Module ConditionalStatementsC5

    Sub Main()
        
	Dim i As Integer = 0

	if "a"c then
	end if
	
    End Sub

End Module
