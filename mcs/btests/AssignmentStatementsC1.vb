REM LineNo: 11
REM ExpectedError: BC30311
REM ErrorMessage: Value of type 'Date' cannot be converted to 'Integer'.

Imports System

Module AssignmentStatementsC1

    Sub main()
        Dim a As Integer
        a = New Date(200)
        Console.WriteLine(a)

    End Sub

End Module
