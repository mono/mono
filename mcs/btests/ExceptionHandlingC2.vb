REM LineNo: 9
REM ExpectedError: BC30030
REM ErrorMessage: Try must have atleast one 'Catch' or a 'Finally' 

Imports System

Module ExceptionHandlingC2
    Sub Main()
        Try
            Console.WriteLine("Exception in Main")
        End Try
    End Sub
End Module

