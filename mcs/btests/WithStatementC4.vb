REM LineNo: 14
REM ExpectedError: BC30085
REM ErrorMessage: 'With' must end with a matching 'End With'.

Imports System

Module WithStatementC4
    Class C1
        Public a1 As Integer = 10
    End Class

    Sub main()
        Dim a As New C1()
        With a
            .a1 = 20

    End Sub

End Module