REM LineNo: 20
REM ExpectedError: BC30512
REM ErrorMessage: Option Strict On disallows implicit conversions from 'Boolean' to 'Long'.

REM LineNo: 20
REM ExpectedError: BC30512
REM ErrorMessage: Option Strict On disallows implicit conversions from 'Double' to 'Long'.

' BC30512: Option Strict On disallows implicit conversions from 'Double' to 'Long'.

Option Strict On

Imports System

Module LogicalOperatorsC1
    Sub main()

        Dim a1 As Boolean = True
        Dim b1 As Double = 10.02
        If a1 And b1 Then
            a1 = False
        End If
    End Sub
End Module