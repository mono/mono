REM LineNo: 10
REM ExpectedError: BC30438
REM ErrorMessage: Constants must have a value.

Imports System

Module LocalDeclarationC1

    Sub Main()
        Const a As Integer
    End Sub

End Module