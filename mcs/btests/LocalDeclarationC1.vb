REM LineNo: 17
REM ExpectedError: BC30438
REM ErrorMessage: Constants must have a value.

REM LineNo: 19
REM ExpectedError: BC30074
REM ErrorMessage: Constant cannot be the target of an assignment.

' BC30438: Constants must have a value
' BC30074: Constant cannot be the target of an assinment.

Imports System

Module LocalDeclarationC1

    Sub main()
        Const a As Integer
        Const b As Integer = 20
        b = b + 2
    End Sub

End Module