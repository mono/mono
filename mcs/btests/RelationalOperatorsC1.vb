REM LineNo: 17
REM ExpectedError: BC30201
REM ErrorMessage: Expression expected.

REM LineNo: 18
REM ExpectedError: BC30201
REM ErrorMessage: Expression expected.

' BC30201: Expression expected

Imports System

Module RelationalOperatorsC1
    Sub Main()

        Dim a As Boolean
        a = 2 < 
        a =  <= 3
        a = 2 > True
        Console.WriteLine(a)
    End Sub
End Module
