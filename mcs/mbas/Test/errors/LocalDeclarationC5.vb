REM LineNo: 12
REM ExpectedError: BC30074
REM ErrorMessage: Constant cannot be the target of an assignment.

Imports System

Module LocalDeclarationC1

    Sub Main()
        Const a As Integer = 10
        Const b As Integer = 20
        b = a + b
    End Sub

End Module