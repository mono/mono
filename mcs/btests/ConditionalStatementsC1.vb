REM LineNo: 16
REM ExpectedError: BC30081
REM ErrorMessage: 'If' must end with a matching 'End If'.

REM LineNo: 16
REM ExpectedError: BC30205
REM ErrorMessage: End of statement expected.

Imports System

Module ConditionalStatementsC1

    Sub Main()
        Dim i As Integer = 0

        If i = 0  i =2

    End Sub

End Module