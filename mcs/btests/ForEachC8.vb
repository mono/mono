REM LineNo: 15
REM ExpectedError: BC30451
REM ErrorMessage: Name 'j' is not declared

Imports System

Module ForEachC8

    Sub main()
        Dim arr() As Integer = {1, 2, 3}
        For Each i As Integer in arr
            For Each j As Integer in arr
                Console.WriteLine("Hello World")
            Next
            j = 2          ' scope check
        Next
    End Sub

End Module