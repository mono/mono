REM LineNo: 16
REM ExpectedError: BC30384
REM ErrorMessage: 'Try' must end with a matching 'End Try'

REM LineNo: 16
REM ExpectedError: BC30030
REM ErrorMessage: Try must have at least one 'Catch' or a 'Finally'

Imports System

Module ExceptionHandlingC15

    Sub Main()

        Dim i As Integer = 0
        Try
            i = 1 / i

    End Sub

End Module

