Imports System


Module WithStatementB
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
            a.a1 = 20
            .a2 = "Hello World"
            Dim x As New C1()
            a = x  ' Tried reassiging the object inside With statement
            If .a1 = a.a1 Or .a2 = a.a2 Then
                Throw New Exception("#WS1 - With Statement failed")
            End If
            a.f1()
            .f1()
        End With

    End Sub

End Module