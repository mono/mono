REM LineNo: 15
REM ExpectedError: BC30451
REM ErrorMessage: Name 'j' is not declared

Imports System

Module ForC9

    Sub main()

        For i As Integer = 0 To 10
            For j As Integer = 0 to 10
                Console.WriteLine("Hello World")
            Next
            j = 2          ' scope check
        Next

    End Sub

End Module