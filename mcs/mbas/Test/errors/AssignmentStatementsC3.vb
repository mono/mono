REM LineNo: 17
REM ExpectedError: BC30512
REM ErrorMessage: Option Strict On disallows implicit conversions

Option Strict On
Imports System
Imports Microsoft.VisualBasic

Module AssignmentStatementsC3

    Sub main()

        Dim b As Byte = 0
        Dim i As Integer = 0

        b += 1000
        b += i

    End Sub

End Module
