REM LineNo: 14
REM ExpectedError: BC30201
REM ErrorMessage: Expression expected.

Imports System

Module WithStatementC3
    Class C1
        Public a1 As Integer = 10
    End Class

    Sub main()
        Dim a As New C1()
        With 
            .a1 = 20
        End With

    End Sub

End Module