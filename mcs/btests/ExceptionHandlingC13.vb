REM LineNo: 10
REM ExpectedError: BC32019
REM ErrorMessage: 'Resume' or 'Goto' expected

Imports System

Module ExceptionHandlingC13

    Sub Main()
        On Error
        On Error Resume Next
    End Sub

End Module

