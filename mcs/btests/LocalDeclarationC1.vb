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