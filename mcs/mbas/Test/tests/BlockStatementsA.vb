Imports System

Module BlockStatementsA

    Sub Main()
        Dim a As Integer = 10
        If a = 10 Then
            GoTo a
        End If

label:  a = 11

a:      a = 5
        If a = 5 Then
            GoTo 123
        End If

123:    a = 7
        If a = 7 Then
            GoTo _12ab
        End If

_12ab:  a = 8
        If a = 8 Then
            GoTo [class]
        End If

[class]: a = 0
        Console.WriteLine(a)

        ' label declaration always takes precedence in any ambiguous situation

f1:     Console.WriteLine("Heh") : a = 1 : f1:

    End Sub

    Function f1() As Boolean
        Console.WriteLine("Inside function f1()")
        Return True
    End Function

End Module