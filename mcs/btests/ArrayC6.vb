REM LineNo: 10
REM ExpectedError: BC30689
REM ErrorMessage: Statement cannot appear outside of a method body.

Imports System

Module ArrayC6

    Dim a As Integer() = {1, 2, 3, 4}
    ReDim Preserve a(5)
    Erase a

    Sub Main()
        ReDim Preserve a(6)
        Erase a
        Console.WriteLine(a(0))
    End Sub
End Module
