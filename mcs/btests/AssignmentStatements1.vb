' ErrorMessage: System.ArrayTypeMismatchException: Attempted to store 
' an element of the incorrect type into the array.

Imports System

Module AssignmentStatements1

    Sub main()

        Dim sa(10) As String
        Dim oa As Object() = sa
        oa(0) = Nothing
        oa(1) = "Hello "
        oa(2) = "World"
        oa(3) = New Date(2004, 8, 17)

    End Sub


End Module
