REM LineNo: 18
REM ExpectedError: BC30512
REM ErrorMessage: Option Strict On disallows implicit conversions from 'Double' to 'Long'.

REM LineNo: 18
REM ExpectedError: BC30512
REM ErrorMessage: Option Strict On disallows implicit conversions from 'Double' to 'Integer'.

'BC30512: Option Strict On disallows implicit conversions from double to long

Option Strict On
Imports System

Module ShiftOperatorsC1

    Sub Main()
        Dim a1 As Double = 200.93
        a1 = a1 << 109.95
        Console.WriteLine(a1)

    End Sub

End Module