' BC30451: Name 'b' is not declared

Imports System

Module LocalVariablesC3

    Sub main()
        Dim a As Integer = 0
        If a <> 0 Then
            Dim b As Integer = 1
            Console.WriteLine(b)
        Else
            Dim b As Integer = 2
            Console.WriteLine(b)
        End If
        Console.WriteLine(b)
    End Sub

End Module