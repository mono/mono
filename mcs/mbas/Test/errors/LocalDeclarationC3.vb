REM LineNo: 11
REM ExpectedError: BC30246
REM ErrorMessage: 'Static' is not valid on a local constant declaration.

Imports System

Module LocalDeclarationC2

    Sub main()
        Static Dim a2 As Integer = 11
        Static Const a3 As Integer = 12
    End Sub

End Module