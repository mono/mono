REM LineNo: 29
REM ExpectedError: BC30246
REM ErrorMessage: 'Dim' is not valid on a local constant declaration.

REM LineNo: 31
REM ExpectedError: BC30246
REM ErrorMessage: 'Static' is not valid on a local constant declaration.

REM LineNo: 32
REM ExpectedError: BC30246
REM ErrorMessage: 'Static' is not valid on a local constant declaration.

REM LineNo: 32
REM ExpectedError: BC30246
REM ErrorMessage: 'Dim' is not valid on a local constant declaration.

REM LineNo: 33
REM ExpectedError: BC30182
REM ErrorMessage: Type expected.

' BC30246: 'Dim' is not valid on local const declaration
' BC30182: Type expected

Imports System

Module LocalDeclarationC2

    Sub main()
        Dim Const a1 As Integer = 10
        Static Dim a2 As Integer = 11
        Static Const a3 As Integer = 12
        Static Dim Const a4 As Integer = 13
        Dim a5 As  
    End Sub

End Module