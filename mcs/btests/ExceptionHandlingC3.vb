REM LineNo: 11
REM ExpectedError: BC30380
REM ErrorMessage: 'Catch' cannot appear outside a 'Try' statement.

Imports System

Module ExceptionHandlingC3

    Sub Main()

        Catch e As Exception
            Console.WriteLine(e.Message)
        End Try
    End Sub

End Module

