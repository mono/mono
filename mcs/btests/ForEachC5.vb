REM LineNo: 11
REM ExpectedError: BC30201
REM ErrorMessage: Expression expected

Imports System

Module ForEachC5

    Sub main()
        Dim i As Integer
        For Each i in
            Console.WriteLine("Hello World")
        Next

    End Sub

End Module