REM LineNo: 14
REM ExpectedError: BC30512
REM ErrorMessage: Option Strict On disallows implicit conversions from 'Double' to 'Long'.

Option Strict On

Imports System

Module LogicalOperatorsC1
    Sub main()

        Dim a1 As Integer = 1
        Dim b1 As Double = 10.02
        If a1 And b1 Then
           	Console.WriteLine ("Oops!")
        End If
    End Sub
End Module