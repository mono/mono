' BC30290: Local variable cannot have the same name as the function containing it.

Imports System

Module LocalVariablesC2

    Function f1(ByVal a As Integer) As Integer
        Dim f1 As Integer = 10
        f1 = f1 + a
    End Function

    Sub Main()
        Console.WriteLine(f1(0))
    End Sub

End Module