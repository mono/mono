REM LineNo: 15
REM ExpectedError: BC30088
REM ErrorMessage: 'End Select' must be preceded by a matching 'Select Case'.

Imports System

Module ConditionalStatementsC9

    Sub Main()

        Dim i As Integer = 0

        Select i 
        End Select
	End Select

    End Sub

End Module