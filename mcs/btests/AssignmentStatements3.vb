' ErrorMessage:  System.OverflowException: Arithmetic operation resulted in an overflow.

Imports System

Module AssignmentStatements3

    Sub main()

        Dim b As Byte = 0
        b += 1000
        If b <> 1000 Then
            Throw New Exception("#AS3-Assignment Statement Failed")
        End If

    End Sub


End Module
