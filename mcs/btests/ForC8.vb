REM LineNo: 11
REM ExpectedError: BC30616
REM ErrorMessage: Variable 'i' hides a variable in an enclosing block.

Imports System

Module ForC8

    Sub main()
        For i As Integer = 0 To 10
            For i As Integer = 0 to 10
                Console.WriteLine("Hello World")
            Next
        Next
    End Sub

End Module