REM LineNo: 18
REM ExpectedError: BC30451
REM ErrorMessage: Name 'j' is not declared

Imports System

Module ExceptionHandlingC6

    Sub Main()
        Dim i As Integer
        Try
            Dim j As Integer = 2
            i = j / i
            i = 3
            Console.WriteLine(i)
        Catch e As Exception When i = 0
            j = 3    ' Local varables from a Try block are not available in Catch block
            Console.WriteLine(e.Message)
        Finally
            j = 4    ' Local varables from a Try block are not available in Finally block
        End Try
    End Sub

End Module

