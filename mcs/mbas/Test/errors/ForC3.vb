REM LineNo: 19
REM ExpectedError: BC30064
REM ErrorMessage: 'ReadOnly' variable cannot be target of an assignment.

Imports System

Module ForC3

    Class C1
        Public ReadOnly index As Integer = 0

        Sub New()
            For index = 0 To 10
                Console.WriteLine("Hello World")
            Next
        End Sub

        Sub f()
            For index = 11 To 14
                Console.WriteLine("Hello World")
            Next
        End Sub

    End Class

    Sub main()
        Dim c As New C1()
        c.f()
    End Sub

End Module