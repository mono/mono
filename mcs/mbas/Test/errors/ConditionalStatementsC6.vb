REM LineNo: 16
REM ExpectedError: BC30086
REM ErrorMessage: 'Else' must be preceded by a matching 'If' or 'ElseIf'.

Imports System

Module ConditionalStatementsC6

    Sub Main()
        Dim i As Integer = 0
	
	if i = 0 then
		i = 2
	else
		i = 3
	else
		i = 4
	End if 

    End Sub

End Module
