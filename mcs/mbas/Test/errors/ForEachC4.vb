REM LineNo: 12
REM ExpectedError: BC30084
REM ErrorMessage: 'For' must end with a matching 'Next'

Imports System

Module ForEachC4

    Sub main()

        Dim arr() As Integer = {1, 2, 3}
        For Each i As Integer in arr
            Console.WriteLine("Hello World")

    End Sub

End Module