REM LineNo: 13
REM ExpectedError: BC30201
REM ErrorMessage: Expression expected.

Option Strict On

Imports System

Module LikeOperatorC1
    Sub main()
        Dim a As Boolean

        a = "HELLO" Like 
        If a <> True Then
            Console.WriteLine("#A1-LikeOperator:Failed")
        End If
    End Sub

End Module
