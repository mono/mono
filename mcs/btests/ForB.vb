REM LineNo: 19
REM ExpectedError: BC30064
REM ErrorMessage: 'ReadOnly' variable cannot be target of an assignment.

Imports System

Module ForB

    Class C1
        Public ReadOnly index As Integer = 0

        Sub New()
            For index = 0 To 2
                Console.WriteLine(index)
            Next
        End Sub

    End Class

    Sub main()
        Dim c As New C1()
        If c.index <> 3 Then
            Throw New Exception("#ForB1")
        End If
    End Sub

End Module