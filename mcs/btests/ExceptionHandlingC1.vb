REM LineNo: 12
REM ExpectedError: BC30441
REM ErrorMessage: 'Catch' must end with a matching 'End Try'

Imports System

Module ExceptionHandlingC1

    Sub Main()
        Try
            'Do something
        Catch e As Exception
            Console.WriteLine(e.Message)

    End Sub

End Module

