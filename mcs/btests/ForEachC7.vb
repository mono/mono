REM LineNo: 12
REM ExpectedError: BC30616
REM ErrorMessage: Variable 'i' hides a variable in an enclosing block.

Imports System

Module ForEachC7

    Sub main()
        Dim arr() As Integer = {1, 2, 3}
        For Each i As Integer in arr
            For Each i As Integer in arr
                Console.WriteLine("Hello World")
            Next
        Next
    End Sub

End Module