REM LineNo: 12
REM ExpectedError: BC30757
REM ErrorMessage: 'Goto labelA' is not valid because 'labelA' is inside a 'For'
REM               or 'For Each' statement that does not contain this statement.

Imports System

Module ForC1

    Sub main()
        Dim i As Integer = 0
        GoTo labelA

        For i = 0 To 10
labelA:
            i = 2
        Next
    End Sub

End Module