' ErrorMessage: System.InvalidCastException: Cast from string "Hello " to type 'Double' 
' is not valid.

Imports System

Module AssignmentStatements4

    Sub main()
        Dim i As Integer = 0
        Dim str As String = "Hello "
        str += i
        Console.WriteLine(str)
    End Sub


End Module
