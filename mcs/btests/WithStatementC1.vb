' BC30456: 'a1' is not a member of 'WithStatementC1.C2'.
Imports System

Module WithStatementC1
    Class C1
        Friend a1 As String = "Hello"
    End Class

    Class C2
        Public b1 As String = " World"
    End Class

    Sub main()
        Dim a As New C1()
        With a
            .a1 = "I am in With Statement"
            Dim b As New C2()
            With b
                .b1 = "I am in nested With statement"
                .a1 = "I am also in nested With statement"
            End With
        End With

    End Sub

End Module