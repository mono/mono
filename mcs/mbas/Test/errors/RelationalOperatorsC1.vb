REM LineNo: 11
REM ExpectedError: BC30201
REM ErrorMessage: Expression expected.

Imports System

Module RelationalOperatorsC1
    Sub Main()

        Dim a As Boolean
        a = 2 < 
        a = 2 > True
        Console.WriteLine(a)
    End Sub
End Module
