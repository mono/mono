REM LineNo: 10
REM ExpectedError: BC30246
REM ErrorMessage: 'Dim' is not valid on a local constant declaration.

Imports System

Module LocalDeclarationC2

    Sub Main()
        Dim Const a1 As Integer = 10
    End Sub

End Module