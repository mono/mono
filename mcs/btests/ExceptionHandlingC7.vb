REM LineNo: 13
REM ExpectedError: BC30201
REM ErrorMessage: Expression expected

Imports System

Module ExceptionHandlingC7

    Sub Main()
        Dim i As Integer
        Try
            i = 1 / i
        Catch e As Exception When
            Console.WriteLine(e.Message)
        End Try
    End Sub

End Module

