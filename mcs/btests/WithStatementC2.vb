REM LineNo: 21
REM ExpectedError: BC30756
REM ErrorMessage: 'GoTo labelA' is not valid because 'labelA' is inside a 'With' statement
REM 		   that does not contain  this statement.

Imports System

Module WithStatementC2
    Class C1
        Public a1 As Integer = 10
        Public a2 As String = "Hello"
        Sub f1()
            Console.WriteLine("Class C1: {0} {1}", a1, a2)
        End Sub
    End Class

    Sub main()
        Dim a As New C1()

        If a.a1 <> 10 Then
            GoTo labelA
        End If

        With a
            .a1 = 20
labelA:
            .a2 = "Hello World"
            .f1()
        End With

    End Sub

End Module