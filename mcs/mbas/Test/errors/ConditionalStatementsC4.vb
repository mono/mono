REM LineNo: 14
REM ExpectedError: BC30081
REM ErrorMessage: 'If' must end with a matching 'End If'.

Imports System

Module ConditionalStatementsC4

    Sub Main()
        Dim i As Integer = 0

	if false then
		i = 1
	elseif true
		i = 2
		
    End Sub

End Module
