REM LineNo: 12
REM ExpectedError: BC30003
REM ErrorMessage: 'Next' expected

Imports System

Module ExceptionHandlingC16

    Sub Main()

        On Error  GoTo ErrorHandler
        On Error Resume
	Exit Sub
ErrorHandler:
	Console.WriteLine("Error Handler")

    End Sub

End Module

