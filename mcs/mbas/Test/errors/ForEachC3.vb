REM LineNo: 21
REM ExpectedError: BC30064
REM ErrorMessage: 'ReadOnly' variable cannot be the target of an assignment.

Imports System

Module ForEachC3

    Class C1
        Public ReadOnly index As Integer = 0

        Sub New()
            Dim arr() As Integer = {1, 2, 3}
            For Each index In arr
                Console.WriteLine("Hello World")
            Next
        End Sub

        Sub f()
            Dim arr() As Integer = {1, 2, 3}
            For Each index In arr
                Console.WriteLine(index)
            Next
        End Sub

    End Class

    Sub main()
        Dim c As New C1()
        c.f()
    End Sub

End Module