Imports System

Module WithStatementA
    Class C1
        Public a1 As Integer = 10
        Friend a2 As String = "Hello"
        Sub f1()
            Console.WriteLine("Class C1: {0} {1}", a1, a2)
        End Sub
    End Class

    Sub main()
        Dim a As New C1()
        With a
            .a1 = 20
            .a2 = "Hello World"
            .f1()
            Dim x As New C1()
            x.a1 = 2
            With x
                .a1 = 3
                .a2 = "In nested With statement"
                .f1()
                a.a1 = 25
                a.a2 = "Me too"
                a.f1()
            End With
        End With

        With a     ' Empty With statement
        End With

    End Sub

End Module