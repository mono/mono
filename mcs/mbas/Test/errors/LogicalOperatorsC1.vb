REM LineNo: 15
REM ExpectedError: BC30512
REM ErrorMessage: Option Strict On disallows implicit conversions from 'Boolean' to 'Long'.


Option Strict On

Imports System

Module LogicalOperatorsC1
    Sub main()

        Dim a1 As Boolean = True
        Dim b1 As Long = 10S
        If a1 And b1 Then
            a1 = False
        End If
    End Sub
End Module