REM LineNo: 14
REM ExpectedError: BC30442
REM ErrorMessage: 'Finally' must end with a matching 'End Try'

Imports System

Module ExceptionHandlingC8

    Sub Main()

        Dim i As Integer = 0
        Try
            i = 1 / i
        Finally

    End Sub

End Module

