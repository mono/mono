' System.InvalidCastException: Cast from string "Hello World" to type 'Integer' is not valid.

Imports System

Module AssignmentStatements2

    Sub main()
        Dim a As Integer
        a = "Hello " + "World"
        Console.WriteLine(a)
    End Sub

End Module
