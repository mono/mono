REM LineNo: 17
REM ExpectedError: BC30071
REM ErrorMessage: 'Case Else' can only appear inside a 'Select Case' statement.

REM LineNo: 19
REM ExpectedError: BC30088
REM ErrorMessage: 'End Select' must be preceded by a matching 'Select Case'.

Imports System

Module ConditionalStatementsC13

    Sub Main()

        Dim i As Integer = 0
        
                Case Else
                    Console.WriteLine("Hello World")
        End Select   

    End Sub

End Module