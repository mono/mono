' BC30094: Label 'a' is already defined in the current method.

Imports System

Module BlockStatementsC2

    Sub Main()
        Dim a As Integer = 10
        If a = 10 Then
            GoTo b
a:          a = 11
b:          a = 12
        Else
            GoTo a
        End If
a:
        Console.WriteLine("Outside label")
    End Sub

End Module
