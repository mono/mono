REM LineNo: 13
REM ExpectedError: BC30754
REM ErrorMessage: 'Goto label1' is not valid because 'label1' is inside a 'Try',
REM               'Catch' or 'Finally' statement that does not contain this statement.

Imports System

Module ExceptionHandlingC5
    Dim i As Integer
    Sub Main()
        Dim i As Integer = 0

        GoTo label1

        Try
            i = 1 / i
label1:     ' do something here
            i = 2 * i
            GoTo label2
        Catch e As Exception
label2:
            Console.WriteLine("Exception in Main: " & e.Message)
            GoTo label3
        Finally
label3:
            i = i + 2
        End Try
    End Sub

End Module