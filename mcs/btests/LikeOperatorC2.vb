REM LineNo: 11
REM ExpectedError: BC30201
REM ErrorMessage: Expression expected.

Option Strict On

Imports System
Module LikeOperatorC1
    Sub main()
        Dim a As Boolean
        a =  Like "H*O"
        If a <> True Then
            Console.WriteLine("#A2-LikeOperator:Failed")
        End If
    End Sub

End Module
