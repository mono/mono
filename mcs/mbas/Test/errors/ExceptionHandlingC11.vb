REM LineNo: 13
REM ExpectedError: BC30544
REM ErrorMessage: Method cannot contain both a 'Try' statement and an 'On Error' 
REM               or 'Resume' statement.

Imports System

Module ExceptionHandlingC11

    Sub Main()
        Resume Next
        Dim i As Integer
        Try
            i = 1 / i
            Console.WriteLine(i)
        Catch e As Exception
            Console.WriteLine(e.Message)
        End Try
    End Sub

End Module

