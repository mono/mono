'BC30132: Label 'c' is not defined

Imports System

Module BlockStatementsC3

    Sub Main()
        Dim a As Integer = 10
        If a = 10 Then
            GoTo b
a:          a = 11
b:          a = 12
        Else
            GoTo c
        End If
    End Sub

    Sub f()
        Console.WriteLine("Inside sub f()")
a:
        Console.WriteLine("label a")
c:
        Console.WriteLine("label c")

    End Sub
End Module
