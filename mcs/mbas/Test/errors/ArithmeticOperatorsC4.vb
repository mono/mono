REM LineNo: 13
REM ExpectedError: BC30542
REM ErrorMessage: Division by zero occurred while evaluating this expression.

Imports System

Module ArithmeticOperatorsC1

    Sub main()
        Dim a1, a2 As Integer
        Dim b1 As Double

        a1 = 10 \ Nothing
        Console.WriteLine(a1)

        b1 = 12.35 / 0
        Console.WriteLine(b1)
    End Sub

End Module