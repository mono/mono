REM LineNo: 16
REM ExpectedError: BC30087
REM ErrorMessage: 'End If' must be preceded by a matching 'If'.

Imports System

Module ConditionalStatementsC7

    Sub Main()

        Dim i As Integer = 0
	
	if i = 0 then
		i = 1
	end if
	end if
								
    End Sub

End Module
