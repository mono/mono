REM LineNo: 19
REM ExpectedError: BC30512
REM ErrorMessage: Option Strict On disallows implicit conversions from 'Double' to 'Long'.

REM LineNo: 23
REM ExpectedError: BC30512
REM ErrorMessage: Option Strict On disallows implicit conversions from 'Decimal' to 'Integer'.

' BC30512: Option strict On disallows implicit conversions

Option Strict On
Imports System

Module ArithmeticOperatorsC2

    Sub main()
        Dim a1, a2 As Integer

        a1 = 12 \ 2.5

        Dim b1 As Decimal
        b1 = 34.45D
        a1 = b1 / 0

    End Sub

End Module