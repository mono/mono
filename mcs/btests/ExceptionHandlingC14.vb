REM LineNo: 10
REM ExpectedError: BC30203
REM ErrorMessage: Identifier Expected

Imports System

Module ExceptionHandlingC14

    Sub Main()
        On Error  GoTo
        On Error Resume Next
    End Sub

End Module

