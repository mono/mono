REM LineNo: 14
REM ExpectedError: BC30070
REM ErrorMessage: Next control variable does not match For loop control variable 'j'.

Imports System

Module ForEachC6

    Sub main()
        Dim arr() As Integer = {1, 2, 3}
        For Each i As Integer in arr
            For Each j As Integer in arr
                Console.WriteLine("Hello World")
            Next i
        Next
    End Sub

End Module