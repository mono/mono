
Option Strict On

Imports System

Module LikeOperator1
    Sub Main()

        Dim a As Boolean
        a = "HELLO" Like "H[A- Z][!M-P][!A-K]O"
        If a <> True Then
            Console.WriteLine("#A1-LikeOperator:Unexpected behaviour")
        End If

    End Sub
End Module
