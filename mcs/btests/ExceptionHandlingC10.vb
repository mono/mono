REM LineNo: 10
REM ExpectedError: BC30132
REM ErrorMessage: Label '14' is not defined

Imports System

Module ExceptionHandlingC10

    Function f1()
        On Error GoTo 14
        Dim i As Integer
        i = 1 / i
    End Function

    Sub Main()
        f1()
    End Sub

End Module

