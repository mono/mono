REM LineNo: 12
REM ExpectedError: BC30512
REM ErrorMessage: Option Strict On disallows implicit conversions from 'Double' to 'Long'.

Option Strict On
Imports System

Module ShiftOperatorsC1

    Sub Main()
        Dim a1 As Double = 200.93
        a1 = a1 << 10
        Console.WriteLine(a1)

    End Sub

End Module