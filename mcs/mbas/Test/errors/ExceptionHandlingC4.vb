REM LineNo: 9
REM ExpectedError: BC30665
REM ErrorMessage: 'Throw' operand must derive from 'System.Exception'

Imports System

Module ExceptionHandlingC4

    Sub Main()
        Dim i As Integer
        Throw i
    End Sub

End Module

