REM LineNo: 13
REM ExpectedError: BC30512
REM ErrorMessage: Option Strict On disallows implicit conversions from 'Double' to 'Long'.

Option Strict On
Imports System

Module ArithmeticOperatorsC2

    Sub main()
        Dim a1, a2 As Integer

        a1 = 12 \ 2.5

        Dim b1 As Decimal
        b1 = 34.45D
    End Sub

End Module