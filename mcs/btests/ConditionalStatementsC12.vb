REM LineNo: 16
REM ExpectedError: BC30072
REM ErrorMessage: 'Case' can only appear inside a 'Select Case' statement.

REM LineNo: 18
REM ExpectedError: BC30088
REM ErrorMessage: 'End Select' must be preceded by a matching 'Select Case'.


Imports System

Module ConditionalStatementsC12

    Sub Main()

		Case 0
		     Console.WriteLine("Hello World")
        End Select   

    End Sub

End Module