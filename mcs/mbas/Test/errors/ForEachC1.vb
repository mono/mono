REM LineNo: 12
REM ExpectedError: BC30757
REM ErrorMessage: 'Goto labelA' is not valid because 'labelA' is inside a 'For'
REM               or 'For Each' statement that does not contain this statement.

Imports System

Module ForEachC1

    Sub Main()
        Dim arr() As Integer = {1, 2, 3}
        GoTo labelA

        For Each i As Integer in arr 
labelA:
            Console.WriteLine(i)
        Next

    End Sub

End Module