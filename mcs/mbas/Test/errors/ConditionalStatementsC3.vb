REM LineNo: 12
REM ExpectedError: BC30205
REM ErrorMessage: End if statement expected

Imports System

Module ConditionalStatementsC3

    Sub Main()
        Dim i As Integer = 0

	if true then i = 1 else i = 2 end if

    End Sub

End Module
