REM LineNo: 12
REM ExpectedError: BC30201
REM ErrorMessage: Expression expected

Imports System

Module ExceptionHandlingC16

    Sub Main()

        On Error  GoTo ErrorHandler
        Error
	Exit Sub
ErrorHandler:
	Console.WriteLine("Error Handler")

    End Sub

End Module

